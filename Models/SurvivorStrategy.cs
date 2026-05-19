using System.Collections.Generic;
using TicTacToeML.Core;

namespace TicTacToeML.Models
{
    public class SurvivorStrategy : IPersonaStrategy
    {
        public PersonaType Type => PersonaType.Survivor;

        public int GetHardRuleMove(float[] state, List<int> validMoves)
        {
            foreach (int move in validMoves)
                if (TicTacToeEnvironment.IsWinningMove(state, 1f, move)) return move;

            foreach (int move in validMoves)
                if (TicTacToeEnvironment.IsWinningMove(state, -1f, move)) return move;

            return -1;
        }

        public float CalculateStepReward(float[] state, int action)
        {
            if (TicTacToeEnvironment.IsWinningMove(state, 1f, action)) return 5.0f;
            if (TicTacToeEnvironment.IsWinningMove(state, -1f, action)) return 3.0f;
            if (action == 4) return 0.2f;
            return 0f;
        }

        public float CalculateFinalReward(bool isWin, bool isLoss, bool isDraw)
        {
            if (isWin) return 1.5f;
            if (isLoss) return -2.0f;
            return 0.5f;
        }
    }
}