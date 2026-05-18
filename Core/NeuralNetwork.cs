using System;
using TicTacToeML.Core;

namespace TicTacToeML.Core
{
    public class NeuralNetwork : INeuralNetwork
    {
        private readonly int[] _layers;
        private readonly float[][] _neurons;
        private readonly float[][] _biases;
        private readonly float[][][] _weights;
        private readonly Random _random;

        public NeuralNetwork(int[] layers)
        {
            _layers = layers;
            _random = new Random();

            _neurons = InitializeJaggedArray(_layers.Length, i => new float[_layers[i]]);
            _biases = InitializeBiases();
            _weights = InitializeWeights();
        }

        public float[] Forward(float[] inputs)
        {
            Array.Copy(inputs, _neurons[0], inputs.Length);

            for (int i = 1; i < _layers.Length; i++)
            {
                for (int j = 0; j < _layers[i]; j++)
                {
                    float value = _biases[i][j];
                    for (int k = 0; k < _layers[i - 1]; k++)
                    {
                        value += _weights[i - 1][j][k] * _neurons[i - 1][k];
                    }

                    if (i == _layers.Length - 1)
                        _neurons[i][j] = value;
                    else
                        _neurons[i][j] = Tanh(value);
                }
            }
            return _neurons[^1];
        }

        public void Backpropagate(float[] expected, float learningRate)
        {
            float[][] errors = InitializeJaggedArray(_layers.Length, i => new float[_layers[i]]);
            int outLayer = _layers.Length - 1;

            for (int i = 0; i < _layers[outLayer]; i++)
            {
                float output = _neurons[outLayer][i];
                errors[outLayer][i] = (expected[i] - output) * 1.0f;
            }

            for (int i = outLayer - 1; i > 0; i--)
            {
                for (int j = 0; j < _layers[i]; j++)
                {
                    float error = 0;
                    for (int k = 0; k < _layers[i + 1]; k++)
                    {
                        error += errors[i + 1][k] * _weights[i][k][j];
                    }
                    errors[i][j] = error * TanhDerivative(_neurons[i][j]);
                }
            }

            UpdateWeightsAndBiases(errors, learningRate);
        }

        private void UpdateWeightsAndBiases(float[][] errors, float learningRate)
        {
            for (int i = 0; i < _weights.Length; i++)
            {
                for (int j = 0; j < _weights[i].Length; j++)
                {
                    float biasError = Math.Clamp(errors[i + 1][j], -1f, 1f);
                    _biases[i + 1][j] += biasError * learningRate;

                    for (int k = 0; k < _weights[i][j].Length; k++)
                    {
                        float weightError = Math.Clamp(errors[i + 1][j] * _neurons[i][k], -1f, 1f);
                        _weights[i][j][k] += weightError * learningRate;
                    }
                }
            }
        }

        private float[][] InitializeBiases()
        {
            return InitializeJaggedArray(_layers.Length, i =>
            {
                if (i == 0) return Array.Empty<float>();
                var layerBiases = new float[_layers[i]];
                for (int j = 0; j < layerBiases.Length; j++) layerBiases[j] = RandomFloat() * 0.1f;
                return layerBiases;
            });
        }

        private float[][][] InitializeWeights()
        {
            var weights = new float[_layers.Length - 1][][];
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = new float[_layers[i + 1]][];
                for (int j = 0; j < weights[i].Length; j++)
                {
                    weights[i][j] = new float[_layers[i]];
                    for (int k = 0; k < _layers[i]; k++) weights[i][j][k] = RandomFloat() * 0.1f;
                }
            }
            return weights;
        }

        private T[] InitializeJaggedArray<T>(int length, Func<int, T> initializer)
        {
            var array = new T[length];
            for (int i = 0; i < length; i++) array[i] = initializer(i);
            return array;
        }

        private float RandomFloat() => (float)(_random.NextDouble() * 2.0 - 1.0);

        private float Tanh(float x) => (float)Math.Tanh(x);
        private float TanhDerivative(float x) => 1f - (x * x);
    }
}