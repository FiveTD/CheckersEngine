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
using EngineController;
using Game;

namespace GameView
{
    public partial class Checkers : Form
    {
        CheckerBoard checkerboard;
        Label debug;

        GameController controller;

        Thread animationUpdate;
        bool animating = true;

        public Checkers(GameController c)
        {
            InitializeComponent();

            checkerboard = new CheckerBoard(null);
            checkerboard.Location = new Point(0, 0);
            checkerboard.Size = new Size(800, 800);
            checkerboard.SpaceClicked += OnSpaceClicked;
            Controls.Add(checkerboard);

            controller = c;
            controller.BoardCreated += OnBoardCreated;
            controller.TurnStarted += OnTurnStart;
            controller.MovedPiece += OnPieceMoved;
            controller.PieceSelected += OnPieceSelected;
            controller.ErrorOnSelection += OnSelectionError;
            controller.GameWon += OnGameWin;

            debug = new Label();
            debug.Location = new Point(0, 800);
            debug.Size = new Size(800, 50);
            debug.Text = "Debug";
            Controls.Add(debug);
            controller.Debug += LogMessage;

            animationUpdate = new Thread(Animate);
        }

        private void OnBoardCreated(Board b)
        {
            checkerboard.SetBoard(b.GetBoard());
        }

        private void OnTurnStart(PlayerType p, HashSet<Move> moves)
        {
            if (p == PlayerType.Local)
            {
                foreach (Move m in moves)
                {
                    checkerboard.HighlightPiece(m.FromX, m.FromY, Color.Yellow);
                }
            }
            else if (p == PlayerType.AI)
            {
                // uhhh
            }
        }

        private void OnSpaceClicked(int x, int y)
        {
            controller.SelectSpace(x, y);
        }

        private void OnPieceSelected(HashSet<Move> legalMoves)
        {
            // animations :DD
        }

        private void OnSelectionError((int, int) errorSpace)
        {
            // animations :DD
        }

        private void OnPieceMoved(Move move, Board b)
        {
            System.Diagnostics.Debug.WriteLine("opm called " + DateTime.Now);
            // animations :DD

            checkerboard.ClearAnimations();
            checkerboard.SetBoard(b.GetBoard());

            checkerboard.HighlightPiece(move.ToX, move.ToY, Color.Cyan);
            System.Diagnostics.Debug.WriteLine("opm finished " + DateTime.Now);
        }

        private void OnGameWin(bool winner)
        {

        }

        private void LogMessage(string text)
        {
            debug.Text = text;
        }

        private void Checkers_Shown(object sender, EventArgs e)
        {
            controller.StartGame(PlayerType.Local, PlayerType.AI);
            animationUpdate.Start();
        }

        private void Animate()
        {
            while (animating)
            {
                //System.Diagnostics.Debug.WriteLine("animate tick");
                Thread.Sleep(17); // 60fps
                checkerboard.Invalidate();
            }
        }

        private void Checkers_FormClosing(object sender, FormClosingEventArgs e)
        {
            animating = false;
        }
    }
}
