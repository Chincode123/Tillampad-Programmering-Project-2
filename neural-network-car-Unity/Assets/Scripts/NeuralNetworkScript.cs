﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
class NetworkLayer : ISerializationCallbackReceiver
{
    [HideInInspector] public int inputNodesNum, outputNodesNum;
    double[,] weights; // en vikt för varje nod koppling
    [SerializeField] double[] biases; // en bias/activationsgränns för varje nod

    // Kod för att serializea multidimensionella arrayer tagit härifrån:
    // https://forum.unity.com/threads/how-i-serialize-multidimensional-arrays.988704/

    [SerializeField, HideInInspector] private List<Package<double>> serializableWeigths;

    [Serializable]
    struct Package<TElement>
    {
        public int Index0;
        public int Index1;
        public TElement Element;
        public Package(int idx0, int idx1, TElement element)
        {
            Index0 = idx0;
            Index1 = idx1;
            Element = element;
        }
    }

    public void OnBeforeSerialize()
    {
        serializableWeigths = new List<Package<double>>();
        for (int i = 0; i < weights.GetLength(0); i++)
        {
            for (int j = 0; j < weights.GetLength(1); j++)
            {
                serializableWeigths.Add(new Package<double>(i, j, weights[i, j]));
            }
        }
    }

    public void OnAfterDeserialize()
    {
        weights = new double[outputNodesNum , inputNodesNum];
        foreach (var package in serializableWeigths)
        {
            weights[package.Index0, package.Index1] = package.Element;
        }
    }

    // NetworkLayer constructor
    public NetworkLayer(int inputNodes, int outputNodes)
    {
        inputNodesNum = inputNodes;
        outputNodesNum = outputNodes;

        weights = new double[outputNodes, inputNodes];
        biases = new double[outputNodes];
    }

    // Constructor för första lagret som inte har några inputnoder
    public NetworkLayer(int outputNodes)
    {
        inputNodesNum = 1;
        outputNodesNum = outputNodes;

        weights = new double[outputNodes, 1];
        for (int i = 0; i < outputNodes; i++)
            weights[i, 0] = 1;
        biases = new double[outputNodes];
    }


    public double[] CalculateOutputs(double[] input)
    {

        double[] output = new double[outputNodesNum];

        // går igenom varje nod i lagret
        for (int i = 0; i < outputNodesNum; i++)
        {
            // går igenom varje input till noden
            for (int j = 0; j < inputNodesNum; j++)
            {
                output[i] += input[j] * weights[i, j];
            }
            output[i] += biases[i];
            output[i] = Activation(output[i]);
        }

        return output;
    }

    double Activation(double input)
    {
        return 1 / (1 + System.Math.Exp(-input)); // sigmoid funktion
    }

    public void SetWeights(int node, double[] values)
    {
        // går igenom varje koppling till noden
        for (int i = 0; i < values.Length; i++)
            weights[node, i] = values[i];
    }
    public void ShiftWeights(int node, double[] values)
    {
        for (int i = 0; i < values.Length; i++)
            weights[node, i] += values[i];
    }
    public double[,] GetWeights()
    {
        return weights;
    }

    public void SetBiases(double[] values)
    {
        biases = values;
    }
    public void ShiftBiases(double[] values)
    {
        // går igenom varje nod
        for (int i = 0; i < biases.Length; i++)
            biases[i] += values[i];
    }

    public double[] GetBiases()
    {
        return biases;
    }

}

[Serializable]
class NeuralNetwork
{
    [SerializeField] NetworkLayer[] layers;

    public double[] GetOutputs(double[] input)
    {
        foreach (NetworkLayer layer in layers)
            input = layer.CalculateOutputs(input);

        return input;
    }

    public int CalculateOutputNode(double[] outputValues)
    {
        int outputNode = 0;
        for (int i = 0; i < outputValues.Length; i++)
            if (outputValues[i] > outputValues[outputNode])
                outputNode = i;

        return outputNode;
    }


    // nodes: hur många noder som finns på varje lager
    public NeuralNetwork(int[] nodes, System.Random rng)
    {
        NetworkLayer[] newLayers = new NetworkLayer[nodes.Length]; // nodes.Lenght: antal lager
        for (int i = 0; i < nodes.Length; i++)
        {
            // första lagret har ingen inputs
            if (i == 0)
            {
                newLayers[0] = new NetworkLayer(nodes[i]);
                continue;
            }

            newLayers[i] = new NetworkLayer(nodes[i - 1], nodes[i]); // input noderna är samma som noderna från det förra lagret
            double[] biases = new double[nodes[i]];
            // anger slumpade värden till koplingarna och noderna
            for (int j = 0; j < nodes[i]; j++)
            {
                biases[j] = rng.NextDouble();

                double[] weigths = new double[nodes[i - 1]];
                for (int k = 0; k < nodes[i - 1]; k++)
                {
                    weigths[k] = rng.NextDouble();
                }
                newLayers[i].SetWeights(j, weigths);
            }
            newLayers[i].SetBiases(biases);
        }

        layers = newLayers;
    }

    public NetworkLayer[] GetLayers()
    {
        return layers;
    }

    public void SetLayers(NetworkLayer[] newLayers)
    {
        layers = newLayers;
    }
}