using System.Collections.Generic;
using UnityEngine;

namespace Chess.Game
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
        [SerializeField] private Sprite squareSprite;
        [SerializeField] private PieceSprite[] pieceSprites;
        private GameObject highlightContainer;

        private Dictionary<int, Sprite> pieceSpriteDict;
        private Dictionary<int, PieceInstance> pieceInstances;
        public readonly int boardSize = 8;
        public float squareSize;

        private void CreatePiece(int piece, int rank, int file)
        {
            if (piece == Piece.None || pieceSpriteDict[piece] == null) return;

            Sprite pieceSprite = pieceSpriteDict[piece];
            GameObject pieceGameObject = new($"{pieceSprite.name}_{rank}_{file}");
            pieceGameObject.transform.parent = transform;
            pieceGameObject.transform.position = BoardToWorldPos(rank, file);

            SpriteRenderer spriteRenderer = pieceGameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = pieceSprite;

            PieceInstance instance = new(piece, new(rank, file), pieceGameObject);
            pieceInstances[GetIndex(rank, file)] = instance;
        }

        private Vector2 BoardToWorldPos(int rank, int file)
        {
            return new Vector2((file - boardSize / 2) * squareSize + squareSize / 2, (rank - boardSize / 2) * squareSize + squareSize / 2);
        }

        // Not validated by design, if move was valid only then this will get called, basically it trusts the caller
        public void MovePiece(Move move)
        {
            if (!pieceInstances.TryGetValue(move.fromSquare, out PieceInstance piece)) return;

            int rank = move.toSquare / 8;
            int file = move.toSquare % 8;

            // Take piece from destination square
            pieceInstances.TryGetValue(move.toSquare, out PieceInstance takePiece);
            if (takePiece != null) Destroy(takePiece.gameObject);
            pieceInstances.Remove(move.toSquare);

            piece.SetPosition(new(rank, file), BoardToWorldPos(rank, file));
            pieceInstances[move.toSquare] = piece;
            pieceInstances.Remove(move.fromSquare);
        }

        private int GetIndex(int rank, int file) => rank * boardSize + file;

        private void ClearBoard()
        {
            foreach (KeyValuePair<int, PieceInstance> pieceInstance in pieceInstances)
            {
                if (pieceInstance.Value != null) Destroy(pieceInstance.Value.gameObject);
            }
            pieceInstances.Clear();
        }

        private void InstantiateSquare(int index, Color squareColor, int sortingColor, string name, Transform parent)
        {
            int rank = index / 8;
            int file = index % 8;
            Vector2 worldPos = BoardToWorldPos(rank, file);

            GameObject highlightTile = new($"{name}_{rank}_{file}");
            highlightTile.transform.parent = parent;
            highlightTile.transform.position = worldPos;

            SpriteRenderer renderer = highlightTile.AddComponent<SpriteRenderer>();
            renderer.sprite = squareSprite;
            renderer.sortingOrder = sortingColor;
            renderer.color = squareColor;

            Vector2 spriteSize = squareSprite.bounds.size;
            float scaleX = squareSize / spriteSize.x;
            float scaleY = squareSize / spriteSize.y;
            highlightTile.transform.localScale = new Vector3(scaleX, scaleY, 1);
        }

        private void InstantiateSquare(int rank, int file, Color squareColor, int sortingColor, string name, Transform parent)
        {
            InstantiateSquare(rank * 8 + file, squareColor, sortingColor, name, parent);
        }

        public void GenerateBoard()
        {
            float halfBoardSize = boardSize / 2f;

            for (int rank = 0; rank < boardSize; rank++)
            {
                for (int file = 0; file < boardSize; file++)
                    InstantiateSquare(rank, file, (rank + file) % 2 != 0 ? new Color(0.91f, 0.76f, 0.65f) : new Color(0.36f, 0.25f, 0.20f), -1, "Square", this.transform);
            }
        }

        public void InitializeBoard(int[] squares)
        {
            float screenHeight = Camera.main.orthographicSize * 2f;
            float screenWidth = screenHeight * Camera.main.aspect;

            float boardWorldSize = Mathf.Min(screenWidth, screenHeight);
            squareSize = boardWorldSize / boardSize;
            GenerateBoard();

            pieceSpriteDict = new Dictionary<int, Sprite>();
            pieceInstances = new Dictionary<int, PieceInstance>();
            foreach (PieceSprite ps in pieceSprites)
            {
                pieceSpriteDict[ps.piece] = ps.sprite;
            }

            ClearBoard();
            for (int idx = 0; idx < squares.Length; idx++)
            {
                int piece = squares[idx];
                if (piece != Piece.None)
                {
                    int rank = idx / 8;
                    int file = idx % 8;
                    CreatePiece(piece, rank, file);
                }
            }

            highlightContainer = new GameObject("HighlightContainer");
            highlightContainer.transform.parent = transform;
        }

        public void HighlightMoves(int originSquareIndex, List<int> squareIndexes)
        {
            if (highlightContainer == null) return;
            RemoveHighlight();

            foreach (int squareIndex in squareIndexes)
            {
                InstantiateSquare(squareIndex, new Color(1f, 0f, 0f, 0.4f), 1, $"Highlight", highlightContainer.transform);
            }

            InstantiateSquare(originSquareIndex, new Color(1f, 0f, 0f, 0.7f), 1, $"Highlight", highlightContainer.transform);
        }

        public void RemoveHighlight()
        {
            if (highlightContainer == null) return;

            foreach (Transform child in highlightContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
