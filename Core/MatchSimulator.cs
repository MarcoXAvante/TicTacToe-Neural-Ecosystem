using System;
using System.Collections.Generic;
using System.Threading;
using TicTacToeML.Events;
using TicTacToeML.Models;

namespace TicTacToeML.Core
{
    public class MatchSimulator
    {
        public event EventHandler<MoveMadeEventArgs> OnMoveMade;
        private readonly Random _random = new Random();

        public int PlayVisualMatch(INeuralNetwork net1, IPersonaStrategy s1, INeuralNetwork net2, IPersonaStrategy s2, int delayMs)
        {
            var env = new TicTacToeEnvironment();
            int result = -2;

            while (result == -2)
            {
                INeuralNetwork currentNet = (env.CurrentPlayer == 1) ? net1 : net2;
                IPersonaStrategy currentStrategy = (env.CurrentPlayer == 1) ? s1 : s2;

                float[] state = env.GetStateForPlayer(env.CurrentPlayer);
                float[] outputs = currentNet.Forward(state);

                int action = GetActionForMatch(state, outputs, currentStrategy);

                if (action == -1) break;

                int activePlayer = env.CurrentPlayer;
                env.MakeMove(action);

                OnMoveMade?.Invoke(this, new MoveMadeEventArgs(env.Board, activePlayer, action));
                Thread.Sleep(delayMs);

                result = env.CheckWinner();
            }
            return result;
        }

        public int PlayHumanVsAIMatch(INeuralNetwork aiNet, IPersonaStrategy aiStrategy, bool humanStarts, Func<float[], int> getHumanMove)
        {
            var env = new TicTacToeEnvironment();
            int result = -2;

            OnMoveMade?.Invoke(this, new MoveMadeEventArgs(env.Board, env.CurrentPlayer, -1));

            while (result == -2)
            {
                int activePlayer = env.CurrentPlayer;
                bool isHumanTurn = (activePlayer == 1 && humanStarts) || (activePlayer == -1 && !humanStarts);
                int action = -1;

                if (isHumanTurn)
                {
                    action = getHumanMove(env.Board);
                }
                else
                {
                    float[] state = env.GetStateForPlayer(activePlayer);
                    float[] outputs = aiNet.Forward(state);
                    action = GetActionForMatch(state, outputs, aiStrategy);
                    Thread.Sleep(800);
                }

                if (action == -1) break;

                env.MakeMove(action);
                result = env.CheckWinner();
                OnMoveMade?.Invoke(this, new MoveMadeEventArgs(env.Board, activePlayer, action));
            }
            return result;
        }

        private int GetActionForMatch(float[] state, float[] outputs, IPersonaStrategy strategy)
        {
            List<int> validMoves = new List<int>();
            for (int i = 0; i < 9; i++) if (state[i] == 0f) validMoves.Add(i);
            if (validMoves.Count == 0) return -1;

            int hardRuleMove = strategy.GetHardRuleMove(state, validMoves);
            if (hardRuleMove != -1) return hardRuleMove;

            float maxQ = -float.MaxValue;
            for (int i = 0; i < 9; i++)
                if (state[i] == 0f && outputs[i] > maxQ) maxQ = outputs[i];

            List<int> topTierMoves = new List<int>();
            for (int i = 0; i < 9; i++)
                if (state[i] == 0f && outputs[i] >= maxQ - 0.05f) topTierMoves.Add(i);

            return topTierMoves[_random.Next(topTierMoves.Count)];
        }
    }
}