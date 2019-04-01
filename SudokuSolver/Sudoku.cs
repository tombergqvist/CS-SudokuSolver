using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SudokuSolver
{
    internal class Sudoku
    {
        Stopwatch sw = new Stopwatch();
        private readonly char[,] board = new char[9, 9];
        public string BoardAsText { get; set; }
        private bool debug;
        int tries = 0;

        public Sudoku(string boardAsText, bool debug)
        {
            this.debug = debug;
            CreateBoard(boardAsText);
        }

        private void FormatBoard(char[,] board)
        {
            BoardAsText = "";
            for (var row = 0; row < board.GetLength(0); row++)
            {
                for (var column = 0; column < board.GetLength(1); column++)
                    BoardAsText += board[row, column] + "  ";
                BoardAsText += "\n";
            }
        }

        public void CreateBoard(string table)
        {
            for (var row = 0; row < board.GetLength(0); row++)
                for (var column = 0; column < board.GetLength(1); column++)
                {
                    if (int.TryParse(table[row * 9 + column].ToString(), out int trash))
                    {
                        board[row, column] = table[row * 9 + column];
                    }
                    else
                    {
                        board[row, column] = '0';
                    }
                }
            FormatBoard(board);
        }

        public void Solve()
        {

            sw.Start();
            Console.Clear();
            Console.WriteLine(BoardAsText);
            char[,] b = Solve(board);
            sw.Stop();
            if (b != null)
            {
                Console.SetCursorPosition(0, board.GetLength(1) + 1); FormatBoard(b);
                Console.WriteLine("After " + tries + " guesses I found this solution:\n\n" + BoardAsText + "\nIt only took me {0:##0.0} seconds to find the answer, because I'm smart", sw.Elapsed.TotalSeconds);
            }
            else
            {
                Console.SetCursorPosition(0, board.GetLength(1) + 3);
                Console.WriteLine("Not fair! This one is unsolvable! \nIt took me {0:##0.##} seconds to figure that out, because I'm smart.", sw.Elapsed.TotalSeconds);
            }
        }

        public char[,] Solve(char[,] currentBoard)
        {
            var hasEmptyCell = false;
            var iGiveUp = true;
            var guess = false;
            var bestCell = -1;

            do
            {
                hasEmptyCell = false;
                iGiveUp = true;
                for (var row = 0; row < currentBoard.GetLength(0); row++)
                {
                    for (var column = 0; column < currentBoard.GetLength(1); column++)
                    {
                        // If this cell is empty
                        if (currentBoard[row, column] == '0')
                        {
                            if (debug) PrintWithColor(row, column, '_', 0, ConsoleColor.DarkRed);
                            hasEmptyCell = true;
                            // Check if 1-9 is possible in this cell
                            List<char> availableNums = GetAvailableNums(currentBoard, row, column);
                            // If we are guessing find the best cell to start guessing on
                            if (guess && availableNums.Count > 1)
                            {
                                if (bestCell < 0) bestCell = row * 9 + column;
                                else if (availableNums.Count <= GetAvailableNums(currentBoard, bestCell / 9, bestCell % 9).Count)
                                    bestCell = row * 9 + column;
                            }
                            if (availableNums.Count == 0) return null;
                            else if (availableNums.Count == 1)
                            {
                                currentBoard[row, column] = availableNums.First();
                                if (debug) PrintWithColor(row, column, currentBoard[row, column], 0, ConsoleColor.DarkGreen);
                                iGiveUp = false;
                                guess = false;
                            }
                        }
                    }
                    // If we are in the end of this board
                    if (row == currentBoard.GetLength(0) - 1)
                    {
                        // If no unique numbers were found we have to start guessing
                        if (iGiveUp) guess = true;
                        // If we have found the best cell to start guessing on
                        if (iGiveUp && bestCell >= 0)
                        {
                            int r = bestCell / 9;
                            int c = bestCell % 9;
                            foreach (char num in GetAvailableNums(currentBoard, r, c))
                            {
                                if (debug) PrintWithColor(r, c, num, 0, ConsoleColor.DarkRed);
                                char[,] testBoard = currentBoard.Clone() as char[,];
                                testBoard[r, c] = num;
                                tries++;
                                // Try the number in a new recursion of a cloned board
                                testBoard = Solve(testBoard);
                                if (testBoard != null)
                                {
                                    return testBoard;
                                }
                            }
                            Console.SetCursorPosition(0, board.GetLength(0) + 2);
                            return null;
                        }
                    }
                }
            } while (hasEmptyCell);
            return currentBoard;
        }

        // Returns available numbers for this cell
        private List<char> GetAvailableNums(char[,] currentBoard, int row, int column)
        {
            List<char> availableNums = new List<char>();
            for (var num = 1; num < 10; num++)
            {
                var checkNum = (char)(num + 48);
                if (IsNotInRow(currentBoard, checkNum, row)
                    && IsNotInColumn(currentBoard, checkNum, column)
                    && IsNotInBox(currentBoard, checkNum, row, column))
                {
                    availableNums.Add(checkNum);
                }
            }
            return availableNums;
        }

        // Check if number is not in row
        private bool IsNotInRow(char[,] board, char num, int row)
        {
            for (var column = 0; column < board.GetLength(0); column++)
                if (board[row, column] == num) return false;

            return true;
        }

        // Check if number is not in column
        private bool IsNotInColumn(char[,] board, char num, int column)
        {
            for (var row = 0; row < board.GetLength(1); row++)
                if (board[row, column] == num)
                    return false;
            return true;
        }

        // Check if number is not in box
        private bool IsNotInBox(char[,] board, char num, int row, int column)
        {
            var startColumn = GetStartFromThisBox(column);
            var startRow = GetStartFromThisBox(row);

            for (var r = startRow; r < startRow + 3; r++)
                for (var c = startColumn; c < startColumn + 3; c++)
                    if (board[r, c] == num)
                        return false;
            return true;
        }

        // Get the start position in a box (Upper left corner)
        private static int GetStartFromThisBox(int element)
        {
            if (element < 3)
                return 0;
            else if (element < 6)
                return 3;
            else
                return 6;
        }

        // Print a number at correct position with specified color
        private static void PrintWithColor(int row, int column, char num, int sleepTime, ConsoleColor color)
        {
            Console.SetCursorPosition(column * 3, row);
            Console.ForegroundColor = color;
            Console.Write(num);
            Thread.Sleep(sleepTime);
        }
    }
}