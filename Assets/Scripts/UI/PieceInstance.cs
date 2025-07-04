using UnityEngine;

namespace Chess.Game
{
    public class PieceInstance
    {
        public int piece;
        public Vector2Int position;
        public GameObject gameObject;

        public PieceInstance(int piece, Vector2Int position, GameObject gameObject)
        {
            this.piece = piece;
            this.position = position;
            this.gameObject = gameObject;
        }

        public void SetPosition(Vector2Int newPos, Vector2 worldPos)
        {
            position = newPos;
            if (gameObject != null)
            {
                gameObject.transform.position = worldPos;
            }
        }
    }
}
