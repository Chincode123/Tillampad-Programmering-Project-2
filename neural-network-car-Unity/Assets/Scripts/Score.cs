using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Klass f�r att h�ll koll p� hur l�ngt en bil har k�rt och hur l�ng tid den har k�rt
 */
[Serializable]
public class Score : MonoBehaviour
{
    [SerializeField, HideInInspector] GameObject[] checkpoints;
    int currentCheckpointIndex = 0; // indexen av checkpointet som bilen ska k�ra till
    Checkpoint currentCheckpoint; // class-obectet av checkpointet som bilen ska k�ra till. g�r koden mer "clean" genom att ha det som en separat variable/atribut; annars hade koden p� rad 83 beh�vts skrivas m�nga fler g�nger

    float distancePassed, distanceFromPassedCheckpoints, distancePassedToNextCheckpoint;
    // Jag m�tte inte bara ut st�rckan som bilen k�rde f�r att undvika att den skulle f� h�ga po�ng av att bara k�ra runt i cirklar
    // distancePassed: den totala passerade str�ckan
    // distanceFromPassedCheckpoints: den sammanlagda str�ckan fr�n distance atributen i checkpointsen som bilen har passerad
    // distancePassedToNextCheckpoint: raka str�ckan till n�sta checkpoint
    float timeSinceStart;

    bool crashed = true;

    bool finished = false;

    [SerializeField, HideInInspector] float distanceScoreMultiplyer, crashPenalty;
    // scoreDistanceMultiplyer: multiplicerar po�ngen get fr�n distance s� att, att k�ra l�ngt lite l�ngsammare v�ger mer �n att k�ra en kort distance snabbt

    public void Initialize(float scoreFromDistanceMultiplyer, float crashScorePenalty)
    {
        distanceScoreMultiplyer = scoreFromDistanceMultiplyer;
        crashPenalty = crashScorePenalty;

        StartScore();
    }

    /*
     * Startar ig�ng po�ngr�kningen
     * 
     * Separat fr�n initsialiserings funktionen f�r att den inte anger n�gra externa v�rden
     * 
     * Funktionen hade anv�nts om samma bil flytades till en annan bana
     */
    public void StartScore()
    {
        checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        checkpoints = SortCheckpoints(checkpoints);

        currentCheckpoint = checkpoints[0].GetComponent<Checkpoint>();

        timeSinceStart = 0;

        crashed = false;

        distancePassed = 0;
        distanceFromPassedCheckpoints = 0;
    }

    private void Update()
    {
        if (crashed) return;

        timeSinceStart += Time.deltaTime;

        distancePassedToNextCheckpoint = currentCheckpoint.GetDistance() - (transform.position - currentCheckpoint.transform.position).magnitude;
        distancePassed = distanceFromPassedCheckpoints + distancePassedToNextCheckpoint;
    }

    public void PassedCheckpoint(int checkpointIndex)
    {
        if (checkpointIndex != currentCheckpointIndex || crashed) return;

        if (checkpointIndex == checkpoints.Length - 1)
        {
            finished = true;
            crashed = true;
            return;
        }

        distanceFromPassedCheckpoints += currentCheckpoint.GetDistance();

        currentCheckpointIndex++;
        currentCheckpoint = checkpoints[currentCheckpointIndex].GetComponent<Checkpoint>();
    }

    public float GetScore()
    {
        float finishBonus = 0;
        if (finished)
            finishBonus = 10000000000; // l�gger till ett stort tal s� att de som klarar hela banan f�r h�ga po�ng
        else if (crashed)
            return (distancePassed * distanceScoreMultiplyer / timeSinceStart) - (crashPenalty / timeSinceStart); // delar po�ng straffet p� tiden s� den blir mindre viktigare senare (g�r nog ingen stor sklidnad)
        return (distancePassed * distanceScoreMultiplyer / timeSinceStart) + finishBonus;
    }

    /*
     * sorterar checkpoint objecten efter deras index
     * tidskomplexitet �r inte s� viktigt f�r att det kommer inte vara s� m�nga object att sortera
     */
    GameObject[] SortCheckpoints(GameObject[] checkpoints)
    {
        GameObject[] sorted = new GameObject[checkpoints.Length];

        foreach (GameObject checkpoint in checkpoints)
        {
            int checkpointIndex = checkpoint.GetComponent<Checkpoint>().index;
            sorted[checkpointIndex] = checkpoint;
        }

        return sorted;
    }

    public void Crash()
    {
        crashed = true;
    }
}
