using System;
using TicTacToeML.Events;

namespace TicTacToeML.UI
{
    public class ConsoleView
    {
        public string PlayerOneName { get; set; } = "P1";
        public string PlayerTwoName { get; set; } = "P2";
        public bool IsHumanPlaying { get; set; } = false;

        public void RenderBoardUpdate(object sender, MoveMadeEventArgs e)
        {
            Console.Clear();

            bool isPlayerOneTurn = e.Player == 1;
            char p1Initial = PlayerOneName[0];
            char p2Initial = PlayerTwoName[0];

            if (e.ActionTaken != -1)
            {
                Console.WriteLine($"Current Turn: {(isPlayerOneTurn ? $"Token '{p1Initial}' ({PlayerOneName})" : $"Token '{p2Initial}' ({PlayerTwoName})")}");
                Console.WriteLine($"Selected cell index: {e.ActionTaken}\n");
            }
            else
            {
                Console.WriteLine("=== CURRENT BOARD STATE ===\n");
            }

            char GetTokenRepresentation(float val, int index)
            {
                if (val == 0f) return IsHumanPlaying ? index.ToString()[0] : '·';
                if (val == 1f) return p1Initial;
                return p2Initial;
            }

            Console.WriteLine($"  {GetTokenRepresentation(e.Board[0], 0)} | {GetTokenRepresentation(e.Board[1], 1)} | {GetTokenRepresentation(e.Board[2], 2)} ");
            Console.WriteLine(" ---+---+---");
            Console.WriteLine($"  {GetTokenRepresentation(e.Board[3], 3)} | {GetTokenRepresentation(e.Board[4], 4)} | {GetTokenRepresentation(e.Board[5], 5)} ");
            Console.WriteLine(" ---+---+---");
            Console.WriteLine($"  {GetTokenRepresentation(e.Board[6], 6)} | {GetTokenRepresentation(e.Board[7], 7)} | {GetTokenRepresentation(e.Board[8], 8)} \n");
        }

        public int GetHumanMove(float[] board)
        {
            int move = -1;
            while (true)
            {
                Console.Write("\nYour turn. Enter the index of an available cell (0-8): ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out move) && move >= 0 && move <= 8)
                {
                    if (board[move] == 0f) break;
                    else Console.WriteLine("Error: Selected cell is already occupied.");
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter an integer between 0 and 8.");
                }
            }
            return move;
        }
    }
}