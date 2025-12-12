
namespace Assembler.MegaChess;

using static Register;

internal sealed class Calculate
{
    public sealed class References
    {
        public Reference CalculateReset { get; } = new ();
    };

    public References Refs { get; } = new ();

    public Assembly Build(Assembly a) => a
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
        .AddBytes(m_BoardState, Enumerable.Repeat<byte>(0, 10 * 12))
        .DefineGlobals(m_Globals, m_Vars, fillByte: 0)
        .Routine(Refs.CalculateReset, CalculateReset)
        // TODO
        ;

    private Assembly CalculateReset(Assembly a) => a
        .SetWordValue(R1, 0, force: true)
        .CopyByteTo(m_Globals + m_Vars.NewEnPassantPawnIndex, R1)
        .CopyByteTo(m_Globals + m_Vars.ClickedBoardIndex, R1)
        .CopyWordTo(m_Globals + m_Vars.ReturnValue, R1)
        .CopyWordTo(m_Globals + m_Vars.RandomValue, R1)
        .SetWordValue(R2, m_BoardState, force: true)
        .SetByteValue(R1, Piece.Enum.OffBoard)
        .Repeat(21, (_, a) => a.CopyByteToIndex(R2, R1, bumpIndex: true))
        .Repeat(9, (i, a) =>
        {
            var values = new [] { Unmoved.Black.Rook,
                                  Unmoved.Black.Knight,
                                  Unmoved.Black.Bishop,
                                  Unmoved.Black.Queen,
                                  Unmoved.Black.King,
                                  Unmoved.Black.Bishop,
                                  Unmoved.Black.Knight,
                                  Unmoved.Black.Rook,
                                  Piece.Enum.OffBoard };

            return a.SetByteValue(R1, values[i])
                    .CopyByteToIndex(R2, R1, bumpIndex: true);
        })
        .CopyByteToIndex(R2, R1, bumpIndex: true)
        .SetByteValue(R1, Unmoved.Black.Pawn)
        .Repeat(8, (_, a) => a.CopyByteToIndex(R2, R1, bumpIndex: true))
        .Repeat(4, (_, a) => a
            .SetByteValue(R1, Piece.Enum.OffBoard)
            .Repeat(2, (_, a) => a.CopyByteToIndex(R2, R1, bumpIndex: true))
            .SetByteValue(R1, Piece.Enum.Empty, force: true)
            .Repeat(8, (_, a) => a.CopyByteToIndex(R2, R1, bumpIndex: true))
        )
        .SetByteValue(R1, Piece.Enum.OffBoard)
        .Repeat(2, (_, a) => a.CopyByteToIndex(R2, R1, bumpIndex: true))
        .SetByteValue(R1, Unmoved.White.Pawn)
        .Repeat(8, (_, a) => a.CopyByteToIndex(R2, R1, bumpIndex: true))
        .SetByteValue(R1, Piece.Enum.OffBoard)
        .Repeat(2, (_, a) => a.CopyByteToIndex(R2, R1, bumpIndex: true))
        .Repeat(9, (i, a) =>
        {
            var values = new [] { Unmoved.White.Rook,
                                  Unmoved.White.Knight,
                                  Unmoved.White.Bishop,
                                  Unmoved.White.Queen,
                                  Unmoved.White.King,
                                  Unmoved.White.Bishop,
                                  Unmoved.White.Knight,
                                  Unmoved.White.Rook,
                                  Piece.Enum.OffBoard };

            return a.SetByteValue(R1, values[i])
                    .CopyByteToIndex(R2, R1, bumpIndex: true);
        })
        .Repeat(20, (_, a) => a.CopyByteToIndex(R2, R1, bumpIndex: true));

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

    private static class Unmoved
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

    private sealed record Globals(int NewEnPassantPawnIndex,
                                  int ClickedBoardIndex,
                                  int ReturnValue,
                                  int RandomValue);

    private readonly Globals m_Vars =
        Variables.ByteSizesToOffsets(new Globals(NewEnPassantPawnIndex: 1,
                                                 ClickedBoardIndex: 1,
                                                 ReturnValue: 2,
                                                 RandomValue: 2));

    private readonly Reference m_Globals = new (),
                               m_BoardState = new ();
}