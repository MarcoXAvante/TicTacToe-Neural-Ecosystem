# TicTacToe: Procedural Personas & Hybrid AI Sandbox

![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![Artificial Intelligence](https://img.shields.io/badge/AI-Hybrid_Agents-FF6F00?style=for-the-badge)

This repository contains a research environment built in **pure C#** to experiment with **Reinforcement Learning** and Artificial Neural Networks from scratch. 

Using the classic game of Tic-Tac-Toe as a foundation, the project implements the concept of **Procedural Personas**: Artificial Intelligence agents that develop asymmetrical behaviors and specific playstyles through the mathematical alteration of their utility functions (*Reward Shaping*).

## 🧠 What does this project do?

The system simultaneously trains and pits 4 distinct Artificial Intelligence archetypes against each other. Each has been trained by modifying its intermediate "carrots" or *Step Rewards*, forcing them to adopt thematic playstyles even if it means deviating from the mathematically perfect strategy:

1. ⚔️ **Achiever (Aggressive):** Plays recklessly. Rewarded for creating viable attacks and placing physically adjacent tokens ("clusters"), prioritizing suffocating the opponent.
2. 🛡️ **Survivor (Defensive):** Optimized for survival. Rewarded for blocking imminent attacks and dominating the center of the board to force tactical draws.
3. 📐 **Planner (Strategic):** Simulates a *min-maxer* player. Evaluates who has the initiative of the match and executes a strict master plan (Corner Trap if starting, Central Control if going second).
4. 🎲 **Rookie (Novice):** Acts as a control group. Knows how to finish a game if victory is one step away, but otherwise makes completely random decisions.

## ⚙️ Technical Features

* **Neural Network "From Scratch":** Native Multilayer Perceptron (MLP) in C# without external libraries (no TensorFlow or PyTorch), implementing *Feedforward* and *Backpropagation*.
* **Hybrid Architecture:** The agents combine *Soft-Rules* (neural network-based decisions) with *Hard-Rules* (traditional code heuristics) to prevent deterministic loops and guarantee lethal gameplay.
* **Q-Learning & Vanishing Gradient Prevention:** Training based on *Self-Play* with $\epsilon$-Greedy decay. Uses the **Tanh** activation function and Linear outputs to process negative rewards and Q-Values without collapsing the mathematical weights.
* **Interactive Simulation:** The system executes an automated training of 100,000 epochs visible in the console, followed by a Best-of-3 AI vs AI tournament, and culminates in a *Freeplay* mode where a human can face off against the trained networks.

## 🚀 How to run it

This project is a standard .NET console application. To test it:

1. Clone or download this repository to your local machine.
2. Open the solution file (`TicTacToe.sln`) with your preferred development environment (Visual Studio, Rider, VS Code, etc.).
3. Simply hit **Run (or Play)** to compile and launch the simulator directly in your console.
