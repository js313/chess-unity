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

            int moveDirection = isWhiteToMove ? 8 : -8;
            int spawnRank = isWhiteToMove ? 1 : 6;
            int rankBeforePromotion = isWhiteToMove ? 6 : 1;
            int enPassantFile = ((int)(board.currentGameState >> 4) & 15) - 1;   // bits 4-7 storing enPassant file info
            int enPassantRank = isWhiteToMove ? 4 : 3;
            int enPassantSquare = -1;
            if (enPassantFile != -1)
                enPassantSquare = enPassantRank * 8 + enPassantFile;

            for (int i = 0; i < myPawns.Count; i++)
            {
                int startSquare = myPawns[i];
                int startSquareRank = startSquare / 8;
                int startSquareFile = startSquare % 8;

                bool isPromotion = startSquareRank == rankBeforePromotion;
                MoveType[] promotionTypes = isPromotion
                    ? new[] { MoveType.PromoteToQueen, MoveType.PromoteToRook, MoveType.PromoteToBishop, MoveType.PromoteToKnight }
                    : new[] { MoveType.None };

                int oneForward = startSquare + moveDirection;
                if (board.squares[oneForward] == Piece.None)
                {
                    foreach (MoveType type in promotionTypes)
                        moves.Add(new(startSquare, oneForward, type));

                    int twoForward = startSquare + 2 * moveDirection;
                    if (startSquareRank == spawnRank && board.squares[twoForward] == Piece.None)
                        moves.Add(new(startSquare, twoForward, MoveType.PawnTwoForward));
                }

                int[] diags = { -1, 1 };
                foreach (int diag in diags)
                {
                    if ((startSquareFile == 0 && diag == -1) || (startSquareFile == 7 && diag == 1)) continue;
                    int piece = board.squares[oneForward + diag];
                    if (piece == Piece.None || Piece.Colour(piece) == board.colorToMove) continue;

                    foreach (MoveType type in promotionTypes)
                        moves.Add(new(startSquare, oneForward + diag, type));
                }

                if (enPassantSquare != -1 && startSquareRank == enPassantRank && (enPassantFile + 1 == startSquareFile || enPassantFile == startSquareFile + 1))
                    moves.Add(new(startSquare, (enPassantRank * 8 + enPassantFile) + moveDirection, MoveType.EnPassantCapture));
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