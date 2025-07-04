using System.Collections.Generic;

namespace Chess
{
    public static class FenUtility
    {
        public const string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        static Dictionary<char, int> pieceTypeFromSymbol = new()
        {
            ['k'] = Piece.King,
            ['p'] = Piece.Pawn,
            ['n'] = Piece.Knight,
            ['b'] = Piece.Bishop,
            ['q'] = Piece.Queen,
            ['r'] = Piece.Rook
        };

        public class LoadedPositionInfo
        {
            public int[] squares;
            //public bool whiteCastleKingside;
            //public bool whiteCastleQueenside;
            //public bool blackCastleKingside;
            //public bool blackCastleQueenside;
            //public int epFile;
            public bool whiteToMove;
            //public int plyCount;

            public LoadedPositionInfo()
            {
                squares = new int[64];
                whiteToMove = true;
            }
        }

        public static LoadedPositionInfo PositionFromFen(string fen)
        {
            LoadedPositionInfo loadedPositionInfo = new();
            string[] sections = fen.Split(' ');

            int rank = 7, file = 0;

            foreach (char symbol in sections[0])
            {
                int idx = rank * 8 + file;
                if (symbol == '/')
                {
                    rank--;
                    file = 0;
                    continue;
                }
                else if (char.IsDigit(symbol))
                {
                    for (int skipCount = symbol - '0'; skipCount > 0; idx++, skipCount--)
                        loadedPositionInfo.squares[idx] = Piece.None;
                }
                else
                {
                    loadedPositionInfo.squares[idx] = pieceTypeFromSymbol[char.ToLower(symbol)];

                    if (char.IsLower(symbol)) loadedPositionInfo.squares[idx] |= Piece.Black;
                    else loadedPositionInfo.squares[idx] |= Piece.White;
                }
                file++;
            }

            loadedPositionInfo.whiteToMove = sections[1] == "w";

            return loadedPositionInfo;
        }
    }
}
