
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace Assembler.Core.Output;

public static class OutputLineExtensions
{
    extension (IEnumerable<OutputLine> outputLines)
    {
        public IEnumerable<OutputLine> CollapseRepeats()
        {
            var state = new State();

            ExceptionDispatchInfo? exceptionDispatchInfo = null;

            using var enumerator =
                CollapseRepeats(outputLines, state).GetEnumerator();

            while (true)
            {
                try
                {
                    if (!enumerator.MoveNext()) break;
                }
                catch (Exception exception)
                {
                    exceptionDispatchInfo =
                        ExceptionDispatchInfo.Capture(exception);
                }

                yield return enumerator.Current;
            }

            foreach (var outputLine in ToCollapsedOutputLines(state.PatternBuffer)
                                      .Concat(state.LineBuffer))
            {
                yield return outputLine;
            }

            exceptionDispatchInfo?.Throw();
        }
    }

    private static IEnumerable<OutputLine> CollapseRepeats
        (IEnumerable<OutputLine> outputLines, State state)
    {
        foreach (var outputLine in outputLines)
        {
            state.LineBuffer.Add(outputLine);

            if (state.PatternBuffer is not null)
            {
                var matchesPattern = MatchesPattern
                    (state.LineBuffer, state.PatternBuffer.Pattern);

                if (matchesPattern is true)
                {
                    state.LineBuffer.Clear();

                    ++state.PatternBuffer.RepeatCount;
                }

                if (matchesPattern is not false) continue;

                var collapsedOutputLines =
                    ToCollapsedOutputLines(state.PatternBuffer);

                foreach (var collapsedOutputLine in collapsedOutputLines)
                {
                    yield return collapsedOutputLine;
                }
            }

            state.PatternBuffer = DetectPattern([..state.LineBuffer]);

            if (state.PatternBuffer is not null)
            {
                var patternOutputLineCount =
                      state.PatternBuffer.Pattern.Count
                    * state.PatternBuffer.RepeatCount;

                var nonPatternOutputLines =
                    state.LineBuffer[.. ^patternOutputLineCount];

                foreach (var nonPatternOutputLine in nonPatternOutputLines)
                {
                    yield return nonPatternOutputLine;
                }

                state.LineBuffer.Clear();
            }
            else if (state.LineBuffer.Count >= 10)
            {
                yield return state.LineBuffer[0];

                state.LineBuffer.RemoveAt(0);
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

            if (patternDetected && pattern.ToArray().All(AllowedInPattern))
            {
                return new ([..pattern]) { RepeatCount = 2 };
            }
        }

        if (   AllowedInPattern(outputLines[^1])
            && comparer.Equals(outputLines[^1], outputLines[^2])
            && comparer.Equals(outputLines[^1], outputLines[^3]))
        {
            return new ([outputLines[^1]]) { RepeatCount = 3 };
        }

        return null;
    }

    private static bool AllowedInPattern(OutputLine outputLine) =>
        outputLine is { Bytes: not null, Comment: not null };

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

    private sealed class State
    {
        public List<OutputLine> LineBuffer { get; } = new ();

        public PatternBuffer? PatternBuffer { get; set; }
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