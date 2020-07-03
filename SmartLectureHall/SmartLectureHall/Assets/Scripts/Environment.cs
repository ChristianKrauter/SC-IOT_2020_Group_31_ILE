using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public float outer_temperature = 25.0f;
    public float inner_base_temperature = 23.0f;

    public float outer_humidity = 20.0f;
    public float inner_humidity = 30.0f;

    public float outer_co2 = 5.0f;
    public float inner_co2 = 10.0f;

    // max 266
    [Range(0,266)]
    public int numberOfStudents = 50;
    public GameObject seat_rows;
    public float refreshTime = 50.0f;
    public float studentRefreshTime = 120.0f;
    private float timer = 0.0f;
    private float studentTimer = 0.0f;
    private readonly Chair[,] chairs = new Chair[19, 14];

    // Ventilation & airconditioning
    public bool ventilating = false;
    public bool airConditioning = false;
    public float airConditioningTemp = 0f;
    public float airConditioningHumid = 0f;
    public float airConditioningCO2 = 0f;

    public void Start()
    {
        for (int i = 0; i < 19; i++)
        {
            var row = seat_rows.gameObject.transform.GetChild(i);
            for (int j = 0; j < 14; j++)
            {
                chairs[i, j] = row.gameObject.transform.GetChild(j).GetComponent<Chair>();
            }
        }
        StartCoroutine(WaitForLoading());
    }

    IEnumerator WaitForLoading()
    {
        yield return 10;
        DistributeStudents();

    }
    private void Update()
    {
        timer += Time.deltaTime;
        studentTimer += Time.deltaTime;

        if (timer > refreshTime)
        {
            timer = 0;
            StudentHeadDissipation();
            if (ventilating)
            {
                Ventilate();
            }
            if (airConditioning)
            {
                RunAirConditioning(airConditioningTemp, airConditioningHumid , airConditioningCO2);
            }
        }

        if (studentTimer > studentRefreshTime)
        {
            studentTimer = 0;
            DistributeStudents();
        }
    }

    private void StudentHeadDissipation()
    {
        inner_base_temperature += ((numberOfStudents * 120 * refreshTime) / 19288000);
    }

    private void RunAirConditioning(float goalTemp, float goalHumid, float goalCO2)
    {
        // Temperature
        if (inner_base_temperature < goalTemp)
        {
            inner_base_temperature += 0.5f;
        }
        else if (inner_base_temperature > goalTemp)
        {
            inner_base_temperature -= 0.5f;
        }

        // Humidity
        if (inner_humidity < goalHumid)
        {
            inner_humidity += 0.002f;
        }
        else if (inner_humidity > goalHumid)
        {
            inner_humidity -= 0.002f;
        }

        // CO2
        if (inner_co2 < goalCO2)
        {
            inner_co2 += 0.001f;
        }
        else if (inner_co2 > goalCO2)
        {
            inner_co2 -= 0.001f;
        }
    }

    private void Ventilate()
    {
        // Temperature
        if (inner_base_temperature < outer_temperature)
        {
            inner_base_temperature += 0.1f;
        }
        else if (inner_base_temperature > outer_temperature)
        {
            inner_base_temperature -= 0.1f;
        }

        // Humidity
        if (inner_humidity < outer_humidity)
        {
            inner_humidity += 0.002f;
        }
        else if (inner_humidity > outer_humidity)
        {
            inner_humidity -= 0.002f;
        }

        // CO2
        if (inner_base_temperature < outer_co2)
        {
            inner_co2 += 0.001f;
        }
        else if (inner_base_temperature > outer_co2)
        {
            inner_co2 -= 0.001f;
        }
    }

    private void DistributeStudents()
    {
        List<Chair> availableChairs = new List<Chair>();
        print("Distribute Students");
        // Reset
        for (int i = 0; i < 19; i++)
        {
            for (int j = 0; j < 14; j++)
            {
                chairs[i, j].EmptyChair();
                if (!chairs[i,j].isLocked)
                {
                    availableChairs.Add(chairs[i, j]);
                }
            }
        }

        // Assign randomly
        for (int i = 0; i < numberOfStudents; i++)
        {
            if (availableChairs.Count == 0)
            {
                break;
            }
            var index = Random.Range(0, availableChairs.Count - 1);
            availableChairs[index].OccupyChair();
            availableChairs.RemoveAt(index);
        }
    }
}
