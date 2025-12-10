
namespace Assembler.Core.Fragments;

using Exceptions;
using References;

public sealed class ReferenceFragment
    (int byteCount, Func<IReferences, IEnumerable<byte>> calculateBytes)
    :
    Fragment
{
    internal override int ByteCount { get; } = byteCount > 0 ? byteCount
        : throw new ArgumentException("Must be positive", nameof(byteCount));

    /// <exception cref="InvalidReferenceException" />
    ///
    [Pure]
    internal IReadOnlyList<byte> CalculateBytes(IReferences references)
    {
        var bytes = calculateBytes(references).ToArray();

        if (bytes.Length != ByteCount)
        {
            throw new InvalidReferenceException
                ($"Expected {ByteCount} bytes, calculated {bytes.Length}");
        }

        return bytes;
    }
}