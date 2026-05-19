using System;
using System.Collections.Generic;
using System.Linq;
using TicTacToeML.Models;

namespace TicTacToeML.Core
{
    public class RLTrainer
    {
        private readonly Random _random = new Random();

        public event Action<int, INeuralNetwork, IPersonaStrategy, INeuralNetwork, IPersonaStrategy> OnTrainingMilestone;

        private class MoveRecord
        {
            public float[] State { get; set; }
            public int ActionTaken { get; set; }
            public float[] NetworkOutputs { get; set; }
            public float StepReward { get; set; }
        }

        public void TrainEcosystem(Dictionary<IPersonaStrategy, INeuralNetwork> agents, int epochs)
        {
            var env = new TicTacToeEnvironment();
            float learningRate = 0.01f;
            float gamma = 0.9f;

            var strategies = agents.Keys.ToList();

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                env.Reset();

                IPersonaStrategy p1Strategy = strategies[_random.Next(strategies.Count)];
                IPersonaStrategy p2Strategy;
                do { p2Strategy = strategies[_random.Next(strategies.Count)]; } while (p1Strategy == p2Strategy);

                bool p1Starts = _random.Next(2) == 0;

                IPersonaStrategy starterStrategy = p1Starts ? p1Strategy : p2Strategy;
                IPersonaStrategy secondStrategy = p1Starts ? p2Strategy : p1Strategy;

                INeuralNetwork netStarter = agents[starterStrategy];
                INeuralNetwork netSecond = agents[secondStrategy];

                var historyStarter = new List<MoveRecord>();
                var historySecond = new List<MoveRecord>();

                float epsilon = Math.Max(0.05f, 1f - ((float)epoch / (epochs * 0.8f)));
                int result = -2;

                while (result == -2)
                {
                    bool isStarterTurn = env.CurrentPlayer == 1;
                    INeuralNetwork currentNet = isStarterTurn ? netStarter : netSecond;
                    var currentHistory = isStarterTurn ? historyStarter : historySecond;
                    IPersonaStrategy currentStrategy = isStarterTurn ? starterStrategy : secondStrategy;

                    float[] state = env.GetStateForPlayer(env.CurrentPlayer);
                    float[] outputs = currentNet.Forward(state);

                    int action = GetTrainingAction(state, outputs, epsilon, currentStrategy);
                    if (action == -1) break;

                    float stepReward = currentStrategy.CalculateStepReward(state, action);

                    currentHistory.Add(new MoveRecord
                    {
                        State = state,
                        ActionTaken = action,
                        NetworkOutputs = (float[])outputs.Clone(),
                        StepReward = stepReward
                    });

                    env.MakeMove(action);
                    result = env.CheckWinner();
                }

                float rewardStarter = starterStrategy.CalculateFinalReward(result == 1, result == -1, result == 0);
                float rewardSecond = secondStrategy.CalculateFinalReward(result == -1, result == 1, result == 0);

                ApplyBackpropagation(netStarter, historyStarter, rewardStarter, learningRate, gamma);
                ApplyBackpropagation(netSecond, historySecond, rewardSecond, learningRate, gamma);

                if (epoch % 20000 == 0)
                {
                    OnTrainingMilestone?.Invoke(epoch, netStarter, starterStrategy, netSecond, secondStrategy);
                }
            }
        }

        private int GetTrainingAction(float[] state, float[] outputs, float epsilon, IPersonaStrategy strategy)
        {
            List<int> validMoves = new List<int>();
            for (int i = 0; i < 9; i++) if (state[i] == 0f) validMoves.Add(i);
            if (validMoves.Count == 0) return -1;

            int hardRuleMove = strategy.GetHardRuleMove(state, validMoves);
            if (hardRuleMove != -1) return hardRuleMove;

            if (_random.NextDouble() < epsilon)
                return validMoves[_random.Next(validMoves.Count)];

            int bestMove = -1;
            float maxQ = -float.MaxValue;
            foreach (int move in validMoves)
            {
                if (outputs[move] > maxQ)
                {
                    maxQ = outputs[move];
                    bestMove = move;
                }
            }
            return bestMove;
        }

        private void ApplyBackpropagation(INeuralNetwork net, List<MoveRecord> history, float finalReward, float lr, float gamma)
        {
            float currentFutureReward = finalReward;
            for (int i = history.Count - 1; i >= 0; i--)
            {
                var record = history[i];
                float totalTargetReward = record.StepReward + currentFutureReward;

                float[] expected = (float[])record.NetworkOutputs.Clone();
                expected[record.ActionTaken] = totalTargetReward;

                net.Forward(record.State);
                net.Backpropagate(expected, lr);

                currentFutureReward = totalTargetReward * gamma;
            }
        }
    }
}