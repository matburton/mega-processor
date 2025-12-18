
using System.Globalization;

namespace Assembler.MegaProcessor;

using Output;

// TODO: Catch known exceptions and display them in a more terse way?

public static class Program
{
    public static int Main(IReadOnlyCollection<string> args,
                           Func<Assembly> createAssembly)
    {
        var assemble = () => createAssembly().Assemble();

        var enableAnsiColours =
            !Console.IsOutputRedirected || args.Contains("--ansi");

        var lines = args.Except(["--ansi"]).ToArray() switch
        {
            ["--listing" or "-l"] =>
                assemble().CollapseRepeats().ToListing(enableAnsiColours),

            ["--hex" or "-h"] => assemble().ToIntelHex(),

            ["--debug" or "-d", var address] =>
                assemble().CollapseRepeats()
                          .Debug(ParseHex(address),
                                 Console.WindowHeight - 1,
                                 enableAnsiColours),

            _ => ["Builds a chess binary for the MegaProcessor",
                  "  -l, --listing      Write a debugging aid to stdout",
                  "  -d, --debug <addr> Write listing around the"
                  +                   " given address in hex to stdout",
                  "  -h, --hex          Write Intel hex to stdout"]
        };

        foreach (var line in lines) Console.WriteLine(line);

        return 0;
    }

    private static int ParseHex(string text) =>
        int.Parse(text.StartsWith("0x") ? text[2 ..] : text,
                  NumberStyles.HexNumber);
}