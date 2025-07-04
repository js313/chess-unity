using UnityEngine;

namespace Chess.Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public Board boardView;
        public Chess.Board board;

        void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);

            board = new Chess.Board();
            board.LoadPositionFromFEN(FenUtility.startFen);
            boardView.InitializeBoard(board.squares);
        }
    }
}
