using System;
using System.Drawing;

namespace GameView
{
    public class PieceAnimation : LoopAnimation
    {
        Color highlightColor;

        public PieceAnimation(Point location, Color highlight) :
            base(120, 0, 119, location)
        {
            highlightColor = highlight;
        }

        protected override void DrawFrame(Graphics g)
        {
            Color adjColor;
            if (currentFrame <= 60)
            {
                adjColor = Color.FromArgb(currentFrame * 2, highlightColor);
            }
            else
            {
                adjColor = Color.FromArgb((120 - currentFrame) * 2, highlightColor);
            }

            Brush b = new SolidBrush(adjColor);

            Size offsetSize = new Size(80, 80);
            Point offsetLoc = location + new Size(10, 10);

            g.FillEllipse(b, new Rectangle(offsetLoc, offsetSize));
        }
    }
}
