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

        public void StartGame(int size = 8, int rows = 3)
        {
            players[true] = PlayerType.AI;
            players[false] = PlayerType.Local;

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
            if (board.Win(out bool winner))
            {
                gameWon = true;
                GameWon(winner);
            }

            PlayerType next = players[board.Turn];

            switch (next)
            {
                case PlayerType.Local:
                    TurnStarted(next, board.LegalMoves());
                    break;
                case PlayerType.AI:
                    TurnStarted(next, null);
                    Move move = AI.Analyze(depth, board);
                    MovePiece(move);
                    break;
            }
        }

        public void MovePiece(Move move)
        {
            if (gameWon)
                return;

            board.Move(move, false); //flags if jump

            MovedPiece(move, board);

            TurnStart();
        }
    }
}
