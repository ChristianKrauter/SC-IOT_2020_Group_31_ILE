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

    [Range(0,266)]
    public int numberOfStudents = 50;

    public GameObject seat_rows;
    private readonly Chair[,] chairs = new Chair[19, 14];

    // Timers and refresh times
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

    // Demo Video
    public bool customStudentDistribution;
    public bool waitForSeatingPlan;

    public void Start()
    {
        // Build arrays to access chairs
        for (int i = 0; i < 19; i++)
        {
            var row = seat_rows.gameObject.transform.GetChild(i);
            for (int j = 0; j < 14; j++)
            {
                chairs[i, j] = row.gameObject.transform.GetChild(j).GetComponent<Chair>();
            }
        }
        // Wait for objects to be loaded before distributing students the first time
        StartCoroutine(WaitForLoading());
    }

    IEnumerator WaitForLoading()
    {
        if (waitForSeatingPlan)
        {
            yield return new WaitForSeconds(10);
        }
        else
        {
            yield return new WaitForSeconds(5);
        }
        DistributeStudents();
    }

    private void Update()
    {
        actuatorTimer += Time.deltaTime;
        studentDistTimer += Time.deltaTime;
        studentCO2Timer += Time.deltaTime;
        studentTempHumidTimer += Time.deltaTime;

        // Apply effects of actuators
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

        // Re-distribute students
        if (studentDistTimer > studentDistRefreshRate)
        {
            studentDistTimer = 0;
            DistributeStudents();
        }

        // Apply temp and humid contribution of students
        if (studentTempHumidTimer > studentTempHumidUpdateRate)
        {
            studentTempHumidTimer = 0;
            StudentHeatDissipation();
            StudentHumidityContribution();
        }

        // Apply CO2 contribution of students
        if (studentCO2Timer > studentCO2UpdateRate)
        {
            studentCO2Timer = 0;
            StudentCO2Contribution();
        }
    }

    // Students increase temperature
    private void StudentHeatDissipation()
    {
        // print(string.Format("Student Heat += {0}", ((numberOfStudents * 120 * studentTempHumidUpdateRate) / 19288000)));
        inner_temperature += ((numberOfStudents * 120 * studentTempHumidUpdateRate) / 19288000);
    }

    // Students increase humidty
    private void StudentHumidityContribution()
    {
        // print(string.Format("Student Humid += {0}", ((0.00000462963f * numberOfStudents * studentTempHumidUpdateRate) / (0.02f * 15883f)) * 100f));
        inner_humidity += ((0.00000462963f * numberOfStudents * studentTempHumidUpdateRate) / (0.02f*15883f)) * 100f;
    }

    // Students increase CO2
    private void StudentCO2Contribution()
    {
        // print(string.Format("Student CO2 += {0}", ((0.04346925f * numberOfStudents) / 19059f) * 100f));
        inner_co2 += ((0.04346925f * numberOfStudents) / 19059f) * 100f;
    }

    // Effect of air conditioning
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

    // Effect of opening windows
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
    
    // Reset students and re-distribute randomly
    private void DistributeStudents()
    {
        if (!customStudentDistribution)
        {
            List<Chair> availableChairs = new List<Chair>();
            print("Distribute Students");
            // Reset
            for (int i = 0; i < 19; i++)
            {
                for (int j = 0; j < 14; j++)
                {
                    chairs[i, j].EmptyChair();
                    if (!chairs[i, j].isLocked)
                    {
                        availableChairs.Add(chairs[i, j]);
                    }
                }
            }

            print(availableChairs.Count);

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
        else
        {
            List<(int, int)> customChairSelection = new List<(int, int)>(){
                (0, 0), (0, 2), (0, 4), (0, 6), (0, 7), (0, 9), (0, 11), (0, 13),
                (1, 1), (1, 3), (1, 5), (1, 8), (1, 10), (1, 12),
                (2, 0), (2, 2), (2, 4), (2, 6), (2, 7), (2, 9), (2, 11), (2, 13),
                (3, 1), (3, 3), (3, 5), (3, 8), (3, 10), (3, 12),
                (4, 0), (4, 2), (4, 4), (4, 6), (4, 7), (4, 9), (4, 11), (4, 13),
                (5, 1), (5, 3), (5, 5), (5, 8), (5, 10), (5, 12),
                (6, 0), (6, 2), (6, 4), (6, 6), (6, 7), (6, 9), (6, 11), (6, 13),
                (7, 1), (7, 3), (7, 5), /*(7, 8),*/ (7, 10), (7, 12),
                (8, 0), (8, 2), (8, 4), (8, 6), (8, 7), (8, 9), (8, 11), (8, 13),
                (9, 1), (9, 3), (9, 5), (9, 8), (9, 10), (9, 12),
                (10, 0), (10, 2), (10, 4), (10, 6), (10, 7), (10, 9), (10, 11), (10, 13),
                (11, 1), (11, 3), (11, 5), (11, 8), (11, 10), (11, 12),
                (12, 0), (12, 2), (12, 4), (12, 6), (12, 7), (12, 9), (12, 11), (12, 13),
                (13, 1), (13, 3), (13, 5), (13, 8), (13, 10), (13, 12),
                (14, 0), (14, 2), (14, 4), (14, 6), (14, 7), (14, 9), (14, 11), (14, 13),
                (15, 1), (15, 3), (15, 5), (15, 8), (15, 10), (15, 12),
                (16, 0), (16, 2), (16, 4), (16, 6), (16, 7), (16, 9), (16, 11), (16, 13),
                (17, 1), (17, 3), (17, 5), (17, 8), (17, 10), (17, 12),
                (18, 0), (18, 2), (18, 4), (18, 6), (18, 7), (18, 9), (18, 11), (18, 13),
            };
            foreach (var index in customChairSelection)
            {
                chairs[index.Item1, index.Item2].OccupyChair();
            }
        }
    }
}
