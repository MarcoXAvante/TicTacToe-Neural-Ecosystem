using System.Collections.Generic;
using TicTacToeML.Core;

namespace TicTacToeML.Models
{
    public class PlannerStrategy : IPersonaStrategy
    {
        public PersonaType Type => PersonaType.Planner;

        public int GetHardRuleMove(float[] state, List<int> validMoves)
        {
            foreach (int move in validMoves)
                if (TicTacToeEnvironment.IsWinningMove(state, 1f, move)) return move;

            return -1;
        }

        public float CalculateStepReward(float[] state, int action)
        {
            if (TicTacToeEnvironment.IsWinningMove(state, 1f, action)) return 5.0f;

            int myTokens = 0;
            int opponentTokens = 0;

            for (int i = 0; i < 9; i++)
            {
                if (state[i] == 1f) myTokens++;
                if (state[i] == -1f) opponentTokens++;
            }

            bool isInitiator = (myTokens == opponentTokens);

            int[] aggressiveOpening = { 0, 8, 2, 6, 4, 1, 3, 5, 7 };
            int[] defensiveOpening = { 4, 0, 2, 6, 8, 1, 3, 5, 7 };

            int[] activePlan = isInitiator ? aggressiveOpening : defensiveOpening;
            int nextPlanStep = -1;

            foreach (int step in activePlan)
            {
                if (state[step] == 0f)
                {
                    nextPlanStep = step;
                    break;
                }
            }

            if (action == nextPlanStep) return 3.0f;

            return 0f;
        }

        public float CalculateFinalReward(bool isWin, bool isLoss, bool isDraw)
        {
            if (isWin) return 1.5f;
            if (isLoss) return -1.0f;
            return 0.1f;
        }
    }
}