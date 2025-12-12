
namespace Assembler.MegaChess;

using static Constants;
using static Register;

internal sealed class Main
{
    public Assembly Build() => new Assembly()
        .Append(Preamble)
        .Append(m_Calculate.Build)
        .Append(m_DrawPiece.Build)
        .Append(m_DrawBoard.Build)
        .DefineGlobals(out var globals, Vars, fillByte: 0)
        .NoOp(cycles: 3)
        .DeclareReference(out var returnFromDrawBoard)
        .DefineReference(m_Start, a => a
            .SetWordValue(R0, 0x8000)
            .StackFromR0()
            .CallRoutine(m_Calculate.Refs.CalculateReset)
            .SetByteValue(R1, SquareIndex.E1)
            .CopyByteTo(globals + Vars.Cursor, R1)
            .CopyByteTo(globals + Vars.Selected, R1)
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

    private sealed record Globals(int Cursor, int Selected);

    private readonly Globals Vars =
        Variables.ByteSizesToOffsets(new Globals(Cursor: 1, Selected: 1));

    private readonly Reference m_Start = new ();

    private readonly Calculate m_Calculate = new ();

    private readonly Draw.Board m_DrawBoard = new ();

    private readonly Draw.Piece m_DrawPiece = new ();
}