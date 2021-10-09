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

        public delegate void PieceMove(Move move, sbyte[,] board);
        public event PieceMove MovedPiece;

        public delegate void CreateBoard(sbyte[,] board);
        public event CreateBoard BoardCreated;

        public delegate void WinGame(Winner winner);
        public event WinGame GameWon;

        public delegate void StartTurn(PlayerType player, HashSet<Move> legalMoves);
        public event StartTurn TurnStarted;

        private Board board;
        private Dictionary<bool, PlayerType> players;
        private Analyzer AI;

        private int depth;

        private (int, int) selectedPiece;

        LinkedList<Thread> movers = new LinkedList<Thread>();
        bool running = true;

        bool moving = false;

        public GameController()
        {
            players = new Dictionary<bool, PlayerType>
            {
                { true, PlayerType.None },
                { false, PlayerType.None }
            };

            selectedPiece = (-1, -1);
        }

        public void SetAIDepth(int depth)
        {
            this.depth = depth;
        }

        public void StartGame(int size = 8, int rows = 3)
        {
            players[true] = PlayerType.AI;
            players[false] = PlayerType.AI;

            board = new Board(size, rows);
            BoardCreated(board.GetBoard());
            AI = new Analyzer(board);

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

                HashSet<Move> moves = board.LegalMoves(selectedPiece.Item1, selectedPiece.Item2);

                foreach (Move legal in moves)
                {
                    if (legal.Equals(move))
                    {
                        MovePiece(legal); //more developed, includes chains etc
                        selectedPiece = (-1, -1);
                        return;
                    }
                }

                if (selectedPiece == (x, y))
                {
                    selectedPiece = (-1, -1);
                    return;
                }
            }

            ErrorOnSelection((x, y));
        }

        private void TurnStart()
        {
            if (!running) return;

            Winner winner = board.Win();
            if (winner != Winner.None)
            {
                GameWon(winner);

                Thread.Sleep(1000);

                depth++;
                StartGame();
                return;
            }

            PlayerType next = players[board.Turn];

            switch (next)
            {
                case PlayerType.Local:
                    TurnStarted(next, board.LegalMoves());
                    break;
                case PlayerType.AI:
                    TurnStarted(next, null);
                    Thread moveThread = new Thread(AIMove);
                    movers.AddLast(moveThread);
                    moveThread.Start();
                    break;
            }
        }

        private void AIMove()
        {
            while (moving) ;

            moving = true;
            Move move;
            if (board.Turn)
                move = AI.Analyze(5);
            else
                move = AI.Analyze(5);
            MovePiece(move);
            Thread.Sleep(500);
            moving = false;
            movers.RemoveFirst(); //removes self from queue
        }

        public void MovePiece(Move move)
        {
            board.Move(move, false);

            MovedPiece(move, board.GetBoard());

            TurnStart();
        }

        /// <summary>
        /// Closes all active threads and prevents the creation of new threads.
        /// </summary>
        public void Quit()
        {
            running = false;
            if (movers.Count > 0)
                movers.Last.Value.Join();
        }
    }
}
