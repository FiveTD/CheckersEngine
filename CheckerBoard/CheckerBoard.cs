using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GameView
{
    public class CheckerBoard : Panel
    {
        const float OFFSET = .8f;

        [Category("Appearance"),
        Description("Specifies the number of tiles per side of the board.")]
        public int BoardSize { set; get; }

        [Category("Appearance"),
        Description("Specifies the color of odd-numbered spaces.")]
        public Color OddColor { set; get; }
        [Category("Appearance"),
        Description("Specifies the color of even-numbered spaces.")]
        public Color EvenColor { set; get; }

        public delegate void ClickSpace(int x, int y);
        public event ClickSpace SpaceClicked;

        private HashSet<Animation> animations = new HashSet<Animation>();
        private HashSet<(int, int)> highlighted = new HashSet<(int, int)>();

        sbyte[,] board;
        Size squareSize;
        
        public CheckerBoard(sbyte[,] b)
        {
            BoardSize = 8;
            squareSize = Size / BoardSize;

            OddColor = Color.Black;
            EvenColor = Color.Red;

            DoubleBuffered = true;

            board = b;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            squareSize = Size / BoardSize;
        }

        public void SetBoard(sbyte[,] b)
        {
            board = b;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using Brush odd = new SolidBrush(OddColor);
            using Brush even = new SolidBrush(EvenColor);

            // Board
            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    if (y % 2 == x % 2)
                    {
                        e.Graphics.FillRectangle(odd, squareSize.Width * x, squareSize.Height * y, squareSize.Width, squareSize.Height);
                    }
                    else
                    {
                        e.Graphics.FillRectangle(even, squareSize.Width * x, squareSize.Height * y, squareSize.Width, squareSize.Height);
                    }
                }
            }

            // Pieces
            if (!(board is null))
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    for (int x = 0; x < BoardSize; x++)
                    {
                        sbyte piece = board[y, x];
                        if (piece != 0)
                            DrawPiece(e.Graphics, new Point(squareSize.Width * x, squareSize.Height * y), squareSize, piece);
                    }
                }
            }

            // Animations
            HashSet<Animation> finished = new HashSet<Animation>();

            lock (animations)
            {
                foreach (Animation anim in animations)
                {
                    anim.DrawNext(e.Graphics);
                    if (anim.Finished)
                    {
                        finished.Add(anim);
                    }
                }

                foreach (Animation anim in finished)
                {
                    animations.Remove(anim);
                }
            }
        }

        private void DrawPiece(Graphics g, Point location, Size size, sbyte piece)
        {
            Brush b;
            Color c;
            if (piece > 0)
                c = Color.DarkRed;
            else
                c = Color.FromArgb(100, 100, 100); // dark gray

            b = new SolidBrush(c);

            Size offsetSize = new Size((int)(size.Width * OFFSET), (int)(size.Height * OFFSET));
            Point offsetLoc = location + ((size - offsetSize) / 2);

            g.FillEllipse(b, new Rectangle(offsetLoc, offsetSize));

            if (Math.Abs(piece) == 2)
            {
                offsetSize /= 2;
                offsetLoc = location + ((size - offsetSize) / 2);
                c = ControlPaint.Dark(c);
                b = new SolidBrush(Color.Black);

                g.FillEllipse(b, new Rectangle(offsetLoc, offsetSize));
            }
        }

        public void HighlightPiece(int x, int y, Color highlight)
        {
            if (!highlighted.Add((x, y))) // already highlighted
                return;

            Point loc = new Point(y * squareSize.Height, x * squareSize.Width);

            lock (animations)
                animations.Add(new PieceAnimation(loc, highlight));
        }

        public void ClearAnimations()
        {
            lock (animations)
                animations.Clear();
            highlighted.Clear();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            SpaceClicked(e.Y / squareSize.Height, e.X / squareSize.Width);
        }
    }
}