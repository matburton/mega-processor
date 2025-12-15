
namespace Assembler.MegaProcessor.Output;

public static class Listing
{
    extension (IEnumerable<OutputLine> outputLines)
    {
        [Pure]
        public IEnumerable<string> ToListing(bool ansiColours = false) =>
            outputLines.ToListingWithAddresses(ansiColours)
                       .Select(t => t.Text);

        [Pure]
        public IEnumerable<string> Debug
            (int address, int maxLines, bool ansiColours = false)
        {
            maxLines -= 1;

            var listingWithAddresses =
                outputLines.ToListingWithAddresses(ansiColours).Skip(1);

            var lineBuffer = new List<string>();

            int? linesAfter = null;

            foreach (var (currentAddress, text) in listingWithAddresses)
            {
                if (linesAfter is null && currentAddress > address)
                {
                    lineBuffer[^1] = WithAnsiCmd(lineBuffer[^1], 41);

                    linesAfter = Math.Max(maxLines / 2,
                                          maxLines - lineBuffer.Count - 1);
                }

                if (--linesAfter < 0) break;

                lineBuffer.Add(text);

                if (lineBuffer.Count > maxLines) lineBuffer.RemoveAt(0);
            }

            return [WithAnsiCmd("addr  bytes  cycles op", 36), ..lineBuffer];

            string WithAnsiCmd(string enclosedText, int ansiCode) =>
                ansiColours ? $"\x1B[{ansiCode}m{enclosedText}\x1B[0m"
                            : enclosedText;
        }

        private IEnumerable<(int Address, string Text)> ToListingWithAddresses
            (bool ansiColours)
        {
            yield return (0, WithAnsiCmd("addr  bytes  cycles op", 36));

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

                yield return (address, $"{listingLine}");

                address += line.Bytes?.Count ?? 0;
            }

            string WithAnsiCmd(string enclosedText, int ansiCode) =>
                ansiColours ? $"\x1B[{ansiCode}m{enclosedText}\x1B[0m"
                            : enclosedText;
        }
    }
}