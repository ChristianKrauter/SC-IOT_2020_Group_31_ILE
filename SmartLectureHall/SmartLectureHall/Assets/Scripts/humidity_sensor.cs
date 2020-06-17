﻿using UnityEngine;

public class Humidity_sensor : MonoBehaviour
{
    public Broker broker;
    public GlobalVariables environment;
    public bool inside;
    public string sensorType;
    public string sensorFamily;
    public float timer = 0.0f;
    public float sensorValue = 0.0f;
    int sensorId;
    // Start is called before the first frame update
    void Start()
    {
        if (inside)
        {
            sensorFamily = sensorType + "_inside";
        }
        else
        {
            sensorFamily = sensorType + "_outside";
        }
        sensorId = this.GetHashCode();
        SensorUpdate();
        broker.AddSensor(sensorFamily, sensorId, sensorValue);

    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 5.0f)
        {
            timer = 0.0f;
            SensorUpdate();
            SendData();

        }
    }

    void SensorUpdate()
    {
        if (inside)
        {
            sensorValue = environment.inner_humidity + Random.Range(-2.0f, 2.0f);
        }
        else
        {
            sensorValue = environment.outer_humidity + Random.Range(-2.0f, 2.0f);
        }
    }

    void SendData()
    {
        broker.SendData(sensorId, sensorValue);
    }
}
