using System;
using System.Linq;

namespace TicTacToeML.Core
{
    public class TicTacToeEnvironment
    {
        public float[] Board { get; private set; }
        public int CurrentPlayer { get; private set; }

        public TicTacToeEnvironment()
        {
            Reset();
        }

        public void Reset()
        {
            Board = new float[9];
            CurrentPlayer = 1;
        }

        public bool MakeMove(int index)
        {
            if (Board[index] != 0f) return false;

            Board[index] = CurrentPlayer;
            CurrentPlayer *= -1;
            return true;
        }

        public int CheckWinner()
        {
            int[,] winLines = {
                {0, 1, 2}, {3, 4, 5}, {6, 7, 8},
                {0, 3, 6}, {1, 4, 7}, {2, 5, 8},
                {0, 4, 8}, {2, 4, 6}             
            };

            for (int i = 0; i < 8; i++)
            {
                float sum = Board[winLines[i, 0]] + Board[winLines[i, 1]] + Board[winLines[i, 2]];
                if (sum == 3f) return 1;
                if (sum == -3f) return -1;
            }

            if (!Board.Contains(0f)) return 0;
            return -2;
        }

        public float[] GetStateForPlayer(int player)
        {
            float[] state = new float[9];
            for (int i = 0; i < 9; i++) state[i] = Board[i] * player;
            return state;
        }
    }
}