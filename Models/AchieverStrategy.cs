using System;
using System.Collections.Generic;
using TicTacToeML.Core;

namespace TicTacToeML.Models
{
    public class AchieverStrategy : IPersonaStrategy
    {
        public PersonaType Type => PersonaType.Achiever;

        public int GetHardRuleMove(float[] state, List<int> validMoves)
        {
            foreach (int move in validMoves)
                if (TicTacToeEnvironment.IsWinningMove(state, 1f, move)) return move;

            return -1;
        }

        public float CalculateStepReward(float[] state, int action)
        {
            if (TicTacToeEnvironment.IsWinningMove(state, 1f, action)) return 5.0f;

            float setupReward = 0f;
            bool isViableAttack = false;

            for (int i = 0; i < 8; i++)
            {
                int a = TicTacToeEnvironment.WinLines[i, 0], b = TicTacToeEnvironment.WinLines[i, 1], c = TicTacToeEnvironment.WinLines[i, 2];
                if (a == action || b == action || c == action)
                {
                    float sumOthers = (a != action ? state[a] : 0) + (b != action ? state[b] : 0) + (c != action ? state[c] : 0);
                    if (sumOthers == 1f)
                    {
                        isViableAttack = true;
                        setupReward += 0.5f;
                    }
                }
            }

            if (isViableAttack)
            {
                int row = action / 3, col = action % 3;
                for (int i = 0; i < 9; i++)
                {
                    if (i != action && state[i] == 1f)
                    {
                        if (Math.Abs(row - i / 3) <= 1 && Math.Abs(col - i % 3) <= 1)
                        {
                            setupReward += 3.0f;
                            break;
                        }
                    }
                }
            }
            return setupReward;
        }

        public float CalculateFinalReward(bool isWin, bool isLoss, bool isDraw)
        {
            if (isWin) return 1.5f;
            if (isLoss) return -1.0f;
            return -0.5f;
        }
    }
}