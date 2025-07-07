using System.Xml.Linq;
using System;
using Unity.VisualScripting;
using TreeEditor;

namespace Chess
{
    public enum MoveType
    {
        None,
        //Capture,
        EnPassantCapture,
        Castling,
        PromoteToQueen,
        PromoteToKnight,
        PromoteToRook,
        PromoteToBishop,
        PawnTwoForward
    }

    // Can represent this like pieces using single integer: https://github.com/SebLague/Chess-Coding-Adventure/blob/Chess-V1-Unity/Assets/Scripts/Core/Move.cs
    public readonly struct Move
    {
        public readonly int fromSquare;
        public readonly int toSquare;
        public readonly MoveType type;

        public Move(int fromSquare, int toSquare, MoveType type)
        {
            this.fromSquare = fromSquare;
            this.toSquare = toSquare;
            this.type = type;
        }

        public override string ToString()
        {
            return $"Move from {fromSquare} to {toSquare}, Type: {type}";
        }
    }
}