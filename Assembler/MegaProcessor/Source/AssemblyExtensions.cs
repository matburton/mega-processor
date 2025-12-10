
using System.Runtime.CompilerServices;

using Assembler.Core.Fragments;

namespace Assembler.MegaProcessor;

using Core;
using Core.References;

public static class AssemblyExtensions
{
    extension (Assembly assembly)
    {
        [Pure]
        public Assembly Routine(out Reference reference,
                                Func<Assembly, Assembly> append,
                                [CallerArgumentExpression(nameof(reference))]
                                    string? referenceProse = null,
                                [CallerFilePath] string? filePath = null,
                                [CallerLineNumber] int lineNumber = -1)
        {
            reference = new ();

            return assembly.Routine
                (reference, append, referenceProse, filePath, lineNumber);
        }

        [Pure]
        public Assembly Routine(Reference reference,
                                Func<Assembly, Assembly> append,
                                [CallerArgumentExpression(nameof(reference))]
                                    string? referenceProse = null,
                                [CallerFilePath] string? filePath = null,
                                [CallerLineNumber] int lineNumber = -1)
        {
            var comment = Caller.ToComment
                (referenceProse, null, filePath, lineNumber, "routine");

            return assembly.AddBlockComment([string.Empty, comment])
                           .DefineReference(reference, null)
                           .Append(append, null, null, -1)
                           .ReturnFromRoutine()
                           .AddBlockComment([string.Empty]);
        }

        [Pure]
        public Assembly Loop
            (Func<Reference, Assembly, Assembly> append,
             [CallerFilePath] string? filePath = null,
             [CallerLineNumber] int lineNumber = -1)
        {
            var loop = new Reference();

            var comment = Caller.ToComment
                (null, null, filePath, lineNumber, "loop");

            return assembly.AddBlockComment([comment])
                           .DefineReference(loop, null)
                           .Append(a => append(loop, a), null, null, -1)
                           .AddBlockComment(["end loop"]);
        }

        [Pure]
        public Assembly DefineGlobals
            (Reference reference,
             object offsets,
             int? totalBytes,
             [CallerArgumentExpression(nameof(reference))]
               string? referenceProse = null,
             [CallerArgumentExpression(nameof(offsets))]
               string? offsetsProse = null)
        {
            var offsetAddresses = Variables
                .OffsetAddresses(offsets, assembly.TotalBytes, offsetsProse)
                .ToArray();

            var lines = new List<Line>();

            for (var index = 0; index < offsetAddresses.Length; ++index)
            {
                var (address, path) = offsetAddresses[index];

                if (totalBytes is null)
                {
                    lines.Add(new ([], $"{address:X4}: {path}"));
                }
                else
                {
                    var nextAddress = index == offsetAddresses.Length - 1
                                    ? assembly.TotalBytes + totalBytes.Value
                                    : offsetAddresses[index + 1].Key;

                    var bytes = Enumerable.Repeat<byte>
                        (0xC | 0b101, nextAddress - address);

                    lines.Add(new ([new BytesFragment(bytes)], path));
                }
            }

            return assembly.DefineReference(reference, referenceProse)
                           .AddLines(lines);
        }
    }
}