using System;
using System.Drawing;
using System.Threading;

namespace GameView
{
    public abstract class Animation
    {
        protected int length;

        protected int currentFrame;

        protected Point location;

        protected Thread anim;

        public virtual bool Finished
        {
            get
            {
                return currentFrame >= length;
            }
        }

        public Animation(int length, Point location)
        {
            this.length = length;
            this.location = location;
        }

        public virtual void DrawNext(Graphics g)
        {
            DrawFrame(g);

            currentFrame++;
        }

        protected abstract void DrawFrame(Graphics g);
    }

    public abstract class LoopAnimation : Animation
    {
        // Start/exit for loop
        protected int start;
        protected int exit;

        public bool Loop { set; get; }

        public override bool Finished
        {
            get
            {
                return base.Finished && Loop == false;
            }
        }

        public LoopAnimation(int length, int loopStart, int loopExit, Point location) :
            base (length, location)
        {
            start = loopStart;
            exit = loopExit;

            Loop = true;
        }

        public override void DrawNext(Graphics g)
        {
            DrawFrame(g);

            if (currentFrame == exit && Loop)
                currentFrame = start;
            else
                currentFrame++;
        }
    }
}
