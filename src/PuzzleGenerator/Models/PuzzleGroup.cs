using System.Collections.Generic;

namespace PuzzleGenerator.Models
{
    public class PuzzleGroup
    {
        public List<PuzzlePiece> Pieces { get; set; } = new List<PuzzlePiece>();

        public void AddPiece(PuzzlePiece piece)
        {
            Pieces.Add(piece);
            piece.Group = this;
        }
    }
}
