using System;
using System.Collections.Generic;

namespace Chess
{
    public class MoveGenerator
    {
        List<Move> moves;
        // 0 - white, 1 - black
        int friendlyColor;
        bool isWhiteToMove;
        Board board;

        readonly int[,] knightMoves = { { 2, 1 }, { 2, -1 }, { -2, 1 }, { -2, -1 }, { 1, 2 }, { 1, -2 }, { -1, 2 }, { -1, -2 } };
        readonly int[,] rookMoves = { { 0, 1 }, { 1, 0 }, { -1, 0 }, { 0, -1 } };
        readonly int[,] bishopMoves = { { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 } };
        readonly int[,] kingMoves = { { 0, 1 }, { 1, 0 }, { -1, 0 }, { 0, -1 }, { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 } };

        void Init()
        {
            moves = new List<Move>(64);
            friendlyColor = board.colorToMove == Piece.White ? 0 : 1;
            isWhiteToMove = board.colorToMove == Piece.White;
        }

        public List<Move> GenerateMoves(Board board)
        {
            this.board = board;
            Init();

            GenerateKnightMoves();
            GenerateRookMoves();
            GenerateBishopMoves();
            GenerateQueenMoves();
            GeneratePawnMoves();
            GenerateKingMoves();

            return moves;
        }

        void GenerateKnightMoves()
        {
            PieceList myknights = board.knights[friendlyColor];

            for (int i = 0; i < myknights.Count; i++)
            {
                int startSquare = myknights[i];
                int startSquareRank = startSquare / 8;
                int startSquareFile = startSquare % 8;

                for (int j = 0; j < knightMoves.GetLength(0); j++)
                {
                    int endSquareRank = startSquareRank + knightMoves[j, 0];
                    int endSquareFile = startSquareFile + knightMoves[j, 1];
                    if (endSquareFile < 0 || endSquareFile > 7 || endSquareRank < 0 || endSquareRank > 7) continue;
                    int endSquare = endSquareRank * 8 + endSquareFile;
                    if (Piece.Colour(board.squares[endSquare]) == board.colorToMove) continue;
                    moves.Add(new(startSquare, endSquare, MoveType.None));
                }
            }
        }

        void GenerateRookMoves(PieceList myQueens = null)
        {
            PieceList myRooks = myQueens ?? board.rooks[friendlyColor];

            for (int i = 0; i < myRooks.Count; i++)
            {
                int startSquare = myRooks[i];
                int startSquareRank = startSquare / 8;
                int startSquareFile = startSquare % 8;

                for (int j = 0; j < rookMoves.GetLength(0); j++)
                {
                    int endSquareRank = startSquareRank + rookMoves[j, 0];
                    int endSquareFile = startSquareFile + rookMoves[j, 1];

                    while (endSquareRank < 8 && endSquareFile < 8 && endSquareFile >= 0 && endSquareRank >= 0)
                    {
                        int endSquare = endSquareRank * 8 + endSquareFile;
                        if (Piece.Colour(board.squares[endSquare]) == board.colorToMove) break;
                        moves.Add(new(startSquare, endSquare, MoveType.None));
                        if (board.squares[endSquare] != Piece.None && Piece.Colour(board.squares[endSquare]) != board.colorToMove) break;
                        endSquareRank += rookMoves[j, 0];
                        endSquareFile += rookMoves[j, 1];
                    }
                }
            }
        }

        void GenerateBishopMoves(PieceList myQueens = null)
        {
            PieceList myBishops = myQueens ?? board.bishops[friendlyColor];

            for (int i = 0; i < myBishops.Count; i++)
            {
                int startSquare = myBishops[i];
                int startSquareRank = startSquare / 8;
                int startSquareFile = startSquare % 8;

                for (int j = 0; j < bishopMoves.GetLength(0); j++)
                {
                    int endSquareRank = startSquareRank + bishopMoves[j, 0];
                    int endSquareFile = startSquareFile + bishopMoves[j, 1];

                    while (endSquareRank < 8 && endSquareFile < 8 && endSquareFile >= 0 && endSquareRank >= 0)
                    {
                        int endSquare = endSquareRank * 8 + endSquareFile;
                        if (Piece.Colour(board.squares[endSquare]) == board.colorToMove) break;
                        moves.Add(new(startSquare, endSquare, MoveType.None));
                        if (board.squares[endSquare] != Piece.None && Piece.Colour(board.squares[endSquare]) != board.colorToMove) break;
                        endSquareRank += bishopMoves[j, 0];
                        endSquareFile += bishopMoves[j, 1];
                    }
                }
            }
        }

        void GenerateQueenMoves()
        {
            PieceList myQueens = board.queens[friendlyColor];

            GenerateBishopMoves(myQueens);
            GenerateRookMoves(myQueens);
        }

        void GeneratePawnMoves()
        {
            PieceList myPawns = board.pawns[friendlyColor];

            int pawnMove = isWhiteToMove ? 1 : -1;
            int pawnStartingRank = isWhiteToMove ? 1 : 6;
            for (int i = 0; i < myPawns.Count; i++)
            {
                int startSquare = myPawns[i];
                int startSquareRank = startSquare / 8;
                int startSquareFile = startSquare % 8;
                int endSquare = startSquare + (pawnMove * 8);
                int diagSquare = startSquare + (pawnMove * 9);
                int diagSquareFile = diagSquare % 8;
                int antiDiagSquare = startSquare + (pawnMove * 7);
                int antiDiagSquareFile = antiDiagSquare % 8;
                int endSquareDouble = startSquare + (pawnMove * 16);
                int endSquareDoubleRank = endSquare / 8;

                if (startSquareRank == 0 || startSquareRank == 7)
                {
                    // Promotion
                    continue;
                }

                if (board.squares[diagSquare] != Piece.None && Piece.Colour(board.squares[diagSquare]) != board.colorToMove && Math.Abs(startSquareFile - diagSquareFile) == 1)
                    moves.Add(new(startSquare, diagSquare, MoveType.None));
                if (board.squares[antiDiagSquare] != Piece.None && Piece.Colour(board.squares[antiDiagSquare]) != board.colorToMove && Math.Abs(startSquareFile - antiDiagSquareFile) == 1)
                    moves.Add(new(startSquare, antiDiagSquare, MoveType.None));

                if (board.squares[endSquare] != Piece.None) continue;
                moves.Add(new(startSquare, endSquare, MoveType.None));
                if (endSquareDoubleRank > 0 && endSquareDoubleRank < 7 && board.squares[endSquareDouble] == Piece.None && startSquareRank == pawnStartingRank)
                    moves.Add(new(startSquare, endSquareDouble, MoveType.PawnTwoForward));
            }
        }

        void GenerateKingMoves()
        {
            int startSquare = board.kingSquare[friendlyColor];
            int startSquareRank = startSquare / 8;
            int startSquareFile = startSquare % 8;
            for (int i = 0; i < kingMoves.GetLength(0); i++)
            {
                int endSquareRank = startSquareRank + kingMoves[i, 0];
                int endSquareFile = startSquareFile + kingMoves[i, 1];
                if (endSquareFile < 0 || endSquareFile > 7 || endSquareRank < 0 || endSquareRank > 7) continue;

                int endSquare = endSquareRank * 8 + endSquareFile;
                if (Piece.Colour(board.squares[endSquare]) == board.colorToMove) continue;
                moves.Add(new(startSquare, endSquare, MoveType.None));
            }
        }
    }
}