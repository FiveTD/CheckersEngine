using System;
using System.Collections.Generic;

namespace Game
{
    public class Board
    {
        private sbyte[,] board;
        public int Size { get; private set; }

        public bool Turn { get; private set; }

        public bool JumpRequired { get; private set; }

        const sbyte PAWN = 1;
        const sbyte KING = 2;

        private LinkedList<Move> moveHistory = new LinkedList<Move>();
        private (int, int) justMoved = (-1, -1);

        /// <summary>
        /// Creates a new checkers board.
        /// </summary>
        /// <param name="size">The length of one side of the board.</param>
        /// <param name="rows">The number of rows of checkers to start.</param>
        public Board(int size = 8, int rows = 3)
        {
            Size = size;
            board = new sbyte[size, size];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < size; j += 2)
                {
                    if (i % 2 == 1 && j == 0) j++; //odd row offset

                    board[i, j] = PAWN;
                    board[size - i - 1, size - j - 1] = -PAWN;
                }
            }

            Turn = true;
        }

        /// <summary>
        /// Returns the piece type at the given coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public sbyte PieceAt(int x, int y)
        {
            return Math.Abs(SignedPieceAt(x, y));
        }

        /// <summary>
        /// Returns the piece at the given coordinates, with the team sign.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public sbyte SignedPieceAt(int x, int y)
        {
            try
            {
                return board[x, y];
            }
            catch (IndexOutOfRangeException)
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns if the owner of the piece at the given coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool PlayerAt(int x, int y)
        {
            return SignedPieceAt(x, y) > 0;
        }

        /// <summary>
        /// Returns if moving the piece at the first coordinates to the second coordinates is legal.
        /// </summary>
        /// <param name="move.FromX"></param>
        /// <param name="move.FromY"></param>
        /// <param name="move.ToX"></param>
        /// <param name="move.ToY"></param>
        /// <returns></returns>
        public bool IsLegalMove(Move move)
        {
            if ((move.FromX < 0 || move.FromX >= Size) ||
                (move.FromY < 0 || move.FromY >= Size) ||
                (move.ToX < 0 || move.ToX >= Size) ||
                (move.ToY < 0 || move.ToY >= Size))
                return false;

            sbyte piece = PieceAt(move.FromX, move.FromY);
            bool player = PlayerAt(move.FromX, move.FromY);

            if (piece == 0)
                return false;
            if (player != Turn)
                return false;

            int xdist = Distance(move.FromX, move.ToX), ydist = Distance(move.FromY, move.ToY);
            if (xdist > 2 || ydist > 2)
                return false;
            if (xdist != ydist || xdist == 0) // non-diagonal or same spot
                return false;

            if (PieceAt(move.ToX, move.ToY) == 0) // spot empty
            {
                if (JumpRequired && !IsJump(move)) // Can only check if legal moves generated
                    return false;

                if (piece == PAWN)
                {
                    if ((player == true && move.ToX < move.FromX) ||
                        (player == false && move.ToX > move.FromX))
                    {
                        return false;
                    }
                }

                if (xdist == 1) return true;

                else // jump
                {
                    Midpoint(move, out int jumpx, out int jumpy);
                    if (PieceAt(jumpx, jumpy) != 0 && PlayerAt(jumpx, jumpy) == !player)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns all legal moves on the board.
        /// </summary>
        /// <returns></returns>
        /// 
        public HashSet<Move> LegalMoves()
        {
            if (justMoved != (-1, -1) && Turn == PlayerAt(justMoved.Item1, justMoved.Item2))
                return LegalMoves(justMoved.Item1, justMoved.Item2);

            HashSet<Move> moves = new HashSet<Move>();
            JumpRequired = false;
            bool jumped = false;

            for (int i = 0; i < Size; i ++)
            {
                for (int j = 0; j < Size; j += 2)
                {
                    if (i % 2 != j % 2) j++; //black offset
                    HashSet<Move> newMoves = LegalMoves(i, j);
                    if (JumpRequired && !jumped)
                    {
                        moves.Clear();
                        jumped = true;
                    }

                    moves.UnionWith(newMoves);
                }
            }

            return moves;
        }

        /// <summary>
        /// Returns all legal moves for the specified piece on the board.
        /// Honors JumpRequired.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public HashSet<Move> LegalMoves(int x, int y)
        {
            HashSet<Move> moves = new HashSet<Move>();

            for (int i = -1; i <= 1; i += 2)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    Move move = new Move
                    {
                        FromX = x,
                        FromY = y,
                        ToX = x + i,
                        ToY = y + j
                    };

                    if (!JumpRequired && IsLegalMove(move))
                    {
                        moves.Add(move);
                    }
                    else //jump?
                    {
                        move.ToX += i;
                        move.ToY += j;
                        if (IsLegalMove(move)) // is jump
                        {
                            if (!JumpRequired)
                            {
                                moves.Clear();
                                JumpRequired = true;
                            }

                            Move(move, false, false);
                            move.chains = LegalMoves(move.ToX, move.ToY); // will be jumps available at next destination
                            Unmove();

                            moves.Add(move);
                        }
                    }
                }
            }

            return moves;
        }

        /// <summary>
        /// Calculates if the performed move is a jump.
        /// Returns the coordinates of the jumped piece.
        /// Assumes legality.
        /// </summary>
        /// <param name="move.FromX"></param>
        /// <param name="move.FromY"></param>
        /// <param name="move.ToX"></param>
        /// <param name="move.ToY"></param>
        /// <param name="jumpx">X coordinate of the jumped piece. 0 if no jump occurs.</param>
        /// <param name="jumpy">Y coordinate of the jumped piece. 0 if no jump occurs.</param>
        private bool IsJump(Move move, out int jumpx, out int jumpy)
        {
            int dist = Distance(move.FromX, move.ToX);
            if (dist == 2)
            {
                Midpoint(move, out jumpx, out jumpy);
                return true;
            }
            else
            {
                jumpx = 0;
                jumpy = 0;
                return false;
            }
        }

        /// <summary>
        /// Calculates if the performed move is a jump.
        /// Assumes legality.
        /// </summary>
        /// <param name="move.FromX"></param>
        /// <param name="move.FromY"></param>
        /// <param name="move.ToX"></param>
        /// <param name="move.ToY"></param>
        private bool IsJump(Move move)
        {
            return IsJump(move, out int _, out int _);
        }

        /// <summary>
        /// Moves a piece from the first coordinates to the second coordinates.
        /// Assumes legality.
        /// </summary>
        /// <returns>The legal moves after the performed move.</returns>
        public HashSet<Move> Move(Move move, bool calcNext = true, bool switchTurn = true)
        {
            sbyte piece = SignedPieceAt(move.FromX, move.FromY);
            bool turn = PlayerAt(move.FromX, move.FromY);
            sbyte jumped = 0;

            bool isJump = IsJump(move, out int jumpx, out int jumpy);
            
            if (isJump)
            {
                jumped = SignedPieceAt(jumpx, jumpy);
                board[jumpx, jumpy] = 0;
            }

            board[move.ToX, move.ToY] = board[move.FromX, move.FromY];
            board[move.FromX, move.FromY] = 0;

            // Promotion
            if (move.ToX == 0 && turn == false)
            {
                board[move.ToX, move.ToY] = -KING;
            }
            else if (move.ToX == Size - 1 && turn == true)
            {
                board[move.ToX, move.ToY] = KING;
            }

            move.Piece = piece;
            move.Jumped = jumped;
            move.Turn = turn;
            moveHistory.AddLast(move);

            if (move.chains.Count > 0)
            {
                return move.chains;
            }

            if (switchTurn) Turn = !Turn;
            if (calcNext)
                return LegalMoves();
            else
                return null;
        }

        /// <summary>
        /// Reverses the last move.
        /// </summary>
        public void Unmove()
        {
            Move move = moveHistory.Last.Value;

            board[move.FromX, move.FromY] = move.Piece;
            board[move.ToX, move.ToY] = 0;
            if (move.Jumped != 0)
            {
                Midpoint(move, out int jumpx, out int jumpy);
                board[jumpx, jumpy] = move.Jumped;
            }

            Turn = move.Turn;
            moveHistory.RemoveLast();
        }

        /// <summary>
        /// Returns if the game has been won.
        /// </summary>
        /// <param name="winner">The winning player.</param>
        /// <returns></returns>
        public bool Win(out bool winner)
        {
            winner = !Turn;
            return LegalMoves().Count == 0;
        }

        public sbyte[,] GetBoard()
        {
            return board.Clone() as sbyte[,];
        }

        /// <summary>
        /// Calculates the midpoint coordinate between two coordinate pairs.
        /// </summary>
        /// <param name="move.FromX"></param>
        /// <param name="move.move.FromY"></param>
        /// <param name="move.ToX"></param>
        /// <param name="move.ToY"></param>
        /// <param name="midx"></param>
        /// <param name="midy"></param>
        private void Midpoint(Move move, out int midx, out int midy)
        {
            midx = move.FromX + ((move.ToX - move.FromX) / 2);
            midy = move.FromY + ((move.ToY - move.FromY) / 2);
        }

        /// <summary>
        /// Calculates the absolute difference between two values.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int Distance(int a, int b)
        {
            return Math.Abs(a - b);
        }
    }

    public class Move
    {
        public int FromX { get; set; }
        public int FromY { get; set; }
        public int ToX { get; set; }
        public int ToY { get; set; }
        public sbyte Piece { get; set; }
        public sbyte Jumped { get; set; }
        public bool Turn { get; set; }
        public HashSet<Move> chains { get; set; }

        public Move()
        {
            chains = new HashSet<Move>();
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;

            Move o = obj as Move;

            return FromX == o.FromX && FromY == o.FromY &&
                ToX == o.ToX && ToY == o.ToY;
        }

        public override int GetHashCode()
        {
            return FromX + FromY * 10 + ToX * 100 + ToY * 1000;
        }
    }
}
