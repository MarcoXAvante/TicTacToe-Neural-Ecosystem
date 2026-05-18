using System;
using System.Collections.Generic;
using System.Linq;
using TicTacToeML.Models;

namespace TicTacToeML.Core
{
    public class RLTrainer
    {
        private readonly Random _random = new Random();

        public event Action<int, INeuralNetwork, PersonaType, INeuralNetwork, PersonaType> OnTrainingMilestone;

        private class MoveRecord
        {
            public float[] State { get; set; }
            public int ActionTaken { get; set; }
            public float[] NetworkOutputs { get; set; }
            public float StepReward { get; set; }
        }

        public void TrainEcosystem(Dictionary<PersonaType, INeuralNetwork> agents, int epochs)
        {
            var env = new TicTacToeEnvironment();
            float learningRate = 0.01f;
            float gamma = 0.9f;

            var personas = agents.Keys.ToList();

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                env.Reset();

                PersonaType p1Type = personas[_random.Next(personas.Count)];
                PersonaType p2Type;
                do { p2Type = personas[_random.Next(personas.Count)]; } while (p1Type == p2Type);

                bool p1Starts = _random.Next(2) == 0;

                PersonaType starterType = p1Starts ? p1Type : p2Type;
                PersonaType secondType = p1Starts ? p2Type : p1Type;

                INeuralNetwork netStarter = agents[starterType];
                INeuralNetwork netSecond = agents[secondType];

                var historyStarter = new List<MoveRecord>();
                var historySecond = new List<MoveRecord>();

                float epsilon = Math.Max(0.05f, 1f - ((float)epoch / (epochs * 0.8f)));
                int result = -2;

                while (result == -2)
                {
                    bool isStarterTurn = env.CurrentPlayer == 1;
                    INeuralNetwork currentNet = isStarterTurn ? netStarter : netSecond;
                    var currentHistory = isStarterTurn ? historyStarter : historySecond;
                    PersonaType currentPersona = isStarterTurn ? starterType : secondType;

                    float[] state = env.GetStateForPlayer(env.CurrentPlayer);
                    float[] outputs = currentNet.Forward(state);

                    int action = GetTrainingAction(state, outputs, epsilon, currentPersona);
                    if (action == -1) break;

                    float stepReward = CalculateStepReward(state, action, currentPersona);

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

                float rewardStarter = CalculateFinalReward(result == 1, result == -1, result == 0, starterType);
                float rewardSecond = CalculateFinalReward(result == -1, result == 1, result == 0, secondType);

                ApplyBackpropagation(netStarter, historyStarter, rewardStarter, learningRate, gamma);
                ApplyBackpropagation(netSecond, historySecond, rewardSecond, learningRate, gamma);

                if (epoch % 20000 == 0)
                {
                    OnTrainingMilestone?.Invoke(epoch, netStarter, starterType, netSecond, secondType);
                }
            }
        }

        private int GetTrainingAction(float[] state, float[] outputs, float epsilon, PersonaType persona)
        {
            List<int> validMoves = new List<int>();
            for (int i = 0; i < 9; i++) if (state[i] == 0f) validMoves.Add(i);
            if (validMoves.Count == 0) return -1;

            foreach (int move in validMoves)
                if (IsWinningMove(state, 1f, move)) return move;

            if (persona == PersonaType.Survivor)
            {
                foreach (int move in validMoves)
                    if (IsWinningMove(state, -1f, move)) return move;
            }

            if (persona == PersonaType.Rookie) return validMoves[_random.Next(validMoves.Count)];

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

        private bool IsWinningMove(float[] state, float targetPlayer, int action)
        {
            int[,] winLines = { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 }, { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 }, { 0, 4, 8 }, { 2, 4, 6 } };
            for (int i = 0; i < 8; i++)
            {
                int a = winLines[i, 0], b = winLines[i, 1], c = winLines[i, 2];
                if (a == action || b == action || c == action)
                {
                    float sum = (a != action ? state[a] : 0) + (b != action ? state[b] : 0) + (c != action ? state[c] : 0);
                    if (sum == targetPlayer * 2f) return true;
                }
            }
            return false;
        }

        private float CalculateStepReward(float[] state, int action, PersonaType persona)
        {
            if (IsWinningMove(state, 1f, action)) return 5.0f;

            if (persona == PersonaType.Survivor)
            {
                if (IsWinningMove(state, -1f, action)) return 3.0f;
                if (action == 4) return 0.2f;
            }
            else if (persona == PersonaType.Achiever)
            {
                int[,] winLines = { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 }, { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 }, { 0, 4, 8 }, { 2, 4, 6 } };
                float setupReward = 0f;
                bool isViableAttack = false;

                for (int i = 0; i < 8; i++)
                {
                    int a = winLines[i, 0], b = winLines[i, 1], c = winLines[i, 2];
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
            else if (persona == PersonaType.Planner)
            {
                int misFichas = 0;
                int susFichas = 0;
                for (int i = 0; i < 9; i++)
                {
                    if (state[i] == 1f) misFichas++;
                    if (state[i] == -1f) susFichas++;
                }

                bool empeceYo = (misFichas == susFichas);

                int[] planEmpezando = { 0, 8, 2, 6, 4, 1, 3, 5, 7 };

                int[] planSegundo = { 4, 0, 2, 6, 8, 1, 3, 5, 7 };

                int[] planActivo = empeceYo ? planEmpezando : planSegundo;

                int siguientePasoDelPlan = -1;
                foreach (int paso in planActivo)
                {
                    if (state[paso] == 0f)
                    {
                        siguientePasoDelPlan = paso;
                        break;
                    }
                }

                if (action == siguientePasoDelPlan)
                {
                    return 3.0f;
                }
            }

            return 0f;
        }

        private float CalculateFinalReward(bool isWin, bool isLoss, bool isDraw, PersonaType persona)
        {
            if (isWin) return 1.5f;
            if (persona == PersonaType.Survivor && isLoss) return -2.0f;
            if (isLoss) return -1.0f;

            if (persona == PersonaType.Survivor) return 0.5f;
            if (persona == PersonaType.Achiever) return -0.5f;
            return 0.1f;
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