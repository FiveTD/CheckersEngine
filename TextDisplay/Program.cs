using System;
using System.Collections.Generic;
using Game;

namespace TextDisplay
{
    class Program
    {
        const sbyte PAWN = 1;
        const sbyte KING = 2;

        static void Main(string[] args)
        {
            Board board = new Board();

            bool winner = false;
            while (!board.Win(out winner))
            {
                PrintBoard(board);
                if (board.Turn)
                {
                    string input = Console.ReadLine();
                    string[] bits = input.Split(' ');
                    int[] numBits = new int[4];
                    for (int i = 0; i < 4; i++)
                    {
                        numBits[i] = int.Parse(bits[i]);
                    }
                    Move move = new Move
                    {
                        FromX = numBits[0],
                        FromY = numBits[1],
                        ToX = numBits[2],
                        ToY = numBits[3]
                    };

                    if (board.IsLegalMove(move))
                    {
                        board.Move(move);
                    }
                    else
                    {
                        Console.WriteLine("ILLEGAL");
                        break;
                    }
                }
                else
                {
                    string input = Console.ReadLine();
                    string[] bits = input.Split(' ');
                    int[] numBits = new int[4];
                    for (int i = 0; i < 4; i++)
                    {
                        numBits[i] = int.Parse(bits[i]);
                    }
                    Move move = new Move
                    {
                        FromX = numBits[0],
                        FromY = numBits[1],
                        ToX = numBits[2],
                        ToY = numBits[3]
                    };

                    if (board.IsLegalMove(move))
                    {
                        board.Move(move);
                    }
                    else
                    {
                        Console.WriteLine("you suck");
                        break;
                    }
                }
            }

            PrintBoard(board);
            if (winner) Console.WriteLine("O wins");
            else Console.WriteLine("0 wins");
        }

        static void PrintBoard(Board board)
        {
            Console.Clear();

            Console.Write("    ");
            for (int i = 0; i < board.Size; i++)
            {
                Console.Write(i + "   ");
            }
            Console.WriteLine("\n");

            for (int i = 0; i < board.Size; i++)
            {
                Console.Write(i + "   ");

                for (int j = 0; j < board.Size; j++)
                {
                    switch (board.SignedPieceAt(i, j))
                    {
                        case PAWN:
                            Console.Write('o');
                            break;
                        case -PAWN:
                            Console.Write('0');
                            break;
                        case KING:
                            Console.Write('O');
                            break;
                        case -KING:
                            Console.Write('@');
                            break;
                        case 0 when i % 2 == j % 2:
                            Console.Write('#');
                            break;
                        default:
                            Console.Write(' ');
                            break;
                    }

                    if (j == board.Size - 1) Console.WriteLine();
                    else Console.Write(" | ");
                }

                if (i != board.Size - 1)
                {
                    Console.Write("   ");

                    for (int j = 0; j < board.Size; j++)
                    {
                        if (j == board.Size - 1) Console.WriteLine("---");
                        else Console.Write("---+");
                    }
                }
            }
            Console.WriteLine();
        }
    }
}
