
namespace Assembler.MegaProcessor;

public sealed class Condition // TODO: Add more conditions
{
    public static class UserFlag
    {
        /// <summary>Met if: [U=0]</summary>
        ///
        public static readonly Condition Zero = new ("uc", 0x0);

        /// <summary>Met if: [U=1]</summary>
        ///
        public static readonly Condition One = new ("us", 0x1);
    }

    /// <summary>Met if: [Z=0]</summary>
    ///
    public static readonly Condition NotEqual = new ("ne", 0x6);

    /// <summary>Met if: [Z=1]</summary>
    ///
    public static readonly Condition Equal = new ("eq", 0x7);

    /// <summary>Met if: [Z=1 & V=1, Z=0 & V=0]</summary>
    ///
    public static readonly Condition GreaterOrEqual = new ("ge", 0xC);

    /// <summary>Met if: [N=1 & V=0, N=0 & V=1]</summary>
    ///
    public static readonly Condition Less = new ("lt", 0xD);

    /// <summary>Met if: [N=1 & V=1 & Z=0, N=0 & V=0 & Z=0]</summary>
    ///
    public static readonly Condition Greater = new ("gt", 0xE);

    /// <summary>Met if: [Z=1, N=1 & V=0, N=0 & V=1]</summary>
    ///
    public static readonly Condition LessOrEqual = new ("le", 0xF);

    private Condition(string label, byte bits) =>
        (Label, Bits) = (label, bits);

    internal string Label { get; }

    internal byte Bits { get; }
}