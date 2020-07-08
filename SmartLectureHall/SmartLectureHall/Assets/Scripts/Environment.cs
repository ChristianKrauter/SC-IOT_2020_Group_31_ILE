using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public float outer_temperature = 25.0f;
    public float inner_temperature = 23.0f;

    public float outer_humidity = 20.0f;
    public float inner_humidity = 30.0f;

    public float outer_co2 = 5.0f;
    public float inner_co2 = 6.0f;

    // max 266
    [Range(0,266)]
    public int numberOfStudents = 50;
    public GameObject seat_rows;
    private readonly Chair[,] chairs = new Chair[19, 14];

    private float actuatorTimer = 0.0f;
    public float actuatorRefreshRate = 1.0f;

    private float studentDistTimer = 0.0f;
    public float studentDistRefreshRate = 120.0f;

    private float studentCO2Timer = 0.0f;
    public float studentCO2UpdateRate = 3600.0f;

    private float studentTempHumidTimer = 0.0f;
    public float studentTempHumidUpdateRate = 50.0f;

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
        actuatorTimer += Time.deltaTime;
        studentDistTimer += Time.deltaTime;
        studentCO2Timer += Time.deltaTime;
        studentTempHumidTimer += Time.deltaTime;

        if (actuatorTimer > actuatorRefreshRate)
        {
            actuatorTimer = 0;
            if (ventilating)
            {
                Ventilate();
            }
            if (airConditioning)
            {
                RunAirConditioning(airConditioningTemp, airConditioningHumid , airConditioningCO2);
            }
        }

        if (studentDistTimer > studentDistRefreshRate)
        {
            studentDistTimer = 0;
            DistributeStudents();
        }

        if (studentCO2Timer > studentCO2UpdateRate)
        {
            studentCO2Timer = 0;
            StudentCO2Contribution();
        }

        if (studentTempHumidTimer > studentTempHumidUpdateRate)
        {
            studentTempHumidTimer = 0;
            StudentHeatDissipation();
            StudentHumidityContribution();
        }

    }

    private void StudentHeatDissipation()
    {
        print(string.Format("Student Heat += {0}", ((numberOfStudents * 120 * studentTempHumidUpdateRate) / 19288000)));
        inner_temperature += ((numberOfStudents * 120 * studentTempHumidUpdateRate) / 19288000);
    }

    private void StudentHumidityContribution()
    {
        print(string.Format("Student Humid += {0}", ((0.00000462963f * numberOfStudents * studentTempHumidUpdateRate) / (0.02f * 15883f)) * 100f));
        inner_humidity += ((0.00000462963f * numberOfStudents * studentTempHumidUpdateRate) / (0.02f*15883f)) * 100f;
    }

    private void StudentCO2Contribution()
    {
        print(string.Format("Student CO2 += {0}", ((0.04346925f * numberOfStudents) / 19059f) * 100f));
        inner_co2 += ((0.04346925f * numberOfStudents) / 19059f) * 100f;
    }

    private void RunAirConditioning(float goalTemp, float goalHumid, float goalCO2)
    {
        // Temperature
        if (inner_temperature < goalTemp)
        {
            inner_temperature += 0.5f;
        }
        else if (inner_temperature > goalTemp)
        {
            inner_temperature -= 0.5f;
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
        if (inner_temperature < outer_temperature)
        {
            inner_temperature += 0.1f;
        }
        else if (inner_temperature > outer_temperature)
        {
            inner_temperature -= 0.1f;
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
        if (inner_temperature < outer_co2)
        {
            inner_co2 += 0.001f;
        }
        else if (inner_temperature > outer_co2)
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
