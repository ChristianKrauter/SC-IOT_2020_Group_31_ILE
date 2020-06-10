using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class humidity_sensor : MonoBehaviour
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
        } else
        {
            sensorFamily = sensorType + "_outside";
        }
        sensorId =  this.GetHashCode();
        sensorUpdate();
        broker.addSensor(sensorFamily,sensorId,sensorValue);
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > 5.0f)
        {
            timer = 0.0f;
            sensorUpdate();
            sendData();

        }
    }

    void sensorUpdate()
    {
        if (inside)
        {
            sensorValue = environment.inner_humidity + Random.Range(-2.0f,2.0f);
        }
        else
        {
            sensorValue = environment.outer_humidity + Random.Range(-2.0f, 2.0f);
        }
    }

    void sendData()
    {
        broker.sendData(sensorId, sensorValue);
    }
}
