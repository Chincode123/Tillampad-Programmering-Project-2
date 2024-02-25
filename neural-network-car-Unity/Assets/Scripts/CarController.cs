using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CarController : MonoBehaviour
{

    [SerializeField] bool control = false; // g�r att man kan kontrollera bilen sj�lv

    [SerializeField] float acceleration = .4f, maxSpeed = 20, rotationSpeed = 180, deceleration = .05f, maxRotationRadius = 5;
    // rotationSpeed: grader / sekund
    // deceleration: andel av hastigheten
    // resten av v�rderna �r i enheter eller enheter / sekund

    public float velocity = 0;

    private float currentAcceleration = 0;

    [SerializeField] Gradient rayColorGradient; // f�rg-gradient f�r uppvisningen av bilens synlinjer

    [SerializeField] Transform front; // framsidan av bilen

    public void Initialize(CarControllerArgs args)
    {
        acceleration = args.acceleration;
        maxSpeed = args.maxSpeed;
        rotationSpeed = args.rotationSpeed;
        deceleration = args.deceleration;
        maxRotationRadius = args.maxRotationRadius;
    }

    void Update()
    {
        if (control)
        {
            if (Input.GetKey(KeyCode.W))
                Accelerate();
            else if (Input.GetKey(KeyCode.S))
                Break();
            if (Input.GetKey(KeyCode.A))
                Turn(-1);
            else if (Input.GetKey(KeyCode.D))
                Turn(1);

            if (Input.GetKeyDown(KeyCode.Space))
                velocity = 0;
        }

        // hade kunat vara b�tre eftersom att den kommer att g� fram och tillbaka med att vara under och �ver max farten n�r accelerationen blir 0
        if (velocity >= maxSpeed)
        {
            velocity = maxSpeed;
            currentAcceleration = 0;
        }
        // s = v * t + a * t^2 / 2
        transform.Translate(transform.forward * (velocity * Time.deltaTime + (currentAcceleration * Mathf.Pow(Time.deltaTime, 2) / 2)), 0);
        velocity += (currentAcceleration - deceleration * velocity) * Time.deltaTime;
        if (velocity < 0.01) // stannar bilen om den �r v�ldigt l�ngsam
            velocity = 0;

        currentAcceleration = 0;
    }

    public void Accelerate()
    {
        currentAcceleration = acceleration;
    }

    public void Break()
    {
        // g�r att den bromsar l�ngsammare �n hur den accelererar
        currentAcceleration = acceleration / -2;
    }

    public void Turn(int direction)
    {
        float rotation;
        // kollar om sv�ngen kommer att bli f�r stor. Jag satte in det h�r s� att bilarna �ndo kan ta skarpa sv�ngar n�r de k�r snabbt
        if (velocity / (Mathf.Deg2Rad * rotationSpeed) > maxRotationRadius) // h�rledd fr�n cirkelsektor
            rotation = (velocity / maxRotationRadius) * Mathf.Rad2Deg * direction * Time.deltaTime;
        else
            rotation = direction * rotationSpeed * Time.deltaTime;
        
        // roterar kring y-ledet
        transform.Rotate(new Vector3(0, rotation), Space.Self);
    }

    /*
     *  returnerar en array med str�ckorna till v�ggarna
     * 
     *  parametrar:
     *      rays: antalet syn-str�lar
     *      minAngle: vinkeln av den f�rsta syn-str�len. vinkeln f�rh�ller sig s� att rakt framf�r bilen �r 0 grader
     *      maxAngle: vinkeln av den sista syn-str�len
     *      viewDistance: max syn str�ckan
     */
    public double[] GetDistances(int rays, float minAngle, float maxAngle, float viewDistance)
    {
        double[] distances = new double[rays];
        float distanceBetweenRays = (maxAngle - minAngle) / (rays - 1);
        // (maxAngle - minAngle): hela omf�nget av str�larna
        // (rays - 1): antal sektioner mellan str�larna
        float carAngle = Mathf.Atan2(transform.forward.z, transform.forward.x);

        for (int i = 0; i < rays; i++)
        {
            Vector3 direction = new Vector3(Mathf.Cos(carAngle + (minAngle + (distanceBetweenRays * i)) * Mathf.Deg2Rad), 0, Mathf.Sin(carAngle + (minAngle + (distanceBetweenRays * i)) * Mathf.Deg2Rad)).normalized;

            RaycastHit hit; // hit data
            if (Physics.Raycast(front.position, direction, out hit, viewDistance, LayerMask.GetMask("Wall")))
            {
                distances[i] = (front.position - hit.point).magnitude;
            }
            else
                distances[i] = viewDistance; // om str�len inte n�dde n�gon v�g s� anges str�ckan som max-str�ckan

            // utritning av str�lar
            Color rayColor = rayColorGradient.Evaluate((float)((viewDistance - distances[i]) / viewDistance));
            Debug.DrawLine(front.position, front.position + direction * (float)distances[i], rayColor);
        }
        return distances;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Wall")
            GetComponent<Car>().Crash();
    }
}
