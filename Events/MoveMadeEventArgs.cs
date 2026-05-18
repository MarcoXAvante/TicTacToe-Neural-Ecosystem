using System;

namespace TicTacToeML.Events
{
    public class MoveMadeEventArgs : EventArgs
    {
        public float[] Board { get; }
        public int Player { get; }
        public int ActionTaken { get; }

        public MoveMadeEventArgs(float[] board, int player, int actionTaken)
        {
            Board = (float[])board.Clone();
            Player = player;
            ActionTaken = actionTaken;
        }
    }
}