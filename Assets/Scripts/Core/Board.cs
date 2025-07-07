namespace Chess
{
    public class Board
    {
        public int[] squares = new int[64];
        public int colorToMove;

        public const int WhiteIndex = 0;
        public const int BlackIndex = 1;

        // Array will always be of size 2 for black and white
        public int[] kingSquare;
        public PieceList[] rooks;
        public PieceList[] bishops;
        public PieceList[] queens;
        public PieceList[] knights;
        public PieceList[] pawns;

        public Board()
        {
            knights = new PieceList[] { new(10), new(10) };
            pawns = new PieceList[] { new(8), new(8) };
            rooks = new PieceList[] { new(10), new(10) };
            bishops = new PieceList[] { new(10), new(10) };
            queens = new PieceList[] { new(9), new(9) };
            kingSquare = new int[2];
        }

        public void LoadPositionFromFEN(string fen)
        {
            FenUtility.LoadedPositionInfo info = FenUtility.PositionFromFen(fen);
            squares = info.squares;
            colorToMove = info.whiteToMove ? Piece.White : Piece.Black;

            for (int squareIndex = 0; squareIndex < squares.Length; squareIndex++)
            {
                int colorIndex = Piece.Colour(squares[squareIndex]) == Piece.White ? WhiteIndex : BlackIndex;
                PieceList piece = GetPieceListAt(squareIndex);
                if (piece == null && Piece.PieceType(squares[squareIndex]) == Piece.King)
                    kingSquare[colorIndex] = squareIndex;
                else
                    piece?.AddPieceAtSquare(squareIndex);
            }
        }

        // Not validated by design, if move was valid only then this will get called, basically it trusts the caller
        public void MovePiece(Move move)
        {
            // Uppdate piece list
            int colorIndex = Piece.Colour(squares[move.fromSquare]) == Piece.White ? WhiteIndex : BlackIndex;
            PieceList piece = GetPieceListAt(move.fromSquare);
            PieceList pieceAtDestination = GetPieceListAt(move.toSquare);
            pieceAtDestination?.RemovePieceAtSquare(move.toSquare);
            if (piece == null && Piece.PieceType(squares[move.fromSquare]) == Piece.King)
                kingSquare[colorIndex] = move.toSquare;
            else
                piece?.MovePiece(move.fromSquare, move.toSquare);

            // Update the board representation
            squares[move.toSquare] = squares[move.fromSquare];
            squares[move.fromSquare] = 0;
        }

        public int GetPieceAt(int idx) => squares[idx];

        private PieceList GetPieceListAt(int idx)
        {
            int colorIndex = Piece.Colour(squares[idx]) == Piece.White ? WhiteIndex : BlackIndex;

            if (Piece.PieceType(squares[idx]) == Piece.Rook)
                return rooks[colorIndex];
            else if (Piece.PieceType(squares[idx]) == Piece.Knight)
                return knights[colorIndex];
            else if (Piece.PieceType(squares[idx]) == Piece.Queen)
                return queens[colorIndex];
            else if (Piece.PieceType(squares[idx]) == Piece.Bishop)
                return bishops[colorIndex];
            else if (Piece.PieceType(squares[idx]) == Piece.Pawn)
                return pawns[colorIndex];

            return null;
        }

        public void SwitchTurns()
        {
            colorToMove = Piece.Colour(colorToMove) == Piece.White ? Piece.Black : Piece.White;
        }
    }
}