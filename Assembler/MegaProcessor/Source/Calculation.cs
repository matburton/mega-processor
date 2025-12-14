
namespace Assembler.MegaProcessor;

public sealed class Calculation(Func<IReferences, int> calculate)
{
    public static implicit operator Calculation (int address) =>
        new (_ => address);

    public static implicit operator Calculation (Reference r) =>
        new (references => references.GetAddress(r));

    public static Calculation operator + (Calculation c, int offset) =>
            new (references => c.Calculate(references) + offset);

    public static Calculation operator - (Calculation c, int offset) =>
            new (references => c.Calculate(references) - offset);

    public Func<IReferences, int> Calculate { get; } = calculate;
}