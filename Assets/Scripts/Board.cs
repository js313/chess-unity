using System.Collections.Generic;
using UnityEngine;

namespace Chess
{
    [System.Serializable]
    public struct PieceSprite
    {
        public string name;
        public int piece;
        public Sprite sprite;
    }

    public class Board : MonoBehaviour
    {
        [Header("Board Size Info")]
        [SerializeField]
        private Sprite squareSprite;
        private float squareSize = 1;
        private readonly int boardSize = 8;

        [Header("Pieces Info")]
        [SerializeField]
        private PieceSprite[] pieceSprites;
        private Dictionary<int, Sprite> pieceSpriteDict;
        private GameObject[] pieceObjects;
        private int selectedPieceIdx;

        private FenUtility.LoadedPositionInfo loadedPositionInfo;

        private void Awake()
        {
            float screenHeight = Camera.main.orthographicSize * 2f;
            float screenWidth = screenHeight * Camera.main.aspect;

            float boardWorldSize = Mathf.Min(screenWidth, screenHeight);
            squareSize = boardWorldSize / boardSize;

            pieceSpriteDict = new Dictionary<int, Sprite>();
            pieceObjects = new GameObject[boardSize * boardSize];
            foreach (PieceSprite ps in pieceSprites)
            {
                pieceSpriteDict[ps.piece] = ps.sprite;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                Vector2 localPos = mouseWorldPos - (Vector2)transform.position;

                float halfBoardSize = boardSize / 2f;
                int file = Mathf.FloorToInt(localPos.x / squareSize + halfBoardSize);
                int rank = Mathf.FloorToInt(localPos.y / squareSize + halfBoardSize);

                if (file >= 0 && file < boardSize && rank >= 0 && rank < boardSize)
                {
                    int idx = rank * boardSize + file;
                    int currentPiece = loadedPositionInfo.squares[idx];
                    if (currentPiece == 0 && selectedPieceIdx != -1)
                    {
                        MovePieceTo(selectedPieceIdx, rank, file);
                        selectedPieceIdx = -1;
                    }
                    else
                    {
                        selectedPieceIdx = idx;
                    }
                }
            }
        }

        private void CreatePiece(int piece, int rank, int file)
        {
            if (piece == Piece.None || pieceSpriteDict[piece] == null) return;

            Sprite pieceSprite = pieceSpriteDict[piece];
            GameObject pieceObject = new($"{pieceSprite.name}_{rank}_{file}");
            pieceObject.transform.parent = transform;
            pieceObject.transform.position = BoardToWorldPos(rank, file);

            SpriteRenderer spriteRenderer = pieceObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = pieceSprite;

            pieceObjects[rank * boardSize + file] = pieceObject;
        }

        private Vector2 BoardToWorldPos(int rank, int file)
        {
            return new Vector2((file - boardSize / 2) * squareSize + squareSize / 2, (rank - boardSize / 2) * squareSize + squareSize / 2);
        }

        private void MovePieceTo(int pieceIdx, int rank, int file)
        {
            int idx = rank * boardSize + file;
            int piece = loadedPositionInfo.squares[pieceIdx];

            loadedPositionInfo.squares[idx] = piece;
            pieceObjects[pieceIdx].transform.position = BoardToWorldPos(rank, file);
            pieceObjects[idx] = pieceObjects[pieceIdx];
            pieceObjects[pieceIdx] = null;
            loadedPositionInfo.squares[pieceIdx] = 0;
        }

        public void GenerateBoard()
        {
            float halfBoardSize = boardSize / 2f;

            for (int rank = 0; rank < boardSize; rank++)
            {
                for (int file = 0; file < boardSize; file++)
                {
                    GameObject squareObj = new($"Square_{rank}_{file}");
                    squareObj.transform.parent = this.transform;

                    float x = (rank - halfBoardSize) * squareSize + squareSize / 2f;
                    float y = (file - halfBoardSize) * squareSize + squareSize / 2f;

                    squareObj.transform.localPosition = new Vector3(x, y, 0);

                    SpriteRenderer renderer = squareObj.AddComponent<SpriteRenderer>();
                    renderer.sprite = squareSprite;
                    renderer.sortingOrder = -1;

                    Vector2 spriteSize = squareSprite.bounds.size;
                    float scaleX = squareSize / spriteSize.x;
                    float scaleY = squareSize / spriteSize.y;
                    squareObj.transform.localScale = new Vector3(scaleX, scaleY, 1);

                    renderer.color = (rank + file) % 2 != 0 ? new Color(0.91f, 0.76f, 0.65f) : new Color(0.36f, 0.25f, 0.20f);
                }
            }
        }

        public void InitializeBoard(string fen)
        {
            loadedPositionInfo = FenUtility.PositionFromFen(fen);

            for (int rank = 0; rank < boardSize; rank++)
            {
                for (int file = 0; file < boardSize; file++)
                {
                    int idx = rank * boardSize + file;
                    int piece = loadedPositionInfo.squares[idx];
                    CreatePiece(piece, rank, file);
                }
            }
        }
    }
}
