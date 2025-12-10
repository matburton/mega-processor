
namespace Assembler.MegaProcessor;

public sealed class BitChange
{
    public static readonly BitChange None   = new ("btst", 0);
    public static readonly BitChange Invert = new ("bchg", 0b01 << 6);
    public static readonly BitChange Zero   = new ("bclr", 0b10 << 6);
    public static readonly BitChange One    = new ("bset", 0b11 << 6);

    private BitChange(string label, byte bits) =>
        (Label, Bits) = (label, bits);

    internal string Label { get; }

    internal byte Bits { get; }
}