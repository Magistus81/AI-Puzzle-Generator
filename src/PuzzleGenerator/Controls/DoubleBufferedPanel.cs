using System.Windows.Forms;

namespace PuzzleGenerator.Controls
{
    public class DoubleBufferedPanel : Panel
    {
        private const int WsExComposited = 0x02000000;

        public DoubleBufferedPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            DoubleBuffered = true;
            ResizeRedraw = true;
            UpdateStyles();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= WsExComposited;
                return createParams;
            }
        }
    }
}
