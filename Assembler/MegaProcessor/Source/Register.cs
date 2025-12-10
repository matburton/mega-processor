
namespace Assembler.MegaProcessor;

public class Register
{
    public static readonly DataRegister  R0 = new ("r0", 0b00);
    public static readonly DataRegister  R1 = new ("r1", 0b01);
    public static readonly IndexRegister R2 = new ("r2", 0b10, 0b0);
    public static readonly IndexRegister R3 = new ("r3", 0b11, 0b01);

    private Register(string label, byte bits) =>
        (Label, Bits) = (label, bits);

    internal string Label { get; }

    internal byte Bits { get; }

    public sealed class IndexRegister : Register
    {
        internal IndexRegister(string label, byte bits, byte indexBits)
            :
            base(label, bits)
        {
            IndexBits = indexBits;
        }

        public byte IndexBits { get; }
    }

    public sealed class DataRegister : Register
    {
        internal DataRegister(string label, byte bits) : base(label, bits) {}

        public byte DataBits  => Bits;
    }
}