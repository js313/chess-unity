using System.Collections.Generic;

namespace Chess
{
    public class MoveGenerator
    {
        List<Move> moves;
        // 0 - white, 1 - black
        int friendlyColor;
        Board board;

        readonly int[,] knightMoves = { { 2, 1 }, { 2, -1 }, { -2, 1 }, { -2, -1 }, { 1, 2 }, { 1, -2 }, { -1, 2 }, { -1, -2 } };
        readonly int[,] rookMoves = { { 0, 1 }, { 1, 0 }, { -1, 0 }, { 0, -1 } };
        readonly int[,] bishopMoves = { { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 } };

        void Init()
        {
            moves = new List<Move>(64);
            friendlyColor = board.colorToMove == Piece.White ? 0 : 1;
        }

        public List<Move> GenerateMoves(Board board)
        {
            this.board = board;
            Init();

            GenerateKnightMoves();
            GenerateRookMoves();
            GenerateBishopMoves();
            GenerateQueenMoves();

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

                for (int j = 0; j < bishopMoves.GetLength(0); j++)
                {
                    int endSquareRank = startSquareRank + rookMoves[j, 0];
                    int endSquareFile = startSquareFile + rookMoves[j, 1];

                    while (endSquareRank < 8 && endSquareFile < 8 && endSquareFile >= 0 && endSquareRank >= 0)
                    {
                        int endSquare = endSquareRank * 8 + endSquareFile;
                        if (Piece.Colour(board.squares[endSquare]) == board.colorToMove) break;
                        moves.Add(new(startSquare, endSquare, MoveType.None));
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
    }
}