using System;
using System.Collections.Generic;
using System.Threading;
using TicTacToeML.Models;
using TicTacToeML.UI;

namespace TicTacToeML.Core
{
    public class TournamentManager
    {
        private readonly MatchSimulator _simulator;
        private readonly ConsoleView _view;

        public TournamentManager(MatchSimulator simulator, ConsoleView view)
        {
            _simulator = simulator ?? throw new ArgumentNullException(nameof(simulator));
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IPersonaStrategy RunBestOf3(Dictionary<IPersonaStrategy, INeuralNetwork> agents, IPersonaStrategy p1, IPersonaStrategy p2, string title)
        {
            int winsP1 = 0, winsP2 = 0, draws = 0, matchCount = 1;

            while (winsP1 < 2 && winsP2 < 2 && matchCount <= 3)
            {
                Console.Clear();
                Console.WriteLine($"=== {title} - MATCH {matchCount} ===");
                Console.WriteLine($"Scoreboard: {p1.Type} ({winsP1}) | {p2.Type} ({winsP2}) | Draws: {draws}");

                int result;

                if (matchCount % 2 != 0)
                {
                    _view.PlayerOneName = p1.Type.ToString();
                    _view.PlayerTwoName = p2.Type.ToString();
                    Console.WriteLine($"\nCommencing: {p1.Type} (Utilizing token '{_view.PlayerOneName[0]}').");
                    Thread.Sleep(1500);
                    result = _simulator.PlayVisualMatch(agents[p1], p1, agents[p2], p2, 1000);

                    if (result == 1) winsP1++; else if (result == -1) winsP2++; else draws++;
                }
                else
                {
                    _view.PlayerOneName = p2.Type.ToString();
                    _view.PlayerTwoName = p1.Type.ToString();
                    Console.WriteLine($"\nCommencing: {p2.Type} (Utilizing token '{_view.PlayerOneName[0]}').");
                    Thread.Sleep(1500);
                    result = _simulator.PlayVisualMatch(agents[p2], p2, agents[p1], p1, 1000);

                    if (result == 1) winsP2++; else if (result == -1) winsP1++; else draws++;
                }

                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
                matchCount++;
            }

            return AnnounceTournamentWinner(p1, p2, winsP1, winsP2, title);
        }

        private IPersonaStrategy AnnounceTournamentWinner(IPersonaStrategy p1, IPersonaStrategy p2, int winsP1, int winsP2, string title)
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine($" RESULT - {title}");
            Console.WriteLine("========================================");

            if (winsP1 > winsP2)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[{p1.Type.ToString().ToUpper()}] WINS THE SERIES!");
                Console.ResetColor();
                return p1;
            }
            if (winsP2 > winsP1)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{p2.Type.ToString().ToUpper()}] WINS THE SERIES!");
                Console.ResetColor();
                return p2;
            }

            Console.WriteLine($"The series concluded in a draw. [{p1.Type}] advances via technical tie-break.");
            return p1;
        }

        public void PlayFreeplayMode(Dictionary<IPersonaStrategy, INeuralNetwork> agents)
        {
            _view.IsHumanPlaying = true;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("   FREEPLAY MODE: HUMAN VS AI");
                Console.WriteLine("========================================");
                Console.Write("Would you like to initiate a match against the trained agents? (y/n): ");
                if (Console.ReadLine()?.ToLower() != "y") break;

                Console.WriteLine("\nPlease select your opponent:");

                int index = 1;
                var strategyList = new List<IPersonaStrategy>(agents.Keys);
                foreach (var strat in strategyList)
                {
                    Console.WriteLine($"{index}. {strat.Type}");
                    index++;
                }

                Console.Write($"Selection (1-{agents.Count}): ");
                if (!int.TryParse(Console.ReadLine(), out int selection) || selection < 1 || selection > agents.Count)
                {
                    Console.WriteLine("Invalid selection. Please try again.");
                    Thread.Sleep(1000);
                    continue;
                }

                IPersonaStrategy selectedStrategy = strategyList[selection - 1];

                Console.WriteLine($"\nYou have selected to confront: {selectedStrategy.Type}...");
                Console.Write("Do you wish to take the first move? (y/n): ");
                bool humanStarts = Console.ReadLine()?.ToLower() == "y";

                _view.PlayerOneName = humanStarts ? "Human" : selectedStrategy.Type.ToString();
                _view.PlayerTwoName = humanStarts ? selectedStrategy.Type.ToString() : "Human";

                Console.WriteLine("\nInitializing the board state. Press Enter to proceed...");
                Console.ReadLine();

                int result = _simulator.PlayHumanVsAIMatch(agents[selectedStrategy], selectedStrategy, humanStarts, _view.GetHumanMove);

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
    }
}