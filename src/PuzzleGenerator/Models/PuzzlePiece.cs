using System.Drawing;
using System.Windows.Forms;

namespace PuzzleGenerator.Models
{
    public class PuzzlePiece
    {
        public PictureBox PictureBox { get; set; }
        public Point CorrectPosition { get; set; }
        public Point GridPosition { get; set; }
        public bool IsPlaced { get; set; }
        public PuzzleGroup Group { get; set; }
    }
}
