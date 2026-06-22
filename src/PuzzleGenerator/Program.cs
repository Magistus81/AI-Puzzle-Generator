using System;
using System.Windows.Forms;
using PuzzleGenerator.Forms;

namespace PuzzleGenerator
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PuzzleForm());
        }
    }
}
