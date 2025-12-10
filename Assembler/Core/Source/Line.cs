
namespace Assembler.Core;

using Fragments;

public sealed class Line(IEnumerable<Fragment> fragments)
{
    public Line(IEnumerable<Fragment> fragments, string comment)
        :
        this(fragments) => Comment = comment;

    internal IReadOnlyList<Fragment> Fragments { get; } = [..fragments];

    public string? Comment { internal get; init; }
}