
namespace Assembler.Core.Output;

public sealed class OutputLine
{
    internal OutputLine(IEnumerable<byte> bytes) =>
        Bytes = bytes.ToArray() is [_, ..] b ? b : null;

    public string? Comment { get; internal init; }

    /// <remarks>Never empty</remarks>
    ///
    public IReadOnlyList<byte>? Bytes { get; }
}