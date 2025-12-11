namespace Assembler.MegaChess;

internal static class Calculate
{
    public static Func<Assembly, Assembly> Build(Reference calculateReset) => a => a
        .AddBytes(out var pieceGameValues,
                 [Piece.GameValue.Empty,
                  Piece.GameValue.Pawn,
                  Piece.GameValue.King,
                  Piece.GameValue.Knight,
                  Piece.GameValue.Bishop,
                  Piece.GameValue.Rook,
                  Piece.GameValue.Queen])
        .AddWords(out var rookMoveDirections, [-1, 1, -10, 10])
        .AddWords(out var bishopMoveDirections, [-11, -9, 9, 11])
        .AddWords(out var blackPawnMoveDirections, [9, 11, 10, 20])
        .AddWords(out var whitePawnMoveDirections, [-11, -9, -10, -20])
        .AddWords(out var knightMoveDirections,
                      [-21, -19, -12, -8, 8, 12, 19, 21])
        .AddWords(out var initialMoveDirections,
                      [0,
                       blackPawnMoveDirections,
                       rookMoveDirections,
                       knightMoveDirections,
                       bishopMoveDirections,
                       rookMoveDirections,
                       rookMoveDirections,
                       0,
                       0,
                       whitePawnMoveDirections,
                       rookMoveDirections,
                       knightMoveDirections,
                       bishopMoveDirections,
                       rookMoveDirections,
                       rookMoveDirections])
        .AddBytes(out var boardState, Enumerable.Repeat<byte>(0, 10 * 12))
        // TODO
        .DefineReference(calculateReset)
        ;

    private sealed record Globals(int NewEnPassantPawnIndex,
                                  int ClickedBoardIndex,
                                  int ReturnValue,
                                  int RandomValue);

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

    public static class Unmoved
    {
        public static class Black
        {
            public const int
                Pawn   = Piece.Unmoved + Piece.Colour.Black + Piece.Enum.Pawn,
                King   = Piece.Unmoved + Piece.Colour.Black + Piece.Enum.King,
                Knight = Piece.Unmoved + Piece.Colour.Black + Piece.Enum.Knight,
                Bishop = Piece.Unmoved + Piece.Colour.Black + Piece.Enum.Bishop,
                Rook   = Piece.Unmoved + Piece.Colour.Black + Piece.Enum.Rook,
                Queen  = Piece.Unmoved + Piece.Colour.Black + Piece.Enum.Queen;
        }

        public static class White
        {
            public const int
                Pawn   = Piece.Unmoved + Piece.Colour.White + Piece.Enum.Pawn,
                King   = Piece.Unmoved + Piece.Colour.White + Piece.Enum.King,
                Knight = Piece.Unmoved + Piece.Colour.White + Piece.Enum.Knight,
                Bishop = Piece.Unmoved + Piece.Colour.White + Piece.Enum.Bishop,
                Rook   = Piece.Unmoved + Piece.Colour.White + Piece.Enum.Rook,
                Queen  = Piece.Unmoved + Piece.Colour.White + Piece.Enum.Queen;
        }
    }
}