
namespace Assembler.MegaChess.Draw;

internal static class Board
{
    public static Func<Assembly, Assembly> Build(Reference draw) => a => a
        .DefineReference(draw)
        // TODO
        ;
}