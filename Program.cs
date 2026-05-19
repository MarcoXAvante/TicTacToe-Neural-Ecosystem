using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TicTacToeML.Core;
using TicTacToeML.Models;
using TicTacToeML.UI;

namespace TicTacToeML
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== 4-PERSONALITY ECOSYSTEM SIMULATION (100,000 Epochs) ===\n");

            int[] networkLayers = { 9, 36, 18, 9 };

            var agents = new Dictionary<IPersonaStrategy, INeuralNetwork>
            {
                { new AchieverStrategy(), new NeuralNetwork(networkLayers) },
                { new SurvivorStrategy(), new NeuralNetwork(networkLayers) },
                { new PlannerStrategy(), new NeuralNetwork(networkLayers) },
                { new RookieStrategy(), new NeuralNetwork(networkLayers) }
            };

            var view = new ConsoleView();
            var simulator = new MatchSimulator();
            simulator.OnMoveMade += view.RenderBoardUpdate;

            var tournamentManager = new TournamentManager(simulator, view);
            var trainer = new RLTrainer();

            Console.WriteLine("Initializing massive ecosystem training (Random Round Robin)...");
            trainer.OnTrainingMilestone += (epoch, net1, strat1, net2, strat2) =>
            {
                Console.WriteLine($"\n>>> EXHIBITION MATCH - EPOCH {epoch}: {strat1.Type} vs {strat2.Type} <<<");
                view.PlayerOneName = strat1.Type.ToString();
                view.PlayerTwoName = strat2.Type.ToString();
                Thread.Sleep(1000);

                simulator.PlayVisualMatch(net1, strat1, net2, strat2, delayMs: 200);

                Console.WriteLine($"\n>>> Resuming training process... <<<\n");
                Thread.Sleep(500);
            };

            trainer.TrainEcosystem(agents, epochs: 100000);

            var strategies = agents.Keys.ToList();
            var achiever = strategies.First(s => s.Type == PersonaType.Achiever);
            var rookie = strategies.First(s => s.Type == PersonaType.Rookie);
            var planner = strategies.First(s => s.Type == PersonaType.Planner);
            var survivor = strategies.First(s => s.Type == PersonaType.Survivor);

            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("   TRAINING PROCESS COMPLETED");
            Console.WriteLine("   WELCOME TO THE FINAL TOURNAMENT (BO3)");
            Console.WriteLine("========================================");
            Console.WriteLine("Press Enter to commence Semifinal 1...");
            Console.ReadLine();

            var winnerSemi1 = tournamentManager.RunBestOf3(agents, achiever, rookie, "SEMIFINAL 1");

            Console.WriteLine("\nPress Enter to commence Semifinal 2...");
            Console.ReadLine();

            var winnerSemi2 = tournamentManager.RunBestOf3(agents, planner, survivor, "SEMIFINAL 2");

            Console.WriteLine("\nPress Enter to commence the GRAND FINAL...");
            Console.ReadLine();

            tournamentManager.RunBestOf3(agents, winnerSemi1, winnerSemi2, "GRAND FINAL");

            tournamentManager.PlayFreeplayMode(agents);
        }
    }
}