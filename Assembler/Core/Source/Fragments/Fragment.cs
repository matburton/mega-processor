
namespace Assembler.Core.Fragments;

/// <summary>A part of a <see cref="Line"/></summary>
///
/// <seealso cref="BytesFragment" />
/// <seealso cref="ReferenceFragment" />
///
public abstract class Fragment
{
    internal Fragment() {}

    internal abstract int ByteCount { get; }
}