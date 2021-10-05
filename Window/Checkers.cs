using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using EngineController;
using Game;
using System.Diagnostics;

namespace GameView
{
    public partial class Checkers : Form
    {
        CheckerBoard checkerboard;

        GameController controller;

        Thread animationUpdate;
        bool animating = true;
        Thread renderThread;

        readonly Color legalHL = Color.Goldenrod;
        readonly Color histHL = Color.White;

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

            animationUpdate = new Thread(Animate);
        }

        #region Listeners
        private void BoardCreatedListener(sbyte[,] board)
        {
            Render(OnBoardCreated, board);
        }

        private void TurnStartedListener(PlayerType player, HashSet<Move> legalMoves)
        {
            Render(OnTurnStart, player, legalMoves);
        }

        private void MovedPieceListener(Move move, sbyte[,] board)
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

        private void GameWonListener(Winner winner)
        {
            Render(OnGameWin, winner);
        }
        #endregion

        private void OnBoardCreated(object[] p)
        {
            sbyte[,] b = p[0] as sbyte[,];

            checkerboard.ClearAnimations();
            checkerboard.SetBoard(b);
        }

        private void OnTurnStart(object[] p)
        {
            PlayerType player = (PlayerType)p[0];
            HashSet<Move> moves = p[1] as HashSet<Move>;

            if (player == PlayerType.Local)
            {
                foreach (Move m in moves)
                {
                    checkerboard.HighlightPiece(m.FromX, m.FromY, legalHL);
                }
            }
            else if (player == PlayerType.AI)
            {
                
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
                checkerboard.HighlightSpace(move.ToX, move.ToY, legalHL);
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
            sbyte[,] b = p[1] as sbyte[,];

            checkerboard.ClearAnimations();
            checkerboard.SetBoard(b);

            checkerboard.HighlightPiece(move.ToX, move.ToY, histHL);
            checkerboard.HighlightSpace(move.FromX, move.FromY, histHL);
        }

        private void OnGameWin(object[] p)
        {
            Winner winner = (Winner)p[0];

            // animations??
        }

        private void Animate()
        {
            while (animating)
            {
                checkerboard.Invalidate();
                Thread.Sleep(17); // 60fps
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
            controller.Quit(); //forces wait until all threads closed
        }
    }
}
