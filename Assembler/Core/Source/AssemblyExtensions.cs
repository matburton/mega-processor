
namespace Assembler.Core;

using Fragments;
using References;

public static class AssemblyExtensions
{
    extension(Assembly assembly)
    {
        [Pure]
        public Assembly DeclareReference(out Reference reference)
        {
            reference = new ();

            return assembly;
        }

        [Pure]
        public Assembly DefineReference
            (Reference reference,
             Func<Assembly, Assembly> append,
             [CallerArgumentExpression(nameof(reference))]
                string? referenceProse = null,
             [CallerArgumentExpression(nameof(append))]
                string? appendProse = null,
             [CallerFilePath] string? filePath = null,
             [CallerLineNumber] int lineNumber = -1)
        {
            var comment = Caller.ToComment
                (referenceProse, appendProse, filePath, lineNumber);

            return assembly.AddBlockComment([comment])
                           .DefineReference(reference, null)
                           .Append(append, null, null, -1);
        }

        [Pure]
        public Assembly DefineReference
            (out Reference reference,
             Func<Assembly, Assembly>? append = null,
             [CallerArgumentExpression(nameof(reference))]
                string? referenceProse = null,
             [CallerArgumentExpression(nameof(append))]
                string? appendProse = null,
             [CallerFilePath] string? filePath = null,
             [CallerLineNumber] int lineNumber = -1)
        {
            reference = new ();

            return assembly.DefineReference(reference,
                                            append ?? (a => a),
                                            referenceProse,
                                            appendProse,
                                            filePath,
                                            lineNumber);
        }

        [Pure]
        public Assembly AddBlockComment(IEnumerable<string?> lines) =>
            assembly.AddLines
                (lines.OfType<string>().Select(s => new Line([], s)));

        [Pure]
        public Assembly AddBytes(IEnumerable<byte> bytes) =>
            assembly.AddLines([new ([new BytesFragment(bytes)])]);

        [Pure]
        public Assembly AddBytes(Reference reference,
                                 IEnumerable<byte> bytes,
                                 [CallerArgumentExpression(nameof(reference))]
                                    string? referenceProse = null)
        {
            return assembly.DefineReference(reference, referenceProse)
                           .AddBytes(bytes);
        }

        [Pure]
        public Assembly AddBytes(out Reference reference,
                                 IEnumerable<byte> bytes,
                                 [CallerArgumentExpression(nameof(reference))]
                                    string? referenceProse = null)
        {
            reference = new ();

            return assembly.AddBytes(reference, bytes, referenceProse);
        }

        [Pure]
        public Assembly Append
            (Func<Assembly, Assembly> append,
             [CallerArgumentExpression(nameof(append))]
                string? appendProse = null,
             [CallerFilePath] string? filePath = null,
             [CallerLineNumber] int lineNumber = -1)
        {
            var comment = Caller.ToComment
                (null, appendProse, filePath, lineNumber);

            return append(assembly.AddBlockComment([comment]));
        }

        [Pure]
        public Assembly Repeat
            (int times, Func<int, Assembly, Assembly> append)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(times, 1);

            return Enumerable.Range(0, times)
                             .Aggregate(assembly, (a, i) => append(i, a));
        }
    }
}