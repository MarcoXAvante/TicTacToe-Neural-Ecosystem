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

        public int PlayVisualMatch(INeuralNetwork net1, PersonaType p1, INeuralNetwork net2, PersonaType p2, int delayMs)
        {
            var env = new TicTacToeEnvironment();
            int result = -2;

            while (result == -2)
            {
                INeuralNetwork currentNet = (env.CurrentPlayer == 1) ? net1 : net2;
                PersonaType currentPersona = (env.CurrentPlayer == 1) ? p1 : p2;

                float[] state = env.GetStateForPlayer(env.CurrentPlayer);
                float[] outputs = currentNet.Forward(state);

                int action = GetActionForMatch(state, outputs, currentPersona);

                if (action == -1) break;

                int activePlayer = env.CurrentPlayer;
                env.MakeMove(action);

                OnMoveMade?.Invoke(this, new MoveMadeEventArgs(env.Board, activePlayer, action));
                Thread.Sleep(delayMs);

                result = env.CheckWinner();
            }

            return result;
        }

        public int PlayHumanVsAIMatch(INeuralNetwork aiNet, PersonaType aiPersona, bool humanStarts, Func<float[], int> getHumanMove)
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
                    action = GetActionForMatch(state, outputs, aiPersona);
                    Thread.Sleep(800);
                }

                if (action == -1) break;

                env.MakeMove(action);
                result = env.CheckWinner();

                OnMoveMade?.Invoke(this, new MoveMadeEventArgs(env.Board, activePlayer, action));
            }

            return result;
        }

        private int GetActionForMatch(float[] state, float[] outputs, PersonaType persona)
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

            float maxQ = -float.MaxValue;
            for (int i = 0; i < 9; i++)
                if (state[i] == 0f && outputs[i] > maxQ) maxQ = outputs[i];

            List<int> topTierMoves = new List<int>();
            for (int i = 0; i < 9; i++)
                if (state[i] == 0f && outputs[i] >= maxQ - 0.05f) topTierMoves.Add(i);

            return topTierMoves[_random.Next(topTierMoves.Count)];
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
    }
}