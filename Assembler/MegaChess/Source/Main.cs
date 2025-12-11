
namespace Assembler.MegaChess;

using static Constants;
using static Register;

internal static class Main
{
    public static Assembly Build() => new Assembly()
        .DeclareReference(out var start)
        .Append(Preamble(start))
        .DeclareReference(out var calculateReset)
        .Append(Calculate.Build(calculateReset))
        .Append(Draw.Piece.Build)
        .DeclareReference(out var drawBoard)
        .Append(Draw.Board.Build(drawBoard))
        .DefineGlobals(out var globals, Vars, fillByte: 0)
        .NoOp(cycles: 3)
        .DeclareReference(out var returnFromDrawBoard)
        .DefineReference(start, a => a
            .SetWordValue(R0, 0x8000)
            .StackFromR0()
            .CallRoutine(calculateReset)
            .SetByteValue(R1, SquareIndex.E1)
            .CopyByteTo(globals + Vars.Cursor, R1)
            .CopyByteTo(globals + Vars.Selected, R1)
            .SetWordValue(R0, returnFromDrawBoard, force: true)
            .GoTo(drawBoard, forceAbsolute: true))
        .DefineReference(returnFromDrawBoard, a => a
            .NoOp()
            .Loop(InfiniteLoop))
        ;

    private static Func<Assembly, Assembly> Preamble(Reference start) => a => a
        .GoTo(start, forceAbsolute: true)
        .NoOp()
        .Repeat(3, (_, a) => a.ReturnFromInterrupt()
                              .NoOp(cycles: 3));

    private static Assembly InfiniteLoop(Reference loop, Assembly a) => a
        .NoOp()
        // TODO
        .GoTo(loop, forceAbsolute: true)
        ;

    private sealed record Globals(int Cursor, int Selected);

    private static readonly Globals Vars =
        Variables.ByteSizesToOffsets(new Globals(1, 1));
}