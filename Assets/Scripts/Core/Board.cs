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

        // Bits 0-3 store white and black kingside/queenside castling legality
        // Bits 4-7 store file of ep square (starting at 1, so 0 = no ep square)
        // Bits 8-13 captured piece
        // Bits 14-... fifty mover counter
        public uint currentGameState;   // Taken from SebLaq

        // Masks to TAKE AWAY rights
        const uint whiteCastleKingsideMask = 0b1111111111111110;
        const uint whiteCastleQueensideMask = 0b1111111111111101;
        const uint blackCastleKingsideMask = 0b1111111111111011;
        const uint blackCastleQueensideMask = 0b1111111111110111;

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

            if (info.epFile != -1) currentGameState |= (ushort)(((info.epFile % 8) + 1) << 4);
            if (info.whiteCastleKingside) currentGameState |= (~whiteCastleKingsideMask);
            if (info.whiteCastleQueenside) currentGameState |= (~whiteCastleQueensideMask);
            if (info.blackCastleKingside) currentGameState |= (~blackCastleKingsideMask);
            if (info.blackCastleQueenside) currentGameState |= (~blackCastleQueensideMask);
        }

        // Not validated by design, if move was valid only then this will get called, basically it trusts the caller
        public void MakeMove(Move validMove)
        {
            // reset game state, to set it again
            uint oldCastlingRights = currentGameState & 15;
            currentGameState = 0;

            // NEEDS REFACTORING
            if (validMove.type == MoveType.PawnTwoForward)
            {
                currentGameState |= (ushort)(((validMove.toSquare % 8) + 1) << 4);
            }
            else if (validMove.IsPromotion())
            {
                PromotePawn(validMove);
                return;
            }
            else if (validMove.type == MoveType.EnPassantCapture)
            {
                EnPassant(validMove);
                return;
            }
            else if (validMove.type == MoveType.Castling)
            {
                bool kingSide = validMove.toSquare == 6 || validMove.toSquare == 62;
                int rookFromSquare = kingSide ? validMove.toSquare + 1 : validMove.toSquare - 2;    // Smart!, get rookTo relative to kingTo to avoid checking color
                int rookToSquare = kingSide ? validMove.toSquare - 1 : validMove.toSquare + 1;

                MovePiece(rookFromSquare, rookToSquare);    // Move Rook
            }

            currentGameState |= oldCastlingRights;
            if (oldCastlingRights != 0)
            {
                if (squares[validMove.fromSquare] == (Piece.King | Piece.Black))
                    currentGameState &= ~(uint)12;
                else if (squares[validMove.fromSquare] == (Piece.King | Piece.White))
                    currentGameState &= ~(uint)3;
                else if (validMove.fromSquare == 0 && squares[validMove.fromSquare] == (Piece.Rook | Piece.White))
                    currentGameState &= whiteCastleQueensideMask;
                else if (validMove.fromSquare == 7 && squares[validMove.fromSquare] == (Piece.Rook | Piece.White))
                    currentGameState &= whiteCastleKingsideMask;
                else if (validMove.fromSquare == 56 && squares[validMove.fromSquare] == (Piece.Rook | Piece.Black))
                    currentGameState &= blackCastleQueensideMask;
                else if (validMove.fromSquare == 63 && squares[validMove.fromSquare] == (Piece.Rook | Piece.Black))
                    currentGameState &= blackCastleKingsideMask;
            }

            MovePiece(validMove.fromSquare, validMove.toSquare);
            SwitchTurns();
        }

        void PromotePawn(Move validMove)
        {
            int colorIndex = Piece.Colour(squares[validMove.fromSquare]) == Piece.White ? WhiteIndex : BlackIndex;

            // Doing this everywhere, REFACTOR
            PieceList piece = GetPieceListAt(validMove.fromSquare);
            piece?.RemovePieceAtSquare(validMove.fromSquare);
            PieceList pieceAtDestination = GetPieceListAt(validMove.toSquare);
            pieceAtDestination?.RemovePieceAtSquare(validMove.toSquare);

            squares[validMove.fromSquare] = 0;
            if (validMove.type == MoveType.PromoteToQueen)
            {
                queens[colorIndex].AddPieceAtSquare(validMove.toSquare);
                squares[validMove.toSquare] = Piece.Queen | colorToMove;
            }
            else if (validMove.type == MoveType.PromoteToKnight)
            {
                knights[colorIndex].AddPieceAtSquare(validMove.toSquare);
                squares[validMove.toSquare] = Piece.Knight | colorToMove;
            }
            else if (validMove.type == MoveType.PromoteToRook)
            {
                rooks[colorIndex].AddPieceAtSquare(validMove.toSquare);
                squares[validMove.toSquare] = Piece.Rook | colorToMove;
            }
            else if (validMove.type == MoveType.PromoteToBishop)
            {
                bishops[colorIndex].AddPieceAtSquare(validMove.toSquare);
                squares[validMove.toSquare] = Piece.Bishop | colorToMove;
            }
            SwitchTurns();
        }

        void EnPassant(Move validMove)
        {
            int captureDirection = colorToMove == Piece.White ? -8 : 8;
            int squareToCapture = validMove.toSquare + captureDirection;

            // Doing this everywhere, REFACTOR
            PieceList pieceAtDestination = GetPieceListAt(squareToCapture);
            pieceAtDestination?.RemovePieceAtSquare(squareToCapture);

            PieceList piece = GetPieceListAt(validMove.fromSquare);
            piece?.MovePiece(validMove.fromSquare, validMove.toSquare);

            squares[squareToCapture] = 0;
            squares[validMove.toSquare] = squares[validMove.fromSquare];
            squares[validMove.fromSquare] = 0;
            SwitchTurns();
        }

        void MovePiece(int fromSquare, int toSquare)
        {
            int colorIndex = Piece.Colour(squares[fromSquare]) == Piece.White ? WhiteIndex : BlackIndex;
            PieceList piece = GetPieceListAt(fromSquare);

            PieceList pieceAtDestination = GetPieceListAt(toSquare);
            pieceAtDestination?.RemovePieceAtSquare(toSquare);

            if (piece == null && Piece.PieceType(squares[fromSquare]) == Piece.King)
                kingSquare[colorIndex] = toSquare;
            else
                piece?.MovePiece(fromSquare, toSquare);

            // Update the board representation
            squares[toSquare] = squares[fromSquare];
            squares[fromSquare] = 0;
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

        private void SwitchTurns()
        {
            colorToMove = Piece.Colour(colorToMove) == Piece.White ? Piece.Black : Piece.White;
        }
    }
}