
using System.Collections.Immutable;
using System.Diagnostics;

namespace Assembler.Core;

using Exceptions;
using Fragments;
using Output;
using References;

public sealed class Assembly
{
    public Assembly() =>
        m_State = new ([], 0, new Dictionary<Reference, int>());

    public int TotalBytes => m_State.TotalBytes;

    [Pure]
    public Assembly AddLines(IEnumerable<Line> lines)
    {
        lines = lines.ToArray();

        return new (m_State with
        {
            Lines = m_State.Lines.AddRange(lines),
            TotalBytes = m_State.TotalBytes
                       + lines.Sum(l => l.Fragments.Sum(f => f.ByteCount)),
        });
    }

    /// <exception cref="InvalidReferenceException">
    /// Reference already has a defined address</exception>
    ///
    [Pure]
    public Assembly DefineReference
        (Reference reference,
         [CallerArgumentExpression(nameof(reference))]
                string? referenceProse = null)
    {
        var referenceAddresses = m_State.ReferenceAddresses.ToDictionary();

        if (!referenceAddresses.TryAdd(reference, m_State.TotalBytes))
        {
            throw new InvalidReferenceException
                ("Reference address has already been defined");
        }

        return new (m_State with
        {
            ReferenceAddresses = referenceAddresses,
            Lines = Caller.ToComment(referenceProse) is {} comment
                  ? m_State.Lines.Add(new ([], comment))
                  : m_State.Lines
        });
    }

    /// <exception cref="Exceptions.InvalidReferenceException" />
    ///
    [Pure]
    public IEnumerable<OutputLine> Assemble()
    {
        // TODO: When CalculateBytes fails give the current address and line comment?

        var usedReferences = new HashSet<Reference>();

        var currentAddress = 0;

        OutputLine? lastOutputLine = null;

        foreach (var line in m_State.Lines)
        {
            var bytes = line.Fragments.SelectMany(f => f switch
            {
                BytesFragment b => b.Bytes,

                // ReSharper disable once AccessToModifiedClosure
                ReferenceFragment r =>
                    r.CalculateBytes(new References(m_State,
                                                    currentAddress,
                                                    r => usedReferences.Add(r))),

                _ => throw new UnreachableException
                        ($"Unknown fragment type {f.GetType()}")
            });

            var outputLine = new OutputLine(bytes)
                { Comment = line.Comment };

            currentAddress += outputLine.Bytes?.Count ?? 0;

            if (!IsEmpty(outputLine) || !IsEmpty(lastOutputLine))
            {
                yield return outputLine;
            }

            lastOutputLine = outputLine;
        }

        var unusedReferences =
            m_State.ReferenceAddresses.Keys.Except(usedReferences).ToArray();

        if (unusedReferences is not [])
        {
            throw new InvalidReferenceException
                ($"{unusedReferences.Length} unused defined references: "
                 + string.Join(", ", unusedReferences.Select(r => r.Label)));
        }
    }

    /// <exception cref="Exceptions.InvalidReferenceException" />
    ///
    [Pure]
    public IEnumerable<OutputLine> Assemble(out IReferences references)
    {
        references = new References(m_State, m_State.TotalBytes, _ => {});

        return Assemble();
    }

    private static bool IsEmpty(OutputLine? outputLine) =>
        outputLine is null or { Bytes: null, Comment: null or [] };

    private Assembly(State state) => m_State = state;

    private sealed record State
        (IImmutableList<Line> Lines,
         int TotalBytes,
         IReadOnlyDictionary<Reference, int> ReferenceAddresses);

    private sealed class References
        (State state, int currentLineAddress, Action<Reference> usedReference)
        :
        IReferences
    {
        public int GetAddress(Reference reference)
        {
            if (state.ReferenceAddresses.TryGetValue(reference, out var address))
            {
                usedReference(reference);

                return address;
            }

            var message = reference.Label is not null
                        ? $"Reference {reference.Label} has no defined address"
                        : "Reference has no defined address";

            throw new InvalidReferenceException(message);
        }

        public int CurrentLineAddress { get; } = currentLineAddress;
    }

    private readonly State m_State;
}