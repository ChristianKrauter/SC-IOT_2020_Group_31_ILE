using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public GameObject chair_rows;
    public float refreshTime = 5.0f;
    private float timer = 0.0f;
    private readonly Chair[,] chairs = new Chair[19,14];

    public void Start()
    {
        for (int i = 0; i < 19; i++)
        {
            var row = chair_rows.gameObject.transform.GetChild(i);
            for (int j = 0; j < 14; j++)
            {
                chairs[i,j] = row.gameObject.transform.GetChild(j).GetComponent<Chair>();
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
        }
    }

    private void DistributeStudents()
    {
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
