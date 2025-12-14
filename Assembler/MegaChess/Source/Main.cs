
namespace Assembler.MegaChess;

using static Constants;
using static Register;

internal sealed class Main
{
    public Main() => m_Calculate = new (m_Globals);

    public sealed record Globals(int CursorSquareIndex = 1,
                                 int SelectedSquareIndex = 1);

    public static readonly Globals Vars =
        Variables.ByteSizesToOffsets(new Globals());

    public Assembly Build() => new Assembly()
        .Append(Preamble)
        .Append(m_Calculate.Build)
        .Append(m_DrawPiece.Build)
        .Append(m_DrawBoard.Build)
        .DefineGlobals(m_Globals, Vars, fillByte: 0)
        .NoOp(cycles: 3)
        .DeclareReference(out var returnFromDrawBoard)
        .DefineReference(m_Start, a => a
            .SetWordValue(R0, 0x8000)
            .StackFromR0()
            .CallRoutine(m_Calculate.Refs.Reset)
            .SetByteValue(R1, SquareIndex.E1)
            .CopyByteTo(m_Globals + Vars.CursorSquareIndex, R1)
            .CopyByteTo(m_Globals + Vars.SelectedSquareIndex, R1)
            .SetWordValue(R0, returnFromDrawBoard, force: true)
            .GoTo(m_DrawBoard.Refs.Draw, forceAbsolute: true))
        .DefineReference(returnFromDrawBoard, a => a
            .NoOp()
            .Loop(InfiniteLoop));

    private Assembly Preamble(Assembly a) => a
        .GoTo(m_Start, forceAbsolute: true)
        .NoOp()
        .Repeat(3, (_, a) => a.ReturnFromInterrupt()
                              .NoOp(cycles: 3));

    private static Assembly InfiniteLoop(Reference loop, Assembly a) => a
        .NoOp()
        // TODO
        .GoTo(loop, forceAbsolute: true)
        ;

    private readonly Reference m_Start = new (),
                               m_Globals = new ();

    private readonly Calculate m_Calculate;

    private readonly Draw.Board m_DrawBoard = new ();

    private readonly Draw.Piece m_DrawPiece = new ();
}