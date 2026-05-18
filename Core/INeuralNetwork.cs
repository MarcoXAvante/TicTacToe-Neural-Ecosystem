namespace TicTacToeML.Core
{
    public interface INeuralNetwork
    {
        float[] Forward(float[] inputs);
        void Backpropagate(float[] expectedOutputs, float learningRate);
    }
}