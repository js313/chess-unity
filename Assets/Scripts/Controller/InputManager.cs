using System.Collections.Generic;
using UnityEngine;

namespace Chess.Game
{
    public class InputManager : MonoBehaviour
    {
        // Make a UI selection for promotion
        [SerializeField]
        private MoveType promoteTo = MoveType.PromoteToQueen;
        private Board boardView;
        private Chess.Board board;
        private int selectedIdx = -1;

        private void Start()
        {
            board = GameManager.instance.board;
            boardView = GameManager.instance.boardView;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 localPos = mouseWorldPos - (Vector2)boardView.transform.position;

                int rank = Mathf.FloorToInt(localPos.y / boardView.squareSize + boardView.boardSize / 2f);
                int file = Mathf.FloorToInt(localPos.x / boardView.squareSize + boardView.boardSize / 2f);

                if (rank < 0 || rank >= 8 || file < 0 || file >= 8)
                    return;

                int idx = rank * 8 + file;
                int piece = board.GetPieceAt(idx);

                MoveGenerator moveGenerator = new();
                List<Move> validMoves = moveGenerator.GenerateMoves(board);

                for (int moveIndex = 0; moveIndex < validMoves.Count; moveIndex++)
                {
                    Move validMove = validMoves[moveIndex];
                    if (validMove.fromSquare == selectedIdx && validMove.toSquare == idx && (!validMove.IsPromotion() || validMove.type == promoteTo))
                    {
                        board.MakeMove(validMove);
                        boardView.RefreshBoard(board.squares);
                    }
                }

                if (selectedIdx == -1 && piece != Piece.None)
                {
                    selectedIdx = idx;
                    List<int> highlightSquares = new();
                    for (int moveIndex = 0; moveIndex < validMoves.Count; moveIndex++)
                    {
                        Move validMove = validMoves[moveIndex];
                        if (validMove.fromSquare == selectedIdx)
                            highlightSquares.Add(validMove.toSquare);
                    }
                    boardView.HighlightMoves(selectedIdx, highlightSquares);
                }
                else
                {
                    boardView.RemoveHighlight();
                    selectedIdx = -1;
                }
            }
        }
    }
}
