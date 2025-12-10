
namespace Assembler.MegaProcessor;

internal sealed class Size
{
    public static readonly Size Byte = new ("b", 1, 0b1);
    public static readonly Size Word = new ("w", 2, 0b0);

    private Size(string label, int byteCount, byte bits) =>
        (Label, ByteCount, Bits) = (label, byteCount, bits);

    public string Label { get; }

    public int ByteCount { get; }

    public byte Bits { get; }
}