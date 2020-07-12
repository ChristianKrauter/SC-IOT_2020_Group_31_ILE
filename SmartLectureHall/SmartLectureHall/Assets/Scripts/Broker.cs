using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Broker : MonoBehaviour
{
    //TODO Do everything
    public float timer = 0.0f;
    ArrayList sensorFamilies = new ArrayList();
    Dictionary<string, ArrayList> sensorValues = new Dictionary<string, ArrayList>();
    Dictionary<string, Sensor> sensors = new Dictionary<string, Sensor>();
    float temperature;
    float humidity;
    float CO2;

    private bool[,] chairs = new bool[19, 14];

    public void AddSensor(string sensorFamily, int sensorId, float sensorData)
    {
        if (!sensorFamilies.Contains(sensorFamily))
        {
            sensorFamilies.Add(sensorFamily);
            sensorValues.Add(sensorFamily, new ArrayList());
        }
        int sensorSlot = sensorValues[sensorFamily].Add(sensorData);
        Sensor sensor = new Sensor(sensorId, sensorFamily, sensorSlot);
        sensors.Add(sensor.id.ToString(), sensor);
    }

    public void SendData(int _sensorId, float data)
    {
        Sensor sensor = sensors[_sensorId.ToString()];
        sensorValues[sensor.sensorFamily][sensor.sensorSlot] = data;
    }

    public void SendData(string sensorId, bool occupancie)
    {
        int row = int.Parse(sensorId.Split('-')[0]);
        int number = int.Parse(sensorId.Split('-')[1]);
        chairs[row - 1, number - 1] = occupancie;
    }

    class Sensor
    {
        public int id;
        public string sensorFamily;
        public int sensorSlot;

        public Sensor(int _id, string _sensorFamily, int _sensorSlot)
        {
            id = _id;
            sensorFamily = _sensorFamily;
            sensorSlot = _sensorSlot;
        }
    }

    class Chair
    {
        public string id;
        public int row;
        public int number;

        public Chair(string _id, int _row, int _number)
        {
            id = _id;
            row = _row;
            number = _number;
        }
    }

    public float GetHumidityInside()
    {
        if (sensorFamilies.Contains("humidity_inside"))
        {
            float value = 0;
            for (int i = 0; i < sensorValues["humidity_inside"].Count; i++)
            {
                value += Convert.ToSingle(sensorValues["humidity_inside"][i]);
            }
            return value / sensorValues["humidity_inside"].Count;
        }
        return -267.0f;
    }

    public float GetHumidityOutside()
    {
        if (sensorFamilies.Contains("humidity_outside"))
        {
            float value = 0;
            for (int i = 0; i < sensorValues["humidity_outside"].Count; i++)
            {
                value += Convert.ToSingle(sensorValues["humidity_outside"][i]);
            }
            return value / sensorValues["humidity_outside"].Count;
        }
        return -267.0f;
    }

    public float GetTemperatureInside()
    {
        if (sensorFamilies.Contains("temperature_inside"))
        {
            float value = 0;
            for (int i = 0; i < sensorValues["temperature_inside"].Count; i++)
            {
                value += Convert.ToSingle(sensorValues["temperature_inside"][i]);
            }
            return value / sensorValues["temperature_inside"].Count;
        }
        return -267.0f;
    }

    public float GetTemperatureOutside()
    {
        if (sensorFamilies.Contains("temperature_outside"))
        {
            float value = 0;
            for (int i = 0; i < sensorValues["temperature_outside"].Count; i++)
            {
                value += Convert.ToSingle(sensorValues["temperature_outside"][i]);
            }
            return value / sensorValues["temperature_outside"].Count;
        }
        return -267.0f;
    }

    public float GetCO2Inside()
    {
        if (sensorFamilies.Contains("CO2_inside"))
        {
            float value = 0;
            for (int i = 0; i < sensorValues["CO2_inside"].Count; i++)
            {
                value += Convert.ToSingle(sensorValues["CO2_inside"][i]);
            }
            return value / sensorValues["CO2_inside"].Count;
        }
        return -267.0f;
    }

    public float GetCO2Outside()
    {
        if (sensorFamilies.Contains("CO2_outside"))
        {
            float value = 0;
            for (int i = 0; i < sensorValues["CO2_outside"].Count; i++)
            {
                value += Convert.ToSingle(sensorValues["CO2_outside"][i]);
            }
            return value / sensorValues["CO2_outside"].Count;
        }

        return -267.0f;
    }

    public bool[,] GetSeatOccupancy()
    {
        return chairs;
    }
}
