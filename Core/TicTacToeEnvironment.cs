using System;

namespace TicTacToeML.Core
{
    public class TicTacToeEnvironment
    {
        public float[] Board { get; private set; }
        public int CurrentPlayer { get; private set; }

        public static readonly int[,] WinLines = {
            {0,1,2}, {3,4,5}, {6,7,8}, 
            {0,3,6}, {1,4,7}, {2,5,8}, 
            {0,4,8}, {2,4,6}          
        };

        public TicTacToeEnvironment()
        {
            Reset();
        }

        public void Reset()
        {
            Board = new float[9];
            CurrentPlayer = 1;
        }

        public void MakeMove(int action)
        {
            if (Board[action] != 0f)
                throw new InvalidOperationException("Invalid move");

            Board[action] = CurrentPlayer;
            CurrentPlayer = -CurrentPlayer;
        }

        public int CheckWinner()
        {
            for (int i = 0; i < 8; i++)
            {
                float sum = Board[WinLines[i, 0]] + Board[WinLines[i, 1]] + Board[WinLines[i, 2]];
                if (sum == 3f) return 1;
                if (sum == -3f) return -1;
            }

            bool isDraw = true;
            foreach (var cell in Board)
            {
                if (cell == 0f) { isDraw = false; break; }
            }

            if (isDraw) return 0;
            return -2;
        }

        public float[] GetStateForPlayer(int player)
        {
            float[] state = new float[9];
            for (int i = 0; i < 9; i++)
            {
                state[i] = Board[i] * player;
            }
            return state;
        }

        public static bool IsWinningMove(float[] state, float targetPlayer, int action)
        {
            for (int i = 0; i < 8; i++)
            {
                int a = WinLines[i, 0], b = WinLines[i, 1], c = WinLines[i, 2];
                if (a == action || b == action || c == action)
                {
                    float sum = (a != action ? state[a] : 0) +
                                (b != action ? state[b] : 0) +
                                (c != action ? state[c] : 0);

                    if (sum == targetPlayer * 2f) return true;
                }
            }
            return false;
        }
    }
}