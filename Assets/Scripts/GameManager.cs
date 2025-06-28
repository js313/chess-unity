using UnityEngine;

namespace Chess
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        [SerializeField]
        private Board board;

        void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }

        void Start()
        {
            board.GenerateBoard();
            board.InitializeBoard(FenUtility.startFen);
        }
    }
}
