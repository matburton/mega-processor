
namespace Assembler.MegaChess;

using C = Condition;

using static Register;

internal sealed class Calculate(Reference m_MainGlobals)
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
        .Routine(m_Calculate, CalculateRoutine)
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

    private Assembly CalculateRoutine(Assembly a) => a
        .SetByteValue(R1, m_Stuff.Locals.TotalBytes)
        .StackToR0()
        .SubFrom(R0, R1)
        .StackFromR0()
        .CopyByteFromStack(m_Stuff.Args.OpponentPieceColor, R0)
        .SetByteValue(R1, Piece.Colour.Mask)
        .ExclusiveOrTo(R1, R0)
        .CopyByteToStack(m_Stuff.Locals.OriginPieceColor, R1)
        .SetWordValue(R0, -32768)
        .CopyWordToStack(m_Stuff.Locals.BestGameValue, R0)
        .SetByteValue(R0, Bools.False, force: true)
        .CopyByteToStack(m_Stuff.Locals.OriginPlayerIsInCheck, R0)
        .CopyByteFromStack(m_Stuff.Args.ModeMaxDepth, R0)
        .GoToIf(C.Equal, m_OriginPlayerIsInCheckNotModeIsZero)
        .StackToR0()
        .CopyTo(R3, R0)
        .SetWordValue(R2, -m_NextArgs.OpponentPieceColor)
        .AddTo(R2, R3)
        .CopyWordToIndex(R2, R1)
        .SetByteValue(R0, 0, force: true)
        .SetWordValue(R2, -m_NextArgs.Depth)
        .AddTo(R2, R3)
        .CopyWordToIndex(R2, R0)
        .SetWordValue(R2, -m_NextArgs.EnPassantPawnIndex)
        .AddTo(R2, R3)
        .CopyWordToIndex(R2, R0)
        .SetWordValue(R2, -m_NextArgs.ModeMaxDepth)
        .AddTo(R2, R3)
        .CopyByteToIndex(R2, R0)
        .SetWordValue(R2, -m_NextArgs.MaxGameValueThatAvoidsPruning)
        .AddTo(R2, R3)
        .CopyWordToIndex(R2, R0)
        .Append(Call)
        .SetByteValue(R2, Bools.False, force: true)
        .SetWordValue(R1, 10000)
        .Compare(R0, R1)
        .DeclareReference(out var wasNotInCheck)
        .GoToIf(C.LessOrEqual, wasNotInCheck)
        .SetByteValue(R2, Bools.True)
        .DefineReference(wasNotInCheck)
        .CopyByteToStack(m_Stuff.Locals.OriginPlayerIsInCheck, R2)
        .DefineReference(m_OriginPlayerIsInCheckNotModeIsZero)
        .SetWordValue(R1, 32767)
        .CopyByteFromStack(m_Stuff.Args.Depth, R0)
        .DeclareReference(out var winGameValueDepthZero)
        .GoToIf(C.Equal, winGameValueDepthZero)
        .SetWordValue(R3, -512)
        .SetByteValue(R2, 1)
        .Compare(R0, R2)
        .DeclareReference(out var winGameValueDepthOne)
        .GoToIf(C.Equal, winGameValueDepthOne)
        .AddTo(R1, R3)
        .DefineReference(winGameValueDepthOne)
        .AddTo(R1, R3)
        .DefineReference(winGameValueDepthZero)
        .CopyWordToStack(m_Stuff.Locals.WinGameValue, R1)
        .SetByteValue(R1, 10)
        .CopyByteFromStack(m_Stuff.Locals.OriginPieceColor, R0)
        .SetByteValue(R2, Piece.Colour.White)
        .Compare(R0, R2)
        .DeclareReference(out var singlePawnJumpBlack)
        .GoToIf(C.NotEqual, singlePawnJumpBlack)
        .Negate(R1)
        .DefineReference(singlePawnJumpBlack)
        .CopyWordToStack(m_Stuff.Locals.SinglePawnJump, R1)
        .CopyByteFromStack(m_Stuff.Args.Depth, R2)
        .DeclareReference(out var notDepth0AndMode1)
        .GoToIf(C.NotEqual, notDepth0AndMode1)
        .SetByteValue(R2, Modes.CheckCanMove)
        .CopyByteFromStack(m_Stuff.Args.ModeMaxDepth, R1)
        .Compare(R1, R2)
        .GoToIf(C.NotEqual, notDepth0AndMode1)
        .CopyByteFrom(m_MainGlobals + Main.Vars.SelectedSquareIndex, R0)
        .DefineReference(notDepth0AndMode1)
        // TODO
        ;

    private Assembly Call(Assembly a) => a
        .SetByteValue(R1, m_Stuff.Args.TotalBytes)
        .StackToR0()
        .SubFrom(R0, R1)
        .StackFromR0()
        .CallRoutine(m_Calculate)
        .SetByteValue(R1, m_Stuff.Args.TotalBytes)
        .StackToR0()
        .AddTo(R0, R1)
        .StackFromR0()
        .CopyWordFrom(m_Globals + m_Vars.ReturnValue, R0);

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

    private static class Modes
    {
        public const int CheckForCheck = 0,
                         CheckCanMove  = 1,
                         CalculateMove = 2;
    }

    private sealed record Globals(int NewEnPassantPawnIndex = 1,
                                  int ClickedBoardIndex = 1,
                                  int ReturnValue = 2,
                                  int RandomValue = 2);

    private readonly Globals m_Vars =
        Variables.ByteSizesToOffsets(new Globals());

    private sealed class Stuff
    {
        public Locals Locals = new ();

        #pragma warning disable CS0414
        public int ReturnAddress = 2;
        #pragma warning restore CS0414

        public Arg Args = new ();
    }

    private sealed record Locals(int JustMovedEnPassantPawnIndex = 2,
                                 int CastlingIsProhibited = 2,
                                 int MoveGameValue = 2,
                                 int TargetSquareValueAfterMoving = 2,
                                 int OtherSquareTargetIndex = 2,
                                 int OtherSquareOriginIndex = 2,
                                 int TargetSquareValue = 2,
                                 int TargetSquareIndex = 2,
                                 int MoveDirectionIndex = 2,
                                 int MoveDirectionNumber = 2,
                                 int OriginPieceIsSlidey = 2,
                                 int OriginPieceIsAKing = 2,
                                 int OriginPieceIsAPawn = 2,
                                 int OriginPieceIsOnOriginalSquare = 2,
                                 int ColorlessOriginPieceValue = 2,
                                 int MovedOriginPieceValue = 2,
                                 int OriginSquareValue = 2,
                                 int OriginSquareIndex = 2,
                                 int SinglePawnJump = 2,
                                 int WinGameValue = 2,
                                 int OriginPlayerIsInCheck = 2,
                                 int BestGameValue = 2,
                                 int OriginPieceColor = 2);

    private sealed record Arg(int MaxGameValueThatAvoidsPruning = 2,
                              int ModeMaxDepth = 2,
                              int EnPassantPawnIndex = 2,
                              int Depth = 2,
                              int OpponentPieceColor = 2);

    private sealed record NextArg(int Hack = 2, // We use '-' with these offsets
                                  int OpponentPieceColor = 2,
                                  int Depth = 2,
                                  int EnPassantPawnIndex = 2,
                                  int ModeMaxDepth = 2,
                                  int MaxGameValueThatAvoidsPruning = 2);

    private readonly Stuff m_Stuff =
        Variables.ByteSizesToOffsets(new Stuff());

    private readonly NextArg m_NextArgs =
        Variables.ByteSizesToOffsets(new NextArg());

    private readonly Reference m_Globals = new (),
                               m_BoardState = new (),
                               m_Calculate = new (),
                               m_OriginPlayerIsInCheckNotModeIsZero = new ();
}