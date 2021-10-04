using System;
using System.Collections.Generic;
using Game;

namespace Minimax
{
    public class Analyzer
    {
        private Dictionary<sbyte[,], int> scored = new Dictionary<sbyte[,], int>();

        public Move Analyze(int depth, Board board)
        {
            Move bestMove = null;
            int alpha = int.MinValue, beta = int.MaxValue;

            if (board.Turn)
            {
                int maxScore = int.MinValue;
                foreach (Move move in board.LegalMoves())
                {
                    HashSet<Move> moves = board.Move(move);
                    int score = Analyze(depth - 1, int.MinValue, int.MaxValue, board, moves);
                    board.Unmove();
                    if (score >= maxScore)
                    {
                        maxScore = score;
                        bestMove = move;
                    }
                    // pruning
                    alpha = Math.Max(score, alpha);
                    if (beta <= alpha)
                        break;
                }
            }
            else
            {
                int minScore = int.MaxValue;
                foreach (Move move in board.LegalMoves())
                {
                    HashSet<Move> moves = board.Move(move);
                    int score = Analyze(depth - 1, int.MinValue, int.MaxValue, board, moves);
                    board.Unmove();
                    if (score <= minScore)
                    {
                        minScore = score;
                        bestMove = move;
                    }
                    // pruning
                    beta = Math.Min(score, beta);
                    if (beta <= alpha)
                        break;
                }
            }

            return bestMove;
        }

        private int Analyze(int depth, int alpha, int beta, Board board, HashSet<Move> moves)
        {
            if (board.Win(out bool winner)) //if (moves.Count == 0)
            {
                if (winner) //if (!Board.Turn)
                    return board.Size * board.Size; // = every space is your king (impossibly high but not +inf)
                else
                    return board.Size * board.Size * -1;
            }

            if (depth == 0)
            {
                int score;

                if (scored.ContainsKey(board.GetBoard()))
                {
                    score = scored[board.GetBoard()];
                }
                else
                {
                    score = 0;
                    for (int i = 0; i < board.Size; i++)
                    {
                        for (int j = 0; j < board.Size; j++)
                        {
                            score += board.SignedPieceAt(i, j);
                        }
                    }

                    scored.Add(board.GetBoard(), score);
                }

                System.Diagnostics.Debug.WriteLine(scored.Count);
                return score;
            }

            if (board.Turn)
            {
                int maxScore = int.MinValue;
                foreach (Move move in moves)
                {
                    HashSet<Move> newMoves = board.Move(move);
                    int score = Analyze(depth - 1, alpha, beta, board, newMoves);
                    board.Unmove();
                    maxScore = Math.Max(score, maxScore);
                    //pruning
                    alpha = Math.Max(score, alpha);
                    if (beta <= alpha)
                        break;
                }
                return maxScore;
            }
            else
            {
                int minScore = int.MaxValue;
                foreach (Move move in moves)
                {
                    HashSet<Move> newMoves = board.Move(move);
                    int score = Analyze(depth - 1, alpha, beta, board, newMoves);
                    board.Unmove();
                    minScore = Math.Min(score, minScore);
                    //pruning
                    beta = Math.Min(score, beta);
                    if (beta <= alpha)
                        break;
                }
                return minScore;
            }
        }
    }
}
