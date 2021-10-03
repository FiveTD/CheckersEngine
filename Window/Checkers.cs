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
        Thread renderThread;

        public Checkers(GameController c)
        {
            InitializeComponent();

            checkerboard = new CheckerBoard(null);
            checkerboard.Location = new Point(0, 0);
            checkerboard.Size = new Size(800, 800);
            checkerboard.SpaceClicked += OnSpaceClicked;
            Controls.Add(checkerboard);

            controller = c;
            controller.BoardCreated += BoardCreatedListener;
            controller.TurnStarted += TurnStartedListener;
            controller.MovedPiece += MovedPieceListener;
            controller.PieceSelected += PieceSelectedListener;
            controller.ErrorOnSelection += ErrorOnSelectionListener;
            controller.GameWon += GameWonListener;

            debug = new Label();
            debug.Location = new Point(0, 800);
            debug.Size = new Size(800, 50);
            debug.Text = "Debug";
            Controls.Add(debug);
            controller.Debug += LogMessage;

            animationUpdate = new Thread(Animate);
        }

        #region Listeners
        private void BoardCreatedListener(Board board)
        {
            Render(OnBoardCreated, board);
        }

        private void TurnStartedListener(PlayerType player, HashSet<Move> legalMoves)
        {
            Render(OnTurnStart, player, legalMoves);
        }

        private void MovedPieceListener(Move move, Board board)
        {
            Render(OnPieceMoved, move, board);
        }

        private void PieceSelectedListener(HashSet<Move> legalMoves)
        {
            Render(OnPieceSelected, legalMoves);
        }

        private void ErrorOnSelectionListener((int, int) selection)
        {
            Render(OnSelectionError, selection);
        }

        private void GameWonListener(bool winner)
        {
            Render(OnGameWin, winner);
        }
        #endregion


        private void OnBoardCreated(object[] p)
        {
            Board b = p[0] as Board;

            checkerboard.SetBoard(b.GetBoard());
        }

        private void OnTurnStart(object[] p)
        {
            PlayerType player = (PlayerType)p[0];
            HashSet<Move> moves = p[1] as HashSet<Move>;

            if (player == PlayerType.Local)
            {
                foreach (Move m in moves)
                {
                    checkerboard.HighlightPiece(m.FromX, m.FromY, Color.Yellow);
                }
            }
            else if (player == PlayerType.AI)
            {
                // uhhh
            }
        }

        private void OnSpaceClicked(int x, int y)
        {
            controller.SelectSpace(x, y);
        }

        private void OnPieceSelected(object[] p)
        {
            HashSet<Move> legalMoves = p[0] as HashSet<Move>;

            foreach (Move move in legalMoves)
            {
                checkerboard.HighlightSpace(move.ToX, move.ToY, Color.Yellow);
            }
        }

        private void OnSelectionError(object[] p)
        {
            (int, int) errorSpace = ((int, int))p[0];

            // animations :DD
        }

        private void OnPieceMoved(object[] p)
        {
            Move move = p[0] as Move;
            Board b = p[1] as Board;

            checkerboard.ClearAnimations();
            checkerboard.SetBoard(b.GetBoard());

            checkerboard.HighlightPiece(move.ToX, move.ToY, Color.Cyan);
            checkerboard.HighlightSpace(move.FromX, move.FromY, Color.Cyan);
        }

        private void OnGameWin(object[] p)
        {
            bool winner = (bool)p[0];

            Thread.Sleep(1000);
            checkerboard.ClearAnimations();
            controller.StartGame();
        }

        private void LogMessage(string text)
        {
            debug.Text = text;
        }

        private void Animate()
        {
            while (animating)
            {
                Thread.Sleep(17); // 60fps
                checkerboard.Invalidate();
            }
        }

        /// <summary>
        /// Starts the render thread.
        /// </summary>
        private void Render(Action<object[]> renderer, params object[] args)
        {
            if (!(renderThread is null))
                renderThread.Join();

            renderThread = new Thread(() => { renderer(args); });
            renderThread.Start();
        }

        private void Checkers_Shown(object sender, EventArgs e)
        {
            controller.StartGame();
            animationUpdate.Start();
        }

        private void Checkers_FormClosing(object sender, FormClosingEventArgs e)
        {
            animating = false;
        }
    }
}
