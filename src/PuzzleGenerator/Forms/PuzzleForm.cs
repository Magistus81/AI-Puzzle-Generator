using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PuzzleGenerator.Controls;
using PuzzleGenerator.Models;
using PuzzleGenerator.Services;

namespace PuzzleGenerator.Forms
{
    public class PuzzleForm : Form
    {
        private Button uploadButton;
        private Button generateButton;
        private Button pauseButton;
        private ComboBox pieceCountComboBox;
        private PictureBox uploadedPictureBox;
        private DoubleBufferedPanel puzzlePanel;
        private Label timerLabel;
        private Label topTimesLabel;
        private List<Label> topTimeLabels;
        private Label resizeHintLabel;

        private Bitmap uploadedImage;
        private Bitmap scaledImage;
        private List<PuzzlePiece> puzzlePieces;
        private int rows;
        private int cols;
        private bool isGameActive;
        private bool isGamePaused;

        private PuzzlePiece lastSelectedPuzzlePiece;
        private System.Windows.Forms.Timer moveTimer;
        private Keys currentKey;

        private System.Windows.Forms.Timer gameTimer;
        private int elapsedTimeInSeconds;

        private Dictionary<string, List<int>> topTimes;
        private ScoreService scoreService;

        private bool isResizing;
        private Point resizeStartPoint;
        private Size resizeStartPanelSize;

        private bool isDragging;
        private Point clickPosition;
        private PuzzlePiece draggedPuzzlePiece;

        public PuzzleForm()
        {
            Text = "Puzzle Generator";
            StartPosition = FormStartPosition.CenterScreen;
            AutoScroll = true;
            TabStop = false;
            KeyPreview = true;
            KeyDown += PuzzleForm_KeyDown;
            KeyUp += PuzzleForm_KeyUp;

            moveTimer = new System.Windows.Forms.Timer();
            moveTimer.Interval = 50;
            moveTimer.Tick += MoveTimer_Tick;

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 1000;
            gameTimer.Tick += GameTimer_Tick;

            elapsedTimeInSeconds = 0;
            topTimes = new Dictionary<string, List<int>>();
            scoreService = new ScoreService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TopTimes.json"));

            InitializeComponents();

            pieceCountComboBox.SelectedIndexChanged += PieceCountComboBox_SelectedIndexChanged;

            LoadTopTimes();
        }

        private void InitializeComponents()
        {
            uploadButton = new Button()
            {
                Text = "Upload Image",
                Left = 10,
                Top = 10,
                Width = 120,
                TabStop = false
            };
            uploadButton.Click += UploadButton_Click;

            pieceCountComboBox = new ComboBox()
            {
                Left = uploadButton.Right + 10,
                Top = 10,
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                TabStop = false
            };

            generateButton = new Button()
            {
                Text = "Generate Puzzle",
                Left = pieceCountComboBox.Right + 10,
                Top = 10,
                Width = 120,
                TabStop = false
            };
            generateButton.Click += GenerateButton_Click;

            timerLabel = new Label()
            {
                Text = TimeFormatter.FormatTimerLabel(0),
                Left = generateButton.Right + 10,
                Top = 15,
                AutoSize = true,
                TabStop = false
            };

            pauseButton = new Button()
            {
                Text = "Pause",
                Left = timerLabel.Right + 10,
                Top = 10,
                Width = 80,
                Visible = false,
                TabStop = false
            };
            pauseButton.Click += PauseButton_Click;

            uploadedPictureBox = new PictureBox()
            {
                Left = 10,
                Top = uploadButton.Bottom + 10,
                Width = 300,
                Height = 300,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                TabStop = false
            };

            topTimesLabel = new Label()
            {
                Text = "Top 5 Results:",
                Left = uploadedPictureBox.Left,
                Top = uploadedPictureBox.Bottom + 10,
                AutoSize = true,
                TabStop = false
            };

            topTimeLabels = new List<Label>();
            int labelTop = topTimesLabel.Bottom + 5;
            for (int i = 0; i < 5; i++)
            {
                Label lbl = new Label()
                {
                    Text = $"#{i + 1}: ",
                    Left = uploadedPictureBox.Left,
                    Top = labelTop,
                    AutoSize = true,
                    TabStop = false
                };
                labelTop = lbl.Bottom + 5;
                topTimeLabels.Add(lbl);
                Controls.Add(lbl);
            }

            puzzlePanel = new DoubleBufferedPanel()
            {
                Left = uploadedPictureBox.Right + 10,
                Top = uploadButton.Bottom + 10,
                Width = 600,
                Height = 600,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                TabStop = false
            };

            puzzlePanel.MouseDown += PuzzlePanel_MouseDown;
            puzzlePanel.MouseMove += PuzzlePanel_MouseMove;
            puzzlePanel.MouseUp += PuzzlePanel_MouseUp;

            Controls.Add(uploadButton);
            Controls.Add(pieceCountComboBox);
            Controls.Add(generateButton);
            Controls.Add(timerLabel);
            Controls.Add(pauseButton);
            Controls.Add(uploadedPictureBox);
            Controls.Add(topTimesLabel);
            Controls.Add(puzzlePanel);

            resizeHintLabel = new Label()
            {
                Text = "Hint: You can resize the game area by dragging the bottom-right corner with the mouse.",
                Left = puzzlePanel.Left,
                AutoSize = true,
                ForeColor = Color.Blue,
                TabStop = false
            };
            Controls.Add(resizeHintLabel);

            UpdateResizeHintLabelPosition();
            GeneratePieceCounts();

            if (pieceCountComboBox.Items.Count > 0)
            {
                pieceCountComboBox.SelectedIndex = 0;
            }

            ResizeFormToContent();

            FormClosing += PuzzleForm_FormClosing;
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        private void PauseGame()
        {
            isGamePaused = true;
            gameTimer.Stop();
            pauseButton.Text = "Resume";

            foreach (PuzzlePiece puzzlePiece in puzzlePieces)
            {
                puzzlePiece.PictureBox.Visible = false;
            }
        }

        private void ResumeGame()
        {
            isGamePaused = false;
            gameTimer.Start();
            pauseButton.Text = "Pause";

            foreach (PuzzlePiece puzzlePiece in puzzlePieces)
            {
                puzzlePiece.PictureBox.Visible = true;
            }
        }

        private void UpdateResizeHintLabelPosition()
        {
            int margin = 5;
            resizeHintLabel.Left = puzzlePanel.Left;
            resizeHintLabel.Top = puzzlePanel.Bottom + margin;
        }

        private void ResizeFormToContent()
        {
            int formWidth = puzzlePanel.Right + 20;
            int formHeight = Math.Max(resizeHintLabel.Bottom + 40, topTimeLabels[topTimeLabels.Count - 1].Bottom + 40);
            ClientSize = new Size(formWidth, formHeight);
        }

        private void GeneratePieceCounts()
        {
            var pieceCounts = new List<string>
            {
                "9 pieces (3x3)",
                "16 pieces (4x4)",
                "25 pieces (5x5)",
                "64 pieces (8x8)",
                "100 pieces (10x10)"
            };

            pieceCountComboBox.DataSource = pieceCounts;
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.jpg;*.png)|*.jpg;*.png";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                uploadedImage = new Bitmap(ofd.FileName);
                uploadedPictureBox.Image = uploadedImage;
            }
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            if (uploadedImage == null)
            {
                MessageBox.Show("Please upload an image first.");
                return;
            }

            GeneratePuzzle();
        }

        private void GeneratePuzzle()
        {
            string selectedOption = pieceCountComboBox.SelectedItem.ToString();
            ParseSelectedOption(selectedOption);

            ScaleImageToFitPanel();

            SplitImage();
            ShufflePieces();
            DisplayPuzzle();

            elapsedTimeInSeconds = 0;
            timerLabel.Text = TimeFormatter.FormatTimerLabel(elapsedTimeInSeconds);
            gameTimer.Start();
            isGameActive = true;
            isGamePaused = false;
            pauseButton.Text = "Pause";
            pauseButton.Visible = true;

            UpdateTopTimesDisplay();
            UpdateResizeHintLabelPosition();
            ResizeFormToContent();
        }

        private void ParseSelectedOption(string selectedOption)
        {
            string[] parts = selectedOption.Split(new char[] { '(', 'x', ')' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                int.TryParse(parts[1], out rows);
                int.TryParse(parts[2], out cols);
            }
        }

        private void ScaleImageToFitPanel()
        {
            int maxWidth = puzzlePanel.Width - 50;
            int maxHeight = puzzlePanel.Height - 50;

            int newWidth = uploadedImage.Width;
            int newHeight = uploadedImage.Height;

            double aspectRatio = (double)uploadedImage.Width / uploadedImage.Height;

            if (aspectRatio >= 1)
            {
                newWidth = maxWidth;
                newHeight = (int)(maxWidth / aspectRatio);
                if (newHeight > maxHeight)
                {
                    newHeight = maxHeight;
                    newWidth = (int)(maxHeight * aspectRatio);
                }
            }
            else
            {
                newHeight = maxHeight;
                newWidth = (int)(maxHeight * aspectRatio);
                if (newWidth > maxWidth)
                {
                    newWidth = maxWidth;
                    newHeight = (int)(maxWidth / aspectRatio);
                }
            }

            scaledImage = new Bitmap(uploadedImage, new Size(newWidth, newHeight));
        }

        private void SplitImage()
        {
            puzzlePieces = new List<PuzzlePiece>();
            int pieceWidth = scaledImage.Width / cols;
            int pieceHeight = scaledImage.Height / rows;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Rectangle rect = new Rectangle(x * pieceWidth, y * pieceHeight, pieceWidth, pieceHeight);
                    Bitmap pieceImage = scaledImage.Clone(rect, scaledImage.PixelFormat);

                    PictureBox pb = new PictureBox();
                    pb.Image = pieceImage;
                    pb.Width = pieceWidth;
                    pb.Height = pieceHeight;
                    pb.SizeMode = PictureBoxSizeMode.StretchImage;
                    pb.BorderStyle = BorderStyle.None;

                    PuzzlePiece puzzlePiece = new PuzzlePiece
                    {
                        PictureBox = pb,
                        CorrectPosition = new Point(x * pieceWidth, y * pieceHeight),
                        GridPosition = new Point(x, y)
                    };

                    pb.MouseDown += Pb_MouseDown;
                    pb.MouseMove += Pb_MouseMove;
                    pb.MouseUp += Pb_MouseUp;

                    PuzzleGroup group = new PuzzleGroup();
                    group.AddPiece(puzzlePiece);

                    puzzlePieces.Add(puzzlePiece);
                }
            }
        }

        private void ShufflePieces()
        {
            Random rng = new Random();
            int n = puzzlePieces.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                PuzzlePiece value = puzzlePieces[k];
                puzzlePieces[k] = puzzlePieces[n];
                puzzlePieces[n] = value;
            }
        }

        private void DisplayPuzzle()
        {
            puzzlePanel.Controls.Clear();

            List<Point> availablePositions = GenerateAvailablePositions();
            Random rnd = new Random();

            foreach (PuzzlePiece puzzlePiece in puzzlePieces)
            {
                PictureBox pb = puzzlePiece.PictureBox;
                puzzlePiece.IsPlaced = false;

                if (availablePositions.Count > 0)
                {
                    int index = rnd.Next(availablePositions.Count);
                    Point position = availablePositions[index];
                    availablePositions.RemoveAt(index);

                    pb.Left = position.X;
                    pb.Top = position.Y;
                }
                else
                {
                    pb.Left = (puzzlePanel.Width - pb.Width) / 2;
                    pb.Top = (puzzlePanel.Height - pb.Height) / 2;
                }

                puzzlePanel.Controls.Add(pb);
                pb.BringToFront();
            }
        }

        private List<Point> GenerateAvailablePositions()
        {
            List<Point> positions = new List<Point>();
            int pieceWidth = scaledImage.Width / cols;
            int pieceHeight = scaledImage.Height / rows;

            int horizontalSpaces = puzzlePanel.Width / pieceWidth;
            int verticalSpaces = puzzlePanel.Height / pieceHeight;

            for (int y = 0; y < verticalSpaces; y++)
            {
                for (int x = 0; x < horizontalSpaces; x++)
                {
                    positions.Add(new Point(x * pieceWidth, y * pieceHeight));
                }
            }

            return positions;
        }

        private void Pb_MouseDown(object sender, MouseEventArgs e)
        {
            if (!isGameActive || isGamePaused)
            {
                return;
            }

            PictureBox pb = sender as PictureBox;
            PuzzlePiece puzzlePiece = puzzlePieces.Find(p => p.PictureBox == pb);

            if (puzzlePiece.IsPlaced)
            {
                return;
            }

            draggedPuzzlePiece = puzzlePiece;
            isDragging = true;
            clickPosition = e.Location;

            foreach (PuzzlePiece piece in puzzlePiece.Group.Pieces)
            {
                piece.PictureBox.BringToFront();
            }

            lastSelectedPuzzlePiece = draggedPuzzlePiece;
            Focus();
        }

        private void Pb_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isGamePaused && isDragging && draggedPuzzlePiece != null)
            {
                Point p = puzzlePanel.PointToClient(Cursor.Position);
                int deltaX = p.X - clickPosition.X - draggedPuzzlePiece.PictureBox.Left;
                int deltaY = p.Y - clickPosition.Y - draggedPuzzlePiece.PictureBox.Top;

                MoveGroup(draggedPuzzlePiece.Group, deltaX, deltaY);
            }
        }

        private void MoveGroup(PuzzleGroup group, int deltaX, int deltaY)
        {
            if (group == null || (deltaX == 0 && deltaY == 0))
            {
                return;
            }

            puzzlePanel.SuspendLayout();

            try
            {
                foreach (PuzzlePiece piece in group.Pieces)
                {
                    PictureBox pictureBox = piece.PictureBox;
                    pictureBox.SetBounds(
                        pictureBox.Left + deltaX,
                        pictureBox.Top + deltaY,
                        pictureBox.Width,
                        pictureBox.Height);
                }
            }
            finally
            {
                puzzlePanel.ResumeLayout(false);
                puzzlePanel.Invalidate();
            }
        }

        private void Pb_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isGameActive || isGamePaused)
            {
                return;
            }

            isDragging = false;

            if (draggedPuzzlePiece != null)
            {
                CheckForConnection(draggedPuzzlePiece);
                CheckGroupPlacement(draggedPuzzlePiece.Group);
            }

            draggedPuzzlePiece = null;
        }

        private void CheckForConnection(PuzzlePiece piece)
        {
            foreach (PuzzlePiece otherPiece in puzzlePieces)
            {
                if (otherPiece == piece || otherPiece.Group == piece.Group || otherPiece.IsPlaced)
                {
                    continue;
                }

                if (ArePiecesAdjacent(piece, otherPiece, out Point offset))
                {
                    MergeGroups(piece.Group, otherPiece.Group, offset);
                    break;
                }
            }
        }

        private bool ArePiecesAdjacent(PuzzlePiece piece1, PuzzlePiece piece2, out Point offset)
        {
            offset = Point.Empty;
            int tolerance = 5;

            int dx = piece1.GridPosition.X - piece2.GridPosition.X;
            int dy = piece1.GridPosition.Y - piece2.GridPosition.Y;

            if ((Math.Abs(dx) == 1 && dy == 0) || (dx == 0 && Math.Abs(dy) == 1))
            {
                Point expectedPosition = new Point(
                    piece1.PictureBox.Left - dx * piece1.PictureBox.Width,
                    piece1.PictureBox.Top - dy * piece1.PictureBox.Height
                );

                int deltaX = Math.Abs(piece2.PictureBox.Left - expectedPosition.X);
                int deltaY = Math.Abs(piece2.PictureBox.Top - expectedPosition.Y);

                if (deltaX <= tolerance && deltaY <= tolerance)
                {
                    offset = new Point(
                        piece2.PictureBox.Left - expectedPosition.X,
                        piece2.PictureBox.Top - expectedPosition.Y);
                    return true;
                }
            }

            return false;
        }

        private void MergeGroups(PuzzleGroup group1, PuzzleGroup group2, Point offset)
        {
            MoveGroup(group2, -offset.X, -offset.Y);

            foreach (PuzzlePiece piece in group2.Pieces)
            {
                group1.AddPiece(piece);
            }

            group2.Pieces.Clear();
        }

        private void CheckGroupPlacement(PuzzleGroup group)
        {
            int tolerance = 5;
            bool allPiecesInPlace = true;

            foreach (PuzzlePiece piece in group.Pieces)
            {
                int deltaX = Math.Abs(piece.PictureBox.Left - piece.CorrectPosition.X);
                int deltaY = Math.Abs(piece.PictureBox.Top - piece.CorrectPosition.Y);

                if (deltaX > tolerance || deltaY > tolerance)
                {
                    allPiecesInPlace = false;
                    break;
                }
            }

            if (allPiecesInPlace)
            {
                foreach (PuzzlePiece piece in group.Pieces)
                {
                    piece.PictureBox.Left = piece.CorrectPosition.X;
                    piece.PictureBox.Top = piece.CorrectPosition.Y;
                    piece.IsPlaced = true;
                    piece.PictureBox.SendToBack();
                }

                CheckPuzzleCompletion();
            }
        }

        private void PuzzleForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isGameActive || isGamePaused)
            {
                return;
            }

            if (lastSelectedPuzzlePiece != null && !lastSelectedPuzzlePiece.IsPlaced)
            {
                currentKey = e.KeyCode;
                moveTimer.Start();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void PuzzleForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (!isGameActive || isGamePaused)
            {
                return;
            }

            if (lastSelectedPuzzlePiece != null && !lastSelectedPuzzlePiece.IsPlaced)
            {
                moveTimer.Stop();
                currentKey = Keys.None;
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void MoveTimer_Tick(object sender, EventArgs e)
        {
            if (!isGamePaused && lastSelectedPuzzlePiece != null && !lastSelectedPuzzlePiece.IsPlaced)
            {
                int moveAmount = 2;

                switch (currentKey)
                {
                    case Keys.Left:
                        lastSelectedPuzzlePiece.PictureBox.Left -= moveAmount;
                        break;
                    case Keys.Right:
                        lastSelectedPuzzlePiece.PictureBox.Left += moveAmount;
                        break;
                    case Keys.Up:
                        lastSelectedPuzzlePiece.PictureBox.Top -= moveAmount;
                        break;
                    case Keys.Down:
                        lastSelectedPuzzlePiece.PictureBox.Top += moveAmount;
                        break;
                }

                EnsurePieceWithinBounds(lastSelectedPuzzlePiece.PictureBox);
            }
        }

        private void EnsurePieceWithinBounds(PictureBox piece)
        {
            if (piece.Left < 0)
            {
                piece.Left = 0;
            }

            if (piece.Top < 0)
            {
                piece.Top = 0;
            }

            if (piece.Right > puzzlePanel.Width)
            {
                piece.Left = puzzlePanel.Width - piece.Width;
            }

            if (piece.Bottom > puzzlePanel.Height)
            {
                piece.Top = puzzlePanel.Height - piece.Height;
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            elapsedTimeInSeconds++;
            timerLabel.Text = TimeFormatter.FormatTimerLabel(elapsedTimeInSeconds);
        }

        private void CheckPuzzleCompletion()
        {
            if (!isGameActive)
            {
                return;
            }

            foreach (PuzzlePiece puzzlePiece in puzzlePieces)
            {
                if (!puzzlePiece.IsPlaced)
                {
                    return;
                }
            }

            gameTimer.Stop();
            isGameActive = false;
            isGamePaused = false;
            pauseButton.Visible = false;

            string puzzleSizeKey = $"{rows}x{cols}";
            var scoreResult = scoreService.RecordCompletionTime(topTimes, puzzleSizeKey, elapsedTimeInSeconds);
            string completionTime = TimeFormatter.FormatCompletionTime(elapsedTimeInSeconds);

            UpdateTopTimesDisplay();
            SaveTopTimes();

            if (scoreResult.IsTopTime)
            {
                MessageBox.Show(
                    $"Congratulations! You completed the puzzle in {completionTime}.\nYour result is #{scoreResult.Position} in the top 5 for this puzzle.",
                    "Puzzle Completed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    $"Congratulations! You completed the puzzle in {completionTime}.",
                    "Puzzle Completed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void UpdateTopTimesDisplay()
        {
            string puzzleSizeKey = GetCurrentPuzzleSizeKey();

            foreach (Label lbl in topTimeLabels)
            {
                lbl.Text = "";
            }

            if (topTimes.ContainsKey(puzzleSizeKey))
            {
                List<int> timesList = topTimes[puzzleSizeKey];

                for (int i = 0; i < timesList.Count; i++)
                {
                    topTimeLabels[i].Text = $"#{i + 1}: {TimeFormatter.FormatTopTime(timesList[i])}";
                }
            }
            else
            {
                topTimeLabels[0].Text = "No recorded results.";
            }
        }

        private string GetCurrentPuzzleSizeKey()
        {
            string selectedOption = pieceCountComboBox.SelectedItem.ToString();
            int startIndex = selectedOption.IndexOf('(');
            int endIndex = selectedOption.IndexOf(')');
            if (startIndex >= 0 && endIndex > startIndex)
            {
                return selectedOption.Substring(startIndex + 1, endIndex - startIndex - 1);
            }

            return $"{rows}x{cols}";
        }

        private void PieceCountComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTopTimesDisplay();
        }

        private void PuzzleForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveTopTimes();
        }

        private void SaveTopTimes()
        {
            try
            {
                scoreService.SaveTopTimes(topTimes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving top times: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTopTimes()
        {
            try
            {
                topTimes = scoreService.LoadTopTimes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading top times: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                topTimes = new Dictionary<string, List<int>>();
            }

            UpdateTopTimesDisplay();
        }

        private void PuzzlePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Left)
            {
                int cornerSize = 16;
                if (e.X >= puzzlePanel.Width - cornerSize && e.Y >= puzzlePanel.Height - cornerSize)
                {
                    isResizing = true;
                    resizeStartPoint = Cursor.Position;
                    resizeStartPanelSize = puzzlePanel.Size;
                    puzzlePanel.Cursor = Cursors.SizeNWSE;
                }
            }
        }

        private void PuzzlePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                Point currentMousePosition = Cursor.Position;
                int deltaX = currentMousePosition.X - resizeStartPoint.X;
                int deltaY = currentMousePosition.Y - resizeStartPoint.Y;

                int newWidth = resizeStartPanelSize.Width + deltaX;
                int newHeight = resizeStartPanelSize.Height + deltaY;

                float aspectRatio = (float)resizeStartPanelSize.Width / resizeStartPanelSize.Height;
                if (newWidth / (float)newHeight > aspectRatio)
                {
                    newWidth = (int)(newHeight * aspectRatio);
                }
                else
                {
                    newHeight = (int)(newWidth / aspectRatio);
                }

                int minSize = 100;
                if (newWidth < minSize || newHeight < minSize)
                {
                    return;
                }

                puzzlePanel.Size = new Size(newWidth, newHeight);

                UpdateResizeHintLabelPosition();
                ResizeFormToContent();
            }
            else
            {
                int cornerSize = 16;
                if (e.X >= puzzlePanel.Width - cornerSize && e.Y >= puzzlePanel.Height - cornerSize)
                {
                    puzzlePanel.Cursor = Cursors.SizeNWSE;
                }
                else
                {
                    puzzlePanel.Cursor = Cursors.Default;
                }
            }
        }

        private void PuzzlePanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                isResizing = false;
                puzzlePanel.Cursor = Cursors.Default;

                if (uploadedImage != null)
                {
                    GeneratePuzzle();
                }

                UpdateResizeHintLabelPosition();
                ResizeFormToContent();
            }
        }
    }
}
