using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Learning : MonoBehaviour
{
    GeneticLearning population;
    bool inizialized = false;

    public void Inizialize(Transform spawnTransform, int populationSize, float elitCutOf, float childToMutationRatio, int[] networkSize, GameObject carObject, CarControllerArgs carControllerValues, int viewLines, float viewAngle, float maxViewDistance, float scoreDistanceMultiplyer, float crashPenalty, int rngSeed)
    {
        inizialized = true;
        population = new GeneticLearning(spawnTransform, populationSize, elitCutOf, childToMutationRatio, networkSize, carObject, carControllerValues, viewLines, viewAngle, maxViewDistance, scoreDistanceMultiplyer, crashPenalty, rngSeed);
    }

    void Update()
    {
        if (!inizialized) return;

        bool stillGoing = false; // kollar om det finns bilar som inte har krachat

        // g�r igenom alla bilar
        foreach (Car car in population.population)
        {
            if (car.crashed) continue;

            car.Drive();

            stillGoing = true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            stillGoing = false;
        }

        if (stillGoing) return;


        population.Evolve();
    }
}


class GeneticLearning
{
    float eliteCutOf, childToMutationRatio;
    public Car[] population;
    int populationSize;
    System.Random rng;

    Transform spawnTransform;
    GameObject carObject;

    // Constructor
    public GeneticLearning(Transform spawnTransform, int populationSize, float eliteCutOf, float childToMutationRatio, int[] networkSize, GameObject carObject, CarControllerArgs carControllerValues, int viewLines, float viewAngle, float maxViewDistance, float scoreDistanceMultiplyer, float crashPenalty, int rngSeed)
    {

        this.eliteCutOf = eliteCutOf;
        this.childToMutationRatio = childToMutationRatio;

        this.populationSize = populationSize;
        population = new Car[populationSize];
        for (int i = 0; i < populationSize; i++)
        {
            population[i] = Object.Instantiate(carObject, spawnTransform).GetComponent<Car>(); // skapar bil-objectet
            population[i].Initialize(i, networkSize, carControllerValues, viewLines, viewAngle, maxViewDistance, scoreDistanceMultiplyer, crashPenalty, rngSeed + i * i + 1);
        }

        rng = new System.Random(rngSeed * rngSeed);

        this.spawnTransform = spawnTransform;
        this.carObject = carObject;
    }

    // struct som ineh�ller en bil class och bilens po�ng
    public struct CarScoreStruct
    {
        public Car car;
        public float score;
    }

    /*
     * bubble sort f�r bilarnas po�ng
     * 
     * tidskomplexitet blir inte ett problem f�r att det kan inte finnas s� m�nga bilar
     * 
     * https://www.w3resource.com/csharp-exercises/searching-and-sorting-algorithm/searching-and-sorting-algorithm-exercise-3.php
     */
    void SortCarsByScore(CarScoreStruct[] values)
    {
        for (int i = 0; i <= values.Length - 2; i++) // Outer loop for passes
        {
            for (int j = 0; j <= values.Length - 2; j++) // Inner loop for comparison and swapping
            {
                if (values[j].score > values[j + 1].score) // Checking if the current element is greater than the next element
                {
                    var temp = values[j + 1]; // Swapping elements if they are in the wrong order
                    values[j + 1] = values[j];
                    values[j] = temp;
                }
            }
        }
    }

    Car[] GetScores()
    {
        CarScoreStruct[] scoreCar = new CarScoreStruct[populationSize]; // array som inneh�ller bilar och deras po�ng. kunde inte komma p� n�got b�ttre namn
        // ange v�rden till alla element
        for (int i = 0; i < populationSize; i++)
        {
            scoreCar[i].car = population[i];
            scoreCar[i].score = population[i].GetScore();
        }
        // sortera
        SortCarsByScore(scoreCar);

        Car[] sortedCars = new Car[populationSize];
        for (int i = 0; i < populationSize; i++)
        {
            // elementen l�ggs in backl�nges s� att den 0:te bilen har h�gst po�ng och den sista bilen har l�gst
            sortedCars[i] = scoreCar[populationSize - 1 - i].car; // 
        }

        return sortedCars;
    }

    public void Evolve()
    {
        Car[] scoredCars = GetScores();
        Car[] newPopulation = new Car[populationSize];
        // g�r igenom och anger v�rden till den nya populationen
        for (int i = 0; i < populationSize; i++)
        {
            // g�r vidare o�ndrade
            if (i < populationSize * eliteCutOf)
            {
                newPopulation[i] = scoredCars[i].Copy(Object.Instantiate(carObject, spawnTransform)); // skapar en ny copia
                newPopulation[i].SetIndex(i);
                continue;
            }
            // skapar "barn"
            if (i < (populationSize * eliteCutOf) + ((populationSize - (populationSize * eliteCutOf)) * childToMutationRatio))
            {
                // anger "f�r�ldrar"
                Car[] parrents = new Car[] { scoredCars[RandomFromPopulation()], scoredCars[RandomFromPopulation()] };
                Car child = parrents[0].Copy(Object.Instantiate(carObject, spawnTransform)); // g�r "barnet" till en kopia av en av "f�r�ldrarna"
                NetworkLayer[] childLayers = new NetworkLayer[child.network.GetLayers().Length];
                // g�r igenom alla lager i n�tverket
                for (int j = 0; j < childLayers.Length; j++)
                {
                    // tar fram v�rden fr�n "f�r�ldrarna"
                    double[] biasesParent1 = parrents[0].network.GetLayers()[j].GetBiases(),
                             biasesParent2 = parrents[1].network.GetLayers()[j].GetBiases();
                    double[,] weightsParent1 = parrents[0].network.GetLayers()[j].GetWeights(),
                              weightsParent2 = parrents[1].network.GetLayers()[j].GetWeights();

                    double[] newBiases = biasesParent1;
                    // skapar n�tverks lagret
                    childLayers[j] = new NetworkLayer(weightsParent1.GetLength(1), biasesParent1.Length);
                    //g�r igenom alla noder
                    for (int k = 0; k < weightsParent1.GetLength(0); k++)
                    {
                        if (rng.NextDouble() >= 0.5) // slumpar mellan att ta v�rdet fr�n "f�r�lder" 1 eller 2
                            newBiases[k] = biasesParent2[k];

                        double[] newWeights = new double[weightsParent1.GetLength(1)];
                        // g�r igenom alla kopplingar till noden
                        for (int l = 0; l < newWeights.Length; l++)
                        {
                            if (rng.NextDouble() < 0.5) // slumpar mellan att ta v�rdet fr�n "f�r�lder" 1 eller 2
                                newWeights[l] = weightsParent1[k, l];
                            else
                                newWeights[l] = weightsParent2[k, l];
                        }
                        // anger de nya vikterna till noden
                        childLayers[j].SetWeights(k, newWeights);
                    }
                    // anger de nya aktiveringstalen till noderna i lagret
                    childLayers[j].SetBiases(newBiases);
                }
                // anger lagerna till n�tverket
                child.network.SetLayers(childLayers);
                newPopulation[i] = child;
                newPopulation[i].SetIndex(i);
                continue;
            }
            // skapar muterade bilar/n�tverk 
            int random = RandomFromPopulation();
            Car mutated = scoredCars[random].Copy(Object.Instantiate(carObject, spawnTransform)); // skapar en kopia av en slumpad bil
            // g�r igenom alla lagerna
            foreach (NetworkLayer layer in mutated.network.GetLayers())
            {
                double[] biasShift = new double[layer.outputNodesNum];
                // g�r igenom noderna
                for (int j = 0; j < layer.outputNodesNum; j++)
                {
                    biasShift[j] = (rng.NextDouble() * 2) - 1; // anger ett slumpat tal mellan -1 och 1

                    double[] weightShift = new double[layer.inputNodesNum];
                    // g�r igenom kopplingaran till noderna
                    for (int k = 0; k < layer.inputNodesNum; k++)
                    {
                        weightShift[k] = (rng.NextDouble() * 2) - 1;// anger ett slumpat tal mellan -1 och 1
                    }
                    layer.ShiftWeights(j, weightShift);
                }
                layer.ShiftBiases(biasShift);
            }
            newPopulation[i] = mutated;
            newPopulation[i].SetIndex(i);
        }

        // tar bort den gamla populationen
        DestroyCars();

        population = new Car[populationSize];
        for (int i = 0; i < populationSize; i++)
        {
            population[i] = newPopulation[i];
            population[i].crashed = false;
        }
    }

    // en slumpad bil fr�n populationen med h�gre sannolikhet att f� h�grepresterande bilar �n l�gre
    int RandomFromPopulation()
    {
        return (int)((1 - System.Math.Pow(rng.NextDouble(), 2)) * populationSize);
    }

    // tar bort alla bilar i populationen
    void DestroyCars()
    {
        foreach (Car car in population)
        {
            Object.Destroy(car.gameObject);
        }
    }
}