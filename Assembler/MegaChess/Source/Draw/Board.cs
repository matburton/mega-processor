
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
        .BlitBitmap(R2, R0, UseXAndJoystick)
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

    private static IEnumerable<string> UseXAndJoystick =>
        ["                                ",
         "████████████████████████████████",
         "                                ",
         " █ █ ██ ██  █   █  ███ █  █ ██  ",
         " █ █ █  █    █ █   █ █ ██ █ █ █ ",
         " █ █ ██ ██    █    ███ █ ██ █ █ ",
         " █ █  █ █    █ █   █ █ █  █ █ █ ",
         " ███ ██ ██  █   █  █ █ █  █ ██  ",
         "                                ",
         "                                ",
         "   █ ███ █ █ ██ ███ █ ██ █ █    ",
         "   █ █ █  █  █   █  █ █  ██     ",
         "   █ █ █  █  ██  █  █ █  █ █    ",
         "   █ █ █  █   █  █  █ █  █ █    ",
         "  ██ ███  █  ██  █  █ ██ █ █    ",
         "                                "];

    private sealed record Locals(int ReturnAddress = 2,
                                 int LoopRankIndex = 1,
                                 int LoopFileIndex = 1,
                                 int SquareIndex = 1);

    private readonly Locals m_Vars =
        Variables.ByteSizesToOffsets(new Locals());

    private readonly Reference m_Locals = new ();
}