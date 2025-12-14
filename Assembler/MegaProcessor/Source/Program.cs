
namespace Assembler.MegaProcessor;

using Output;

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

            _  => ["Builds a chess binary for the MegaProcessor",
                   "  -l, --listing   Write a debugging aid to stdout",
                   "  -h, --hex       Write Intel hex to stdout"]
        };

        foreach (var line in lines) Console.WriteLine(line);

        return 0;
    }
}