﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CO2_sensor : MonoBehaviour
{
    public Broker broker;
    public Environment env;
    public bool inside;
    public string sensorType;
    public string sensorFamily;
    public float timer = 0.0f;
    public float sensorValue = 0.0f;
    int sensorId;

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
            sensorValue = env.inner_co2 + Random.Range(-0.5f, 0.5f);
        }
        else
        {
            sensorValue = env.outer_co2 + Random.Range(-0.5f, 0.5f);
        }
    }

    void SendData()
    {
        broker.SendData(sensorId, sensorValue);
    }
}
