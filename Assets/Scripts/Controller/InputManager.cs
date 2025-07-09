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
                    if (validMove.fromSquare == selectedIdx && validMove.toSquare == idx)
                    {
                        if (validMove.type == MoveType.None || validMove.type == MoveType.PawnTwoForward)
                        {
                            board.MovePiece(validMove);
                            boardView.MovePiece(validMove);

                            // Always switch turns AFTER the movement is settled
                            board.SwitchTurns();
                        }
                        else if (validMove.IsPromotion())
                        {
                            if (validMove.type == promoteTo)
                            {
                                board.PromotePawn(validMove);
                                boardView.PromotePawn(validMove, board.squares[validMove.toSquare]);    // call after board.PromotePawn, search for a beter design

                                // Always switch turns AFTER the movement is settled
                                board.SwitchTurns();
                            }
                        }
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
