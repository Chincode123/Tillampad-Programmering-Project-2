using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] public int index;
    [SerializeField] float distance; // str�ckan fr�n det f�reg�ende checkpointet

    /*
     *  Ger spel objectet v�rden f�r n�r den skapas med kod
     * 
     *  parametrar:
     *      width: bredden p� banan
     *      distance: str�ckan fr�n det f�reg�ende checkpointet
     *      index: index
     */
    public void Initialize(float width, float distance, int index)
    {
        this.index = index;
        gameObject.name = "Checkpoint: " + index.ToString();
        this.distance = distance;
        transform.localScale = new Vector3(1 / 2, 1, width);
    }

    /*  Activeras n�r en collider kolliderar med objectet
     *  parameter:
     *      other: collidern p� objectet som kolliderade med objectet
    */
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Car")
            other.GetComponent<Score>().PassedCheckpoint(index);
    }

    public float GetDistance()
    {
        return distance;
    }
}
