
using System.Runtime.CompilerServices;
using Assembler.MegaProcessor.Exceptions;

namespace Assembler.MegaChess.Draw;

using static Register;

public static class AssemblyExtensions
{
    extension (Assembly assembly)
    {
        public Assembly BlitBitmap
            (IndexRegister to,
             DataRegister temp,
             IEnumerable<string> bitmapLines,
             [CallerArgumentExpression(nameof(bitmapLines))]
                string? bitmapLinesProse = null,
             [CallerFilePath] string? filePath = null,
             [CallerLineNumber] int lineNumber = -1)
        {
            int? lastWord = null;

            var comment = Caller.ToComment
                (null, bitmapLinesProse, filePath, lineNumber);

            if (comment is not null)
            {
                assembly = assembly.AddBlockComment([$"bitmap {comment}"]);
            }

            foreach (var word in bitmapLines.SelectMany(l => l.Chunk(16))
                                            .Select(CharsToWord))
            {
                if (word != lastWord)
                {
                    var calculationProse = $"0b{word:b16}";

                    assembly = assembly.SetWordValue
                        (temp, word, force: true, calculationProse);
                }

                assembly = assembly.CopyWordToIndex(to, temp, bumpIndex: true);

                lastWord = word;
            }

            return assembly;
        }
    }

    private static int CharsToWord(IReadOnlyCollection<char> characters)
    {
        if (characters.Count is not 16)
        {
            throw new InvalidInstructionException
                ("Bitmap string length was not a multiple of 16");
        }

        return characters.Reverse()
                         .Select(c => c is ' ' ? 0 : 1)
                         .Aggregate(0, (w, b) => w << 1 | b);
    }
}