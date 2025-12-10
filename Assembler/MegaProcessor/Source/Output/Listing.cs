
using System.Text;

namespace Assembler.MegaProcessor.Output;

using Core.Output;

public static class Listing
{
    extension (IEnumerable<OutputLine> outputLines)
    {
        [Pure]
        public IEnumerable<string> ToListing(bool ansiColours = false)
        {
            yield return WithAnsiCmd("addr  bytes  cycles op", 36);

            var address = 0;

            foreach (var line in outputLines)
            {
                var listingLine = new StringBuilder();

                if (line.Bytes is {} bytes)
                {
                    listingLine.Append(WithAnsiCmd($"{address:X4}", 33));

                    listingLine.Append(": ");

                    bytes.ToList().ForEach(b => listingLine.Append($"{b:X2}"));
                }

                if (line.Comment is [_, ..] comment)
                {
                    var padTo = ansiColours ? 22 : 13;

                    var padding = listingLine.Length is 0 ? 0
                                : Math.Max(padTo - listingLine.Length, 0);

                    listingLine.Append(new string(' ', padding))
                               .Append(WithAnsiCmd($"// {comment}", 32));
                }

                yield return $"{listingLine}";

                address += line.Bytes?.Count ?? 0;
            }

            string WithAnsiCmd(string enclosedText, int ansiCode) =>
                ansiColours ? $"\x1B[{ansiCode}m{enclosedText}\x1B[0m"
                            : enclosedText;
        }
    }
}