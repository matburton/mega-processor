
namespace Assembler.MegaChess;

internal static class Structures
{
    public sealed record Globals(int Cursor, int Selected);

    static Structures() => (Vars, VarsTotalBytes) =
        Variables.ByteSizesToOffsets(new Globals(Cursor: 1, Selected: 1));

    public static readonly Globals Vars;

    public static readonly int VarsTotalBytes;
}