using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Reflection;
using System.IO.Compression;

namespace TicTacToeGUI
{
    public partial class GameForm : Form
    {
        private const int CellSize = 100;
        private const int BoardSize = 3;
        private readonly char[,] board = new char[BoardSize, BoardSize];
        private char currentPlayer;
        private bool gameEnded = false;
        private string winDirection;

        private SoundPlayer soundX;
        private SoundPlayer soundO;
        private SoundPlayer soundL;
        private SoundPlayer soundW;
        private SoundPlayer soundD;

        public GameForm()
        {
            InitializeComponent();
            this.Icon = TicTacToeGUI.Properties.Resources.tic_tac_toe_icon;
            NewGame();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGUIBoard();
            LoadSound();
        }

        private void LoadSound()
        {
            soundX = new SoundPlayer
            {
                Stream = Properties.Resources.X
            };
            soundO = new SoundPlayer
            {
                Stream = Properties.Resources.O
            };
            soundL = new SoundPlayer
            {
                Stream = Properties.Resources.L
            };
            soundW = new SoundPlayer
            {
                Stream = Properties.Resources.win
            };
            soundD = new SoundPlayer
            {
                Stream = Properties.Resources.draw
            };
        }

        private void NewGame()
        {
            currentPlayer = 'X';
            gameEnded = false;
            winDirection = null;
            InitializeBoard();
            button1.Text = "RESTART GAME";
        }

        private void newGame_Click(object sender, EventArgs e)
        {
            ClearGUIBoard();
            NewGame();
        }

        private void InitializeBoard()
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    board[row, col] = ' ';
                }
            }
        }

        private void InitializeGUIBoard()
        {
            int menuStripHeight = menuStrip1.Height;
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    PictureBox pictureBox = new PictureBox
                    {
                        Name = "pictureBox" + row + col,
                        Size = new Size(CellSize, CellSize),
                        Location = new Point(col * CellSize, row * CellSize + menuStripHeight),
                        BorderStyle = BorderStyle.FixedSingle,
                        Tag = new Point(row, col),
                        Cursor = Cursors.Hand
                    };
                    pictureBox.MouseClick += PictureBox_Click;
                    Controls.Add(pictureBox);
                }
            }
        }

        private void ClearGUIBoard()
        {
            foreach (Control control in Controls)
                if (control is PictureBox pictureBox)
                    pictureBox.Image = null;
            Refresh();
        }

        private void DrawCircle(Graphics g, Pen pen, int centerX, int centerY, int radius)
        {
            int angle = 0;
            while (angle <= 360)
            {
                g.DrawArc(pen, centerX - radius, centerY - radius, radius * 2, radius * 2, angle, 1);
                angle++;
            }
        }

        private void DrawLine(Graphics g, Pen pen, int startX, int startY, int endX, int endY)
        {
            float step = 0;
            while (step <= 1)
            {
                float x = startX + (endX - startX) * step;
                float y = startY + (endY - startY) * step;
                g.DrawLine(pen, startX, startY, x, y);
                step += 0.01f;
            }
        }

        private async Task DrawSymbol(PictureBox pictureBox, char symbol)
        {
            Graphics g = pictureBox.CreateGraphics();
            Pen pen = new Pen(Color.Black, 2);
            if (symbol == 'X')
            {
                await Task.Run(() => soundX.Play());
                DrawLine(g, pen, 10, 10, pictureBox.Width - 10, pictureBox.Height - 10);
                Thread.Sleep(250);
                DrawLine(g, pen, pictureBox.Width - 10, 10, 10, pictureBox.Height - 10);
            }
            else if (symbol == 'O')
            {
                int centerX = pictureBox.Width / 2;
                int centerY = pictureBox.Height / 2;
                int diameter = Math.Min(pictureBox.Width, pictureBox.Height) - 20;
                int radius = diameter / 2;
                await Task.Run(() => soundO.Play());
                Thread.Sleep(250);
                DrawCircle(g, pen, centerX, centerY, radius);
            }
            g.Dispose();
        }

        private Point[] GetWinningLine(int row, int col)
        {
            char symbol = board[row, col];

            if (board[row, 0] == symbol && board[row, 1] == symbol && board[row, 2] == symbol)
            {
                winDirection = "row";
                return new Point[] { new Point(row, 0), new Point(row, 2) };
            }
            if (board[0, col] == symbol && board[1, col] == symbol && board[2, col] == symbol)
            {
                winDirection = "col";
                return new Point[] { new Point(0, col), new Point(2, col) };
            }
            if ((board[0, 0] == symbol && board[1, 1] == symbol && board[2, 2] == symbol))
            {
                winDirection = "diag1";
                return new Point[] { new Point(0, 0), new Point(2, 2) };
            }
            if ((board[0, 2] == symbol && board[1, 1] == symbol && board[2, 0] == symbol))
            {
                winDirection = "diag2";
                return new Point[] { new Point(0, 2), new Point(2, 0) };
            }

            return null;
        }

        private bool IsBoardFull()
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (board[row, col] == ' ')
                        return false;
                }
            }
            return true;
        }

        private async void PictureBox_Click(object sender, EventArgs e)
        {
            if (gameEnded)
                return;

            PictureBox pictureBox = (PictureBox)sender;
            Point location = (Point)pictureBox.Tag;
            int row = location.X;
            int col = location.Y;

            if (board[row, col] == ' ')
            {
                await DrawSymbol(pictureBox, currentPlayer);
                board[row, col] = currentPlayer;

                Point[] winningLine = GetWinningLine(row, col);
                if (winningLine != null)
                {
                    await DrawWinningLine(winningLine[0], winningLine[1]);
                    await Task.Run(() => soundW.Play());
                    Thread.Sleep(200);
                    MessageBox.Show(currentPlayer + " wins!", "Game Over", MessageBoxButtons.OK);
                    gameEnded = true;
                    button1.Text = "START A NEW GAME";
                }
                else if (IsBoardFull())
                {
                    await Task.Run(() => soundD.Play());
                    Thread.Sleep(200);
                    MessageBox.Show("Draw!", "Game Over", MessageBoxButtons.OK);
                    gameEnded = true;
                    button1.Text = "START A NEW GAME";
                }
                else
                {
                    currentPlayer = (currentPlayer == 'X') ? 'O' : 'X';
                }
            }
        }

        private void DrawLine(PictureBox pictureBox, Point startPoint, Point endPoint)
        {
            Pen pen = new Pen(Color.Red, 5);
            Graphics g = pictureBox.CreateGraphics();
            g.DrawLine(pen, startPoint, endPoint);
            g.Dispose();
        }

        private async Task DrawWinningLine(Point startPoint, Point endPoint)
        {
            Thread.Sleep(500);
            PictureBox firstPictureBox = (PictureBox)Controls.Find("pictureBox" + startPoint.X + startPoint.Y, true)[0];
            PictureBox middlePictureBox = (PictureBox)Controls.Find("pictureBox" + ((startPoint.X + endPoint.X) / 2) + ((startPoint.Y + endPoint.Y) / 2), true)[0];
            PictureBox lastPictureBox = (PictureBox)Controls.Find("pictureBox" + endPoint.X + endPoint.Y, true)[0];

            Point startPointFirst = Point.Empty;
            Point endPointFirst = Point.Empty;
            Point startPointMiddle = Point.Empty;
            Point endPointMiddle = Point.Empty;
            Point startPointLast = Point.Empty;
            Point endPointLast = Point.Empty;

            if (winDirection == "row")
            {
                startPointFirst = new Point(CellSize / 4, CellSize / 2);
                endPointFirst = new Point(CellSize, CellSize / 2);
                startPointMiddle = new Point(0, CellSize / 2);
                endPointMiddle = new Point(CellSize, CellSize / 2);
                startPointLast = new Point(0, CellSize / 2);
                endPointLast = new Point(CellSize / 2 + CellSize / 4, CellSize / 2);
            }
            else if (winDirection == "col")
            {
                startPointFirst = new Point(CellSize / 2, CellSize / 4);
                endPointFirst = new Point(CellSize / 2, CellSize);
                startPointMiddle = new Point(CellSize / 2, 0);
                endPointMiddle = new Point(CellSize/2, CellSize);
                startPointLast = new Point(CellSize / 2,0);
                endPointLast = new Point(CellSize / 2, CellSize / 2 + CellSize / 4);
            }
            else if (winDirection == "diag1")
            {
                startPointFirst = new Point(CellSize / 4, CellSize / 4);
                endPointFirst = new Point(CellSize, CellSize);
                startPointMiddle = new Point(0, 0);
                endPointMiddle = new Point(CellSize, CellSize);
                startPointLast = new Point(0, 0);
                endPointLast = new Point(CellSize / 2 + CellSize / 4, CellSize / 2 + CellSize / 4);
            }
            else if (winDirection == "diag2")
            {
                startPointFirst = new Point(CellSize / 2 + CellSize / 4, CellSize / 4);
                endPointFirst = new Point(0, CellSize);
                startPointMiddle = new Point(CellSize, 0);
                endPointMiddle = new Point(0, CellSize);
                startPointLast = new Point(CellSize, 0);
                endPointLast = new Point(CellSize / 4, CellSize / 2 + CellSize / 4);
            }

            await Task.Run(() => soundL.Play());
            DrawLine(firstPictureBox, startPointFirst, endPointFirst);
            Thread.Sleep(150);
            DrawLine(middlePictureBox, startPointMiddle, endPointMiddle);
            Thread.Sleep(150);
            DrawLine(lastPictureBox, startPointLast, endPointLast);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutForm = new AboutBox();
            aboutForm.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
