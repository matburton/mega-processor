
namespace Assembler.MegaChess;

using static Constants;
using static Register;
using static Structures;

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
        .DefineGlobals(out var globals, Vars, VarsTotalBytes, fillByte: 0)
        .NoOp(cycles: 3)
        .DefineReference(start)
        .SetWordValue(R0, 0x8000)
        .StackFromR0()
        .CallRoutine(calculateReset)
        .SetByteValue(R1, SquareIndex.E1)
        .CopyByteTo(globals + Vars.Cursor, R1)
        .CopyByteTo(globals + Vars.Selected, R1)
        .DeclareReference(out var returnFromDrawBoard)
        .SetWordValue(R0, returnFromDrawBoard, force: true)
        .GoTo(drawBoard, forceAbsolute: true)
        .DefineReference(returnFromDrawBoard)
        .NoOp()
        .Loop(InfiniteLoop)
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
}