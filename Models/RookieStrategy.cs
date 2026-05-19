using System;
using System.Collections.Generic;
using TicTacToeML.Core;

namespace TicTacToeML.Models
{
    public class RookieStrategy : IPersonaStrategy
    {
        public PersonaType Type => PersonaType.Rookie;
        private readonly Random _random = new Random();

        public int GetHardRuleMove(float[] state, List<int> validMoves)
        {
            foreach (int move in validMoves)
                if (TicTacToeEnvironment.IsWinningMove(state, 1f, move)) return move;

            return validMoves[_random.Next(validMoves.Count)];
        }

        public float CalculateStepReward(float[] state, int action) => 0f;
        public float CalculateFinalReward(bool isWin, bool isLoss, bool isDraw) => 0f;
    }
}