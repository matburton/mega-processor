
namespace Assembler.Example;

using C = Condition;

using static Register;

internal sealed class Snail
{
    public Assembly Build() => new Assembly()
        .Append(Preamble)
        .Append(Init)
        .Loop((busy, a) => a
            .Append(AdvanceAndDraw(m_Offsets.Head))
            .Append(AdvanceAndDraw(m_Offsets.Tail))
            .GoToIf(C.UserFlag.Zero, busy))
        .Routine(m_Advance, Advance)
        .Routine(m_DrawPoint, DrawPoint)
        .DefineGlobals(m_Globals, m_Offsets, fillByte: null);

    private static Assembly Preamble(Assembly a) => a
        .StatusAndWith(0)
        .GoToIf(C.UserFlag.Zero, out var start)
        .Repeat(3, (_, a) => a
            .ReturnFromInterrupt()
            .NoOp(cycles: 3))
        .DefineReference(start)
        .SetWordValue(R0, RamEnd)
        .StackFromR0();

    private Assembly Init(Assembly a) => a
        .Clear(R0)
        .SetWordValue(R2, DisplayRamStart)
        .SetWordValue(R1, DisplayRamLength)
        .Loop((clearDisplay, a) => a
            .CopyWordToIndex(R2, R0, bumpIndex: true)
            .AddConst(R1, -2)
            .GoToIf(C.NotEqual, clearDisplay))
        .SetByteValue(R0, 1)
        .SetByteValue(R1, 23)
        .SetByteValue(R3, 4)
        .SetWordValue(R2, DisplayRamStart + 4)
        .Loop((initLine, a) => a
            .CopyByteToIndex(R2, R0)
            .AddTo(R2, R3)
            .AddConst(R1, -1)
            .GoToIf(C.NotEqual, initLine))
        .Clear(R0)
        .CopyByteTo(m_Globals + m_Offsets.Tail.X, R0)
        .CopyByteTo(m_Globals + m_Offsets.Tail.Y, R0)
        .CopyByteTo(m_Globals + m_Offsets.Head.X, R0)
        .SetByteValue(R0, 23)
        .CopyByteTo(m_Globals + m_Offsets.Head.Y, R0);

    private Func<Assembly, Assembly> AdvanceAndDraw(Point offsets) => a => a
        .CopyByteFrom(m_Globals + offsets.X, R0)
        .CopyByteFrom(m_Globals + offsets.Y, R1)
        .CallRoutine(m_Advance)
        .CopyByteTo(m_Globals + offsets.X, R0)
        .CopyByteTo(m_Globals + offsets.Y, R1)
        .CallRoutine(m_DrawPoint);

    private static Assembly Advance(Assembly a) => a
        .SetByteValue(R2, 1)
        .AndTo(R2, R0)
        .GoToIf(C.NotEqual, out var ap1)
        .SetByteValue(R2, 63)
        .Compare(R2, R1)
        .GoToIf(C.Equal, out var ap2)
        .AddConst(R1, 1)
        .ReturnFromRoutine()
        .DefineReference(ap1, a => a
            .Test(R1)
            .GoToIf(C.Equal, ap2)
            .AddConst(R1, -1)
            .ReturnFromRoutine())
        .DefineReference(ap2, a => a
            .AddConst(R0, 1)
            .SetByteValue(R2, 0x1F)
            .AndTo(R0, R2));

    private static Assembly DrawPoint(Assembly a) => a
        .CopyTo(R2, R0)
        .SetWordValue(R3, DisplayRamStart)
        .AddBytes([0xDA, 0x1D])
        .AddTo(R3, R2)
        .AddBytes([0xD9, 0x02])
        .AddTo(R3, R1)
        .SetByteValue(R1, 1)
        .SetByteValue(R2, 7)
        .AndTo(R0, R2)
        .AddBytes([0xD9, 0x20])
        .CopyByteFromIndex(R3, R0)
        .ExclusiveOrTo(R0, R1)
        .CopyByteToIndex(R3, R0);

    private sealed record Point(int X = 1, int Y = 1);

    private sealed class Globals
    {
        public Point Head { get; } = new ();
        public Point Tail { get; } = new ();
    }

    private const int DisplayRamStart  = 0xA000,
                      DisplayRamLength = 0x0100,
                      RamEnd           = 0x8000;

    private readonly Reference m_Globals   = new (),
                               m_Advance   = new (),
                               m_DrawPoint = new ();

    private readonly Globals m_Offsets =
        Variables.ByteSizesToOffsets(new Globals());
}