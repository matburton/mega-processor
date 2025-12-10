
namespace Assembler.MegaChess;

internal static class Constants
{
    public static class SquareIndex
    {
        public const int A8 = 21, E1 = 95;
    }

    public const int PeripheralsBase = 0x8000;

    public static class GenIo
    {
        public const int Base = PeripheralsBase + 0x30,
                         Input = Base + 2;
    }
}