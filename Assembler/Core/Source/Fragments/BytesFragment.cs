
namespace Assembler.Core.Fragments;

public sealed class BytesFragment(IEnumerable<byte> bytes) : Fragment
{
    internal override int ByteCount => Bytes.Count;

    internal IReadOnlyList<byte> Bytes { get; } = [..bytes];
}