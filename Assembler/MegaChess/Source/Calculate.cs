namespace Assembler.MegaChess;

internal static class Calculate
{
    public static Func<Assembly, Assembly> Build(Reference calculateReset) => a => a
        .DefineReference(calculateReset); // TODO

    private static class Bools
    {
        public const int False = 0, True = 0xFF;
    }

    private static class Piece
    {
        public static class Colour
        {
            public const int Mask  = 0b1000,
                             White = 0b1000,
                             Black = 0b0000;
        }

        public const int Unmoved   = 0b110000,
                         ValueMask = 0b001111;

        public static class Enum
        {
            public const int Mask      = 0b111,
                             Empty     = 0b000,
                             Pawn      = 0b001,
                             King      = 0b010,
                             Knight    = 0b011,
                             Bishop    = 0b100,
                             Rook      = 0b101,
                             Queen     = 0b110,
                             OffBoard  = 0b111,
                             WhitePawn = Colour.White + Pawn;
        }

        public static class GameValue
        {
            public const byte Empty  = 0,
                              Pawn   = 14,
                              King   = 0,
                              Knight = 40,
                              Bishop = 38,
                              Rook   = 68,
                              Queen  = 124,
                              QueenPawnDiff = Queen - Pawn;
        }
    }
}