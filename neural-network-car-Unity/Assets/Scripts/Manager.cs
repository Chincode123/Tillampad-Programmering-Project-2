using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{

    private float timeScale = 1;

    // prefab f�r bil-objectet
    [SerializeField] GameObject car;

    // track-genererings variabler; anv�nds inte
    GenerateTrack trackGenerator;
    [SerializeField] float trackLength, trackIncrementSize, trackWidth, trackCheckpointDensity, turnRange, minTurnDistance, maxTurnDistance;
    [SerializeField] int turns;


    [SerializeField] Transform spawnObject; // bilar skapas som ett "barn" till objectet vid dess position och i riktingen den pekar
    [SerializeField] int populationSize;
    [SerializeField] float eliteCutOf, childToMutationRatio; 
    // eliteCutOf: Hur stor andel av populationen som g�r vidare utan att f�r�ndras
    // childToMutationRatio: hur stor andel av resten av populationen som kommer att antingen bli ett barn eller muteras
    // 1: alla blir barn; 0: alla blir muterade
    [SerializeField] int[] networkSize; // storleken av n�tverket; hur m�nga noder det �r vid varje lager
    [SerializeField] CarControllerArgs carControllerValues;
    [SerializeField] int viewLines; // Hur m�nga syn intag bilarna har
    [SerializeField] float viewAngle, maxViewDistance, scoreDistanceMultiplyer, crashPenalty;
    // viewAngle: Vinkeln som bilen kan se fr�n mitten av dess vy i grader
    // scoreDistanceMultiplyer: multiplicerar po�ngen get fr�n distance s� att, att k�ra l�ngt lite l�ngsammare v�ger mer �n att k�ra en kort distance snabbt
    [SerializeField] int rngSeed;

    Learning learning;

    private void Start()
    {
        trackGenerator = GetComponent<GenerateTrack>();
        learning = GetComponent<Learning>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            // g�r att tiden g�r dubbelt s� snabbt
            timeScale = timeScale * 2;
            SetTime(timeScale);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            // g�r att tiden g�r h�lften s� snabbt
            timeScale = timeScale / 2;
            SetTime(timeScale);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            learning.Inizialize(spawnObject, populationSize, eliteCutOf, childToMutationRatio, networkSize, car, carControllerValues, viewLines, viewAngle, maxViewDistance, scoreDistanceMultiplyer, crashPenalty, rngSeed);
        }
    }

    void SetTime(float timeScale)
    {
        Time.timeScale = this.timeScale;
        Debug.Log("Time Set to: " + this.timeScale);
    }
}
