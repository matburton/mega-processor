
using Assembler.MegaProcessor.Exceptions;

namespace Assembler.MegaProcessor;

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
            (out Reference reference,
             object offsets,
             byte? fillByte = 0xC | 0b1010, // nop
             [CallerArgumentExpression(nameof(reference))]
               string? referenceProse = null,
             [CallerArgumentExpression(nameof(offsets))]
               string? offsetsProse = null)
        {
            reference = new ();

            return assembly.DefineGlobals(reference,
                                          offsets,
                                          fillByte,
                                          referenceProse,
                                          offsetsProse);
        }

        [Pure]
        public Assembly DefineGlobals
            (Reference reference,
             object offsets,
             byte? fillByte = 0xC | 0b1010, // nop
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

                if (fillByte is null)
                {
                    lines.Add(new ([], $"{address:X4}: {path}"));
                }
                else
                {
                    var nextAddress = index == offsetAddresses.Length - 1
                                    ? assembly.TotalBytes + offsets.TotalBytes
                                    : offsetAddresses[index + 1].Key;
                    var bytes =
                        Enumerable.Repeat(fillByte.Value, nextAddress - address);

                    lines.Add(new ([new BytesFragment(bytes)], path));
                }
            }

            return assembly.DefineReference(reference, referenceProse)
                           .AddLines(lines);
        }

        [Pure]
        public Assembly AddWords(IEnumerable<Calculation> words)
        {
            var fragments = words.Select(c => new ReferenceFragment(2, r =>
            {
                var value = c.Calculate(r);

                if (value is < short.MinValue or > ushort.MaxValue)
                {
                    throw new InvalidInstructionException
                        ($"Data value {value} is not within 16-bit range");
                }

                var bytes = BitConverter.GetBytes(value);

                if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);

                return bytes[.. 2];
            }));

            return assembly.AddLines([new (fragments)]);
        }

        [Pure]
        public Assembly AddWords(Reference reference,
                                 IEnumerable<Calculation> words,
                                 [CallerArgumentExpression(nameof(reference))]
                                    string? referenceProse = null)
        {
            return assembly.DefineReference(reference, referenceProse)
                           .AddWords(words);
        }

        [Pure]
        public Assembly AddWords(out Reference reference,
                                 IEnumerable<Calculation> words,
                                 [CallerArgumentExpression(nameof(reference))]
                                     string? referenceProse = null)
        {
            reference = new ();

            return assembly.DefineReference(reference, referenceProse)
                           .AddWords(words);
        }

        /// <summary>Cycles:4. Bytes:3</summary>
        ///
        /// <remarks>Sets:[]</remarks>
        ///
        [Pure]
        public Assembly GoTo
            (out Reference reference,
             bool forceAbsolute = false,
             [CallerArgumentExpression(nameof(reference))]
                string referenceProse = "???")
        {
            reference = new ();

            return assembly.GoTo(reference, forceAbsolute, referenceProse);
        }

        /// <summary>Cycles:3 if <paramref name="condition"/> was met,
        ///          otherwise 2. Bytes:2</summary>
        ///
        /// <remarks>Sets:[]</remarks>
        ///
        [Pure]
        public Assembly GoToIf
            (Condition condition,
             out Reference reference,
             [CallerArgumentExpression(nameof(reference))]
                string referenceProse = "???")
        {
            reference = new ();

            return assembly.GoToIf(condition, reference, referenceProse);
        }
    }
}