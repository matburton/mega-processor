
namespace Assembler.MegaChess.Draw;

using C = Condition;

using static Register;

internal sealed class Board(Reference m_DrawPiece)
{
    public sealed class References
    {
        public Reference Draw { get; } = new ();
    };

    public References Refs { get; } = new ();

    public Assembly Build(Assembly a) => a
        .DefineGlobals(m_Locals, m_Vars, fillByte: 0)
        .NoOp(cycles: 3)
        .DefineReference(Refs.Draw)
        .CopyWordTo(m_Locals + m_Vars.ReturnAddress, R0)
        .SetWordValue(R2, 0xA000)
        .SetWordValue(R0, 0b0000000000000000, force: true)
        .Repeat(16 * 6, (_, a) => a.CopyWordToIndex(R2, R0, bumpIndex: true))
        .SetWordValue(R0, 0b0000000000000000, force: true)
        .Repeat(2, (_, a) => a.CopyWordToIndex(R2, R0, bumpIndex: true))
        .Repeat(m_UseXAndJoystick.Count, (index, a) => a
            .SetWordValue(R0, m_UseXAndJoystick[index].Bitmap, force: true)
            .Repeat(m_UseXAndJoystick[index].Repeats, (_, a) => a
                .CopyWordToIndex(R2, R0, bumpIndex: true)))
        .SetByteValue(R0, 8)
        .CopyByteTo(m_Locals + m_Vars.LoopRankIndex, R0)
        .CopyByteTo(m_Locals + m_Vars.LoopFileIndex, R0)
        .SetByteValue(R1, Constants.SquareIndex.A8)
        .Loop((loop, a) => a
            .CopyByteTo(m_Locals + m_Vars.LoopRankIndex, R0)
            .CopyByteTo(m_Locals + m_Vars.SquareIndex, R1)
            .DeclareReference(out var returnFromDrawPiece)
            .SetWordValue(R0, returnFromDrawPiece)
            .GoTo(m_DrawPiece)
            .DefineReference(returnFromDrawPiece)
            .CopyByteFrom(m_Locals + m_Vars.SquareIndex, R1)
            .AddConst(R1, 1)
            .CopyByteFrom(m_Locals + m_Vars.LoopRankIndex, R0)
            .AddConst(R0, -1)
            .GoToIf(C.NotEqual, loop)
            .SetByteValue(R0, 8)
            .AddConst(R1, 2)
            .CopyByteFrom(m_Locals + m_Vars.LoopFileIndex, R2)
            .AddConst(R2, -1)
            .CopyByteTo(m_Locals + m_Vars.LoopFileIndex, R2)
            .GoToIf(C.NotEqual, loop))
        .CopyWordFrom(m_Locals + m_Vars.ReturnAddress, R0)
        .GoToR0();

    // TODO: ASCII to bitmap
    private readonly IReadOnlyList<(int Bitmap, int Repeats)>
        m_UseXAndJoystick =
    [
        (0b0000000000000000,2),
        (0b1111111111111111,2),
        (0b0000000000000000,2),
        (0b0001001101101010,1),
        (0b0011010010111001,1),
        (0b1010000100101010,1),
        (0b0101010110101000,1),
        (0b0100001101101010,1),
        (0b0101011010111000,1),
        (0b1010000101001010,1),
        (0b0101010010101000,1),
        (0b0001001101101110,1),
        (0b0011010010101001,1),
        (0b0000000000000000,4),
        (0b0110101011101000,1),
        (0b0000101011010111,1),
        (0b0010010010101000,1),
        (0b0000011001010010,1),
        (0b0110010010101000,1),
        (0b0000101001010010,1),
        (0b0100010010101000,1),
        (0b0000101001010010,1),
        (0b0110010011101110,1),
        (0b0000101011010010,1),
        (0b0000000000000000,2)
    ];

    private sealed record Locals(int ReturnAddress = 2,
                                 int LoopRankIndex = 1,
                                 int LoopFileIndex = 1,
                                 int SquareIndex = 1);

    private readonly Locals m_Vars =
        Variables.ByteSizesToOffsets(new Locals());

    private readonly Reference m_Locals = new ();
}