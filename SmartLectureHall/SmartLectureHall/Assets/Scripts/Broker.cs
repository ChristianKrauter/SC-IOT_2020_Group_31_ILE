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
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 10.0f)
        {
            timer = 0;
            foreach (string key in sensorValues.Keys)
            {
                float sum = 0.0f;
                foreach (float f in sensorValues[key])
                {
                    sum += f;
                }
                Debug.Log("Family: " + key + " / Avg: " + (sum / sensorValues[key].Count));
            }

        }

    }

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
}
