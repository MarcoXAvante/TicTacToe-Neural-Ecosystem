using System.Collections.Generic;

namespace TicTacToeML.Models
{
    public interface IPersonaStrategy
    {
        PersonaType Type { get; }

        int GetHardRuleMove(float[] state, List<int> validMoves);

        float CalculateStepReward(float[] state, int action);

        float CalculateFinalReward(bool isWin, bool isLoss, bool isDraw);
    }
}