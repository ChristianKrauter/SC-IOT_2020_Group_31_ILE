﻿using UnityEngine;

public class GlobalVariables : MonoBehaviour
{
    public float outer_temperature = 25.0f;
    public float inner_base_temperature = 23.0f;

    public float outer_humidity = 20.0f;
    public float inner_humidity = 30.0f;

    public float outer_co2 = 5.0f;
    public float inner_co2 = 10.0f;

    // max 266
    public int numberOfStudents = 50;
    public GameObject seat_rows;
    public float refreshTime = 50.0f;
    private float timer = 0.0f;
    private readonly Chair[,] chairs = new Chair[19, 14];

    // Ventilation
    public bool ventilating = false;

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
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > refreshTime)
        {
            timer = 0;
            DistributeStudents();
            if (ventilating)
            {
                Ventilate();
            }
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
            inner_humidity += 0.1f;
        }
        else if (inner_humidity > outer_humidity)
        {
            inner_humidity -= 0.1f;
        }

        // CO2
        if (inner_base_temperature < outer_co2)
        {
            inner_co2 += 0.1f;
        }
        else if (inner_base_temperature > outer_co2)
        {
            inner_co2 -= 0.1f;
        }
    }

    private void DistributeStudents()
    {
        print("Distribute Students");
        // Reset
        for (int i = 0; i < 19; i++)
        {
            for (int j = 0; j < 14; j++)
            {
                chairs[i, j].EmptyChair();
            }
        }

        // Assign randomly
        for (int i = 0; i < numberOfStudents; i++)
        {
            var rowNumber = Random.Range(0, 19);
            var chairNumber = Random.Range(0, 14);
            chairs[rowNumber, chairNumber].OccupyChair();
        }
    }
}
