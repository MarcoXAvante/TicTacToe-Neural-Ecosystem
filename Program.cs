using System;
using System.Collections.Generic;
using System.Threading;
using TicTacToeML.Core;
using TicTacToeML.Events;
using TicTacToeML.Models;

namespace TicTacToeML
{
    class Program
    {
        static MatchSimulator _simulator;
        static string _p1NameUI;
        static string _p2NameUI;
        static bool _isHumanPlaying = false;

        static void Main(string[] args)
        {
            Console.WriteLine("=== 4-PERSONALITY ECOSYSTEM SIMULATION (100,000 Epochs) ===\n");

            int[] layers = { 9, 36, 18, 9 };

            var agents = new Dictionary<PersonaType, INeuralNetwork>
            {
                { PersonaType.Achiever, new NeuralNetwork(layers) },
                { PersonaType.Survivor, new NeuralNetwork(layers) },
                { PersonaType.Planner, new NeuralNetwork(layers) },
                { PersonaType.Rookie, new NeuralNetwork(layers) }
            };

            _simulator = new MatchSimulator();
            _simulator.OnMoveMade += RenderBoardUpdate;

            Console.WriteLine("Initializing massive ecosystem training (Random Round Robin)...");
            RLTrainer trainer = new RLTrainer();

            trainer.OnTrainingMilestone += (epoch, net1, p1Type, net2, p2Type) =>
            {
                Console.WriteLine($"\n>>> EXHIBITION MATCH - EPOCH {epoch}: {p1Type} vs {p2Type} <<<");
                _p1NameUI = p1Type.ToString();
                _p2NameUI = p2Type.ToString();
                Thread.Sleep(1000);

                _simulator.PlayVisualMatch(net1, p1Type, net2, p2Type, delayMs: 200);

                Console.WriteLine($"\n>>> Resuming training process... <<<\n");
                Thread.Sleep(500);
            };

            trainer.TrainEcosystem(agents, epochs: 100000);

            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("   TRAINING PROCESS COMPLETED");
            Console.WriteLine("   WELCOME TO THE FINAL TOURNAMENT (BO3)");
            Console.WriteLine("========================================");
            Console.WriteLine("Press Enter to commence Semifinal 1...");
            Console.ReadLine();

            var winnerSemi1 = RunBO3(agents, PersonaType.Achiever, PersonaType.Rookie, "SEMIFINAL 1");

            Console.WriteLine("\nPress Enter to commence Semifinal 2...");
            Console.ReadLine();

            var winnerSemi2 = RunBO3(agents, PersonaType.Planner, PersonaType.Survivor, "SEMIFINAL 2");

            Console.WriteLine("\nPress Enter to commence the GRAND FINAL...");
            Console.ReadLine();

            RunBO3(agents, winnerSemi1, winnerSemi2, "GRAND FINAL");

            _isHumanPlaying = true;
            PlayAgainstAI(agents);
        }

        static PersonaType RunBO3(Dictionary<PersonaType, INeuralNetwork> agents, PersonaType p1, PersonaType p2, string title)
        {
            int winsP1 = 0, winsP2 = 0, draws = 0, matchCount = 1;

            while (winsP1 < 2 && winsP2 < 2 && matchCount <= 3)
            {
                Console.Clear();
                Console.WriteLine($"=== {title} - MATCH {matchCount} ===");
                Console.WriteLine($"Scoreboard: {p1} ({winsP1}) | {p2} ({winsP2}) | Draws: {draws}");

                int result;

                if (matchCount % 2 != 0)
                {
                    _p1NameUI = p1.ToString();
                    _p2NameUI = p2.ToString();
                    Console.WriteLine($"\nCommencing: {p1} (Utilizing token '{_p1NameUI[0]}').");
                    Thread.Sleep(1500);
                    result = _simulator.PlayVisualMatch(agents[p1], p1, agents[p2], p2, 1000);

                    if (result == 1) winsP1++;
                    else if (result == -1) winsP2++;
                    else draws++;
                }
                else
                {
                    _p1NameUI = p2.ToString();
                    _p2NameUI = p1.ToString();
                    Console.WriteLine($"\nCommencing: {p2} (Utilizing token '{_p1NameUI[0]}').");
                    Thread.Sleep(1500);
                    result = _simulator.PlayVisualMatch(agents[p2], p2, agents[p1], p1, 1000);

                    if (result == 1) winsP2++;
                    else if (result == -1) winsP1++;
                    else draws++;
                }

                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
                matchCount++;
            }

            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine($" RESULT - {title}");
            Console.WriteLine("========================================");
            if (winsP1 > winsP2)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"¡{p1.ToString().ToUpper()} WINS THE SERIES!");
                Console.ResetColor();
                return p1;
            }
            else if (winsP2 > winsP1)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"¡{p2.ToString().ToUpper()} WINS THE SERIES!");
                Console.ResetColor();
                return p2;
            }

            Console.WriteLine($"The series concluded in a draw. {p1} advances via technical tie-break.");
            return p1;
        }

        static void PlayAgainstAI(Dictionary<PersonaType, INeuralNetwork> agents)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("   FREEPLAY MODE: HUMAN VS AI");
                Console.WriteLine("========================================");
                Console.Write("Would you like to initiate a match against the trained agents? (y/n): ");
                if (Console.ReadLine()?.ToLower() != "y") break;

                Console.WriteLine("\nPlease select your opponent:");
                Console.WriteLine("1. Achiever (Aggressive)");
                Console.WriteLine("2. Survivor (Defensivo)");
                Console.WriteLine("3. Planner (Strategic)");
                Console.WriteLine("4. Rookie (Novice)");
                Console.Write("Selection (1-4): ");

                PersonaType selectedPersona = PersonaType.Rookie;
                string sel = Console.ReadLine();
                if (sel == "1") selectedPersona = PersonaType.Achiever;
                else if (sel == "2") selectedPersona = PersonaType.Survivor;
                else if (sel == "3") selectedPersona = PersonaType.Planner;
                else if (sel == "4") selectedPersona = PersonaType.Rookie;
                else
                {
                    Console.WriteLine("Invalid selection. Please try again.");
                    Thread.Sleep(1000);
                    continue;
                }

                Console.WriteLine($"\nYou have selected to confront: {selectedPersona}...");
                Console.Write("Do you wish to take the first move? (y/n): ");
                bool humanStarts = Console.ReadLine()?.ToLower() == "y";

                _p1NameUI = humanStarts ? "Human" : selectedPersona.ToString();
                _p2NameUI = humanStarts ? selectedPersona.ToString() : "Human";

                Console.WriteLine("\nInitializing the board state. Press Enter to proceed...");
                Console.ReadLine();

                int result = _simulator.PlayHumanVsAIMatch(agents[selectedPersona], selectedPersona, humanStarts, GetHumanMove);

                Console.WriteLine("\n========================================");
                if ((result == 1 && humanStarts) || (result == -1 && !humanStarts))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("VICTORY. You have successfully defeated the artificial agent.");
                }
                else if (result == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("DRAW. The match concluded in an equal tactical balance.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DEFEAT. The AI agent has demonstrated strategic superiority.");
                }
                Console.ResetColor();
                Console.WriteLine("========================================");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
            }

            Console.WriteLine("\nThank you for participating. Terminating application...");
        }

        private static int GetHumanMove(float[] board)
        {
            int move = -1;
            while (true)
            {
                Console.Write("\nYour turn. Enter the index of an available cell (0-8): ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out move) && move >= 0 && move <= 8)
                {
                    if (board[move] == 0f) break;
                    else Console.WriteLine("Error: Selected cell is already occupied.");
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter an integer between 0 and 8.");
                }
            }
            return move;
        }

        private static void RenderBoardUpdate(object sender, MoveMadeEventArgs e)
        {
            Console.Clear();

            bool isP1Turn = e.Player == 1;
            char p1Initial = _p1NameUI[0];
            char p2Initial = _p2NameUI[0];

            if (e.ActionTaken != -1)
            {
                Console.WriteLine($"Current Turn: {(isP1Turn ? $"Token '{p1Initial}' ({_p1NameUI})" : $"Token '{p2Initial}' ({_p2NameUI})")}");
                Console.WriteLine($"Selected cell index: {e.ActionTaken}\n");
            }
            else
            {
                Console.WriteLine("=== CURRENT BOARD STATE ===\n");
            }

            char Rep(float val, int index)
            {
                if (val == 0f)
                    return _isHumanPlaying ? index.ToString()[0] : '·';

                if (val == 1f) return p1Initial;
                return p2Initial;
            }

            Console.WriteLine($"  {Rep(e.Board[0], 0)} | {Rep(e.Board[1], 1)} | {Rep(e.Board[2], 2)} ");
            Console.WriteLine(" ---+---+---");
            Console.WriteLine($"  {Rep(e.Board[3], 3)} | {Rep(e.Board[4], 4)} | {Rep(e.Board[5], 5)} ");
            Console.WriteLine(" ---+---+---");
            Console.WriteLine($"  {Rep(e.Board[6], 6)} | {Rep(e.Board[7], 7)} | {Rep(e.Board[8], 8)} \n");
        }
    }
}