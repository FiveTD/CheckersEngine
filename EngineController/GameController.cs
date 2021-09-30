using System;
using Game;
using Minimax;
using System.Collections.Generic;
using System.Threading;

namespace EngineController
{
    public enum PlayerType
    {
        Local,
        AI,
        None
    }

    public class GameController
    {
        public delegate void SelectPiece(HashSet<Move> legalMoves);
        public event SelectPiece PieceSelected;

        public delegate void ErrorSelect((int, int) selection);
        public event ErrorSelect ErrorOnSelection;

        public delegate void PieceMove(Move move, Board board);
        public event PieceMove MovedPiece;

        public delegate void CreateBoard(Board board);
        public event CreateBoard BoardCreated;

        public delegate void WinGame(bool winner);
        public event WinGame GameWon;

        public delegate void StartTurn(PlayerType player, HashSet<Move> legalMoves);
        public event StartTurn TurnStarted;

        public delegate void DebugMessage(string text);
        public event DebugMessage Debug;

        private Board board;

        private Dictionary<bool, PlayerType> players;
        private Analyzer AI;

        private int depth;

        private (int, int) selectedPiece;

        private bool gameWon;

        Thread renderThread;

        public GameController()
        {
            players = new Dictionary<bool, PlayerType>();
            players.Add(true, PlayerType.None);
            players.Add(false, PlayerType.None);

            selectedPiece = (-1, -1);

            AI = new Analyzer();
        }

        public void SetAIDepth(int depth)
        {
            this.depth = depth;
        }

        public void StartGame(PlayerType p1, PlayerType p2, int size = 8, int rows = 3)
        {
            players[true] = p1;
            players[false] = p2;

            board = new Board(size, rows);
            BoardCreated(board);

            gameWon = false;

            TurnStart();
        }

        public void SelectSpace(int x, int y)
        {
            if (board.PieceAt(x, y) != 0 && board.PlayerAt(x, y) == board.Turn)
            {
                HashSet<Move> moves = board.LegalMoves(x, y);

                if (moves.Count > 0)
                {
                    selectedPiece = (x, y);
                    PieceSelected(moves);
                    return;
                }
            }

            else if (selectedPiece != (-1, -1))
            {
                Move move = new Move
                {
                    FromX = selectedPiece.Item1,
                    FromY = selectedPiece.Item2,
                    ToX = x,
                    ToY = y
                };

                if (board.IsLegalMove(move))
                {
                    MovePiece(move);
                    selectedPiece = (-1, -1);
                    return;
                }

                else if (selectedPiece == (x, y))
                {
                    selectedPiece = (-1, -1);
                    return;
                }
            }

            ErrorOnSelection((x, y));
        }

        private void TurnStart()
        {
            //System.Diagnostics.Debug.WriteLine("ts start " + DateTime.Now);
            if (board.Win(out bool winner))
            {
                gameWon = true;
                GameWon(winner);
            }

            PlayerType next = players[board.Turn];

            switch (next)
            {
                case PlayerType.Local:
                    Render("TurnStarted", next, board.LegalMoves());
                    break;
                case PlayerType.AI:
                    TurnStarted(next, null);
                    Move move = AI.Analyze(depth, board);
                    MovePiece(move);
                    break;
            }
            //System.Diagnostics.Debug.WriteLine("ts finish " + DateTime.Now);
        }

        public void MovePiece(Move move)
        {
            //System.Diagnostics.Debug.WriteLine("mp start " + DateTime.Now);
            if (gameWon)
                return;

            board.Move(move, false); //flags if jump

            //System.Diagnostics.Debug.WriteLine("pre thread " + DateTime.Now);

            Render("MovedPiece", move, board);

            TurnStart();
        }

        /// <summary>
        /// Starts the render thread.
        /// </summary>
        private void Render(string eventName, params object[] args)
        {
            ThreadArgs a = new ThreadArgs
            {
                Event = eventName,
                Args = args
            };

            if (!(renderThread is null))
                renderThread.Join();

            renderThread = new Thread(Redraw);
            renderThread.Start(a);
        }

        /// <summary>
        /// Allows render events to be called on a separate thread.
        /// </summary>
        private void Redraw(object a)
        {
            ThreadArgs args = a as ThreadArgs;
            switch (args.Event)
            {
                case "MovedPiece":
                    MovedPiece(args.Args[0] as Move, args.Args[1] as Board);
                    break;
                case "TurnStarted":
                    TurnStarted((PlayerType)args.Args[0], args.Args[1] as HashSet<Move>);
                    break;
            }
        }

        private class ThreadArgs
        {
            public string Event { get; set; }
            public object[] Args { get; set; }
        }
    }
}
