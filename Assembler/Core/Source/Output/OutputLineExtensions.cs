
using System.Diagnostics;

namespace Assembler.Core.Output;

public static class OutputLineExtensions
{
    extension (IEnumerable<OutputLine> outputLines)
    {
        public IEnumerable<OutputLine> CollapseRepeats()
        {
            var lineBuffer = new List<OutputLine>();

            PatternBuffer? patternBuffer = null;

            foreach (var outputLine in outputLines)
            {
                lineBuffer.Add(outputLine);

                if (patternBuffer is not null)
                {
                    var matchesPattern = MatchesPattern(lineBuffer,
                                                        patternBuffer.Pattern);
                    if (matchesPattern is true)
                    {
                        lineBuffer.Clear();

                        ++patternBuffer.RepeatCount;
                    }

                    if (matchesPattern is not false) continue;

                    var collapsedOutputLines =
                        ToCollapsedOutputLines(patternBuffer);

                    foreach (var collapsedOutputLine in collapsedOutputLines)
                    {
                        yield return collapsedOutputLine;
                    }
                }

                patternBuffer = DetectPattern([..lineBuffer]);

                if (patternBuffer is not null)
                {
                    var patternOutputLineCount =
                        patternBuffer.Pattern.Count * patternBuffer.RepeatCount;

                    var nonPatternOutputLines =
                        lineBuffer[.. ^patternOutputLineCount];

                    foreach (var nonPatternOutputLine in nonPatternOutputLines)
                    {
                        yield return nonPatternOutputLine;
                    }

                    lineBuffer.Clear();
                }
                else if (lineBuffer.Count >= 10)
                {
                    yield return lineBuffer[0];

                    lineBuffer.RemoveAt(0);
                }
            }

            foreach (var outputLine in ToCollapsedOutputLines(patternBuffer)
                                      .Concat(lineBuffer))
            {
                yield return outputLine;
            }
        }
    }

    private static bool? MatchesPattern(IReadOnlyList<OutputLine> buffer,
                                        IReadOnlyList<OutputLine> pattern)
    {
        return buffer.SequenceEqual(pattern.Take(buffer.Count),
                                    OutputLineEqualityComparer.Instance)
             ? buffer.Count == pattern.Count ? true : null
             : false;
    }

    private static PatternBuffer? DetectPattern
        (ReadOnlySpan<OutputLine> outputLines)
    {
        if (outputLines.Length < 3) return null;

        var comparer = OutputLineEqualityComparer.Instance;

        for (var patternLength = 5; patternLength > 1; --patternLength)
        {
            if (outputLines.Length < patternLength * 2) continue;

            var pattern = outputLines[^patternLength ..];

            var patternDetected = pattern.SequenceEqual
                (outputLines[^(patternLength * 2) .. ^patternLength], comparer);

            if (patternDetected && pattern.ToArray().All(l => l.Bytes != null))
            {
                return new ([..pattern]) { RepeatCount = 2 };
            }
        }

        if (   outputLines[^1].Bytes is not null
            && comparer.Equals(outputLines[^1], outputLines[^2])
            && comparer.Equals(outputLines[^1], outputLines[^3]))
        {
            return new ([outputLines[^1]]) { RepeatCount = 3 };
        }

        return null;
    }

    private static IEnumerable<OutputLine> ToCollapsedOutputLines
        (PatternBuffer? patternBuffer)
    {
        if (patternBuffer is null) yield break;

        var prefix = $"[{patternBuffer.RepeatCount}x]";

        var padding = new string(' ', prefix.Length);

        yield return new ([])
            { Comment = $"{prefix} {patternBuffer.Pattern[0].Comment}" };

        foreach (var outputLine in patternBuffer.Pattern.Skip(1))
        {
            yield return new ([])
                { Comment = $"{padding} {outputLine.Comment}" };
        }

        var bytes = patternBuffer.Pattern.SelectMany(l => l.Bytes ?? []);

        yield return new (RepeatEnumerable(bytes, patternBuffer.RepeatCount));
    }

    private static IEnumerable<T> RepeatEnumerable<T>(IEnumerable<T> items,
                                                      int repeatCount)
    {
        items = items.ToArray();

        for (var index = 0; index < repeatCount; ++index)
        {
            foreach (var item in items) yield return item;
        }
    }

    private sealed record PatternBuffer(IReadOnlyList<OutputLine> Pattern)
    {
        public int RepeatCount { get; set; } = 2;
    }

    private sealed class OutputLineEqualityComparer
        :
        IEqualityComparer<OutputLine>
    {
        public static readonly OutputLineEqualityComparer Instance = new ();

        public bool Equals(OutputLine? a, OutputLine? b)
        {
            if (ReferenceEquals(a, b)) return true;

            if (a is null || b is null) return false;

            return BytesEqual(a.Bytes, b.Bytes) && a.Comment == b.Comment;
        }

        public int GetHashCode(OutputLine outputLine) =>
            throw new UnreachableException();

        private static bool BytesEqual(IReadOnlyList<byte>? a,
                                       IReadOnlyList<byte>? b)
        {
            if (ReferenceEquals(a, b)) return true;

            return a is not null && b is not null && a.SequenceEqual(b);
        }
    }
}