using CymaticLabs.Unity3D.Amqp;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Temp_sensor : MonoBehaviour
{
    
    public Environment env;
    public bool inside;
    public string sensorType;
    public string sensorFamily;
    public float timer = 0.0f;
    public float sensorValue = 0.0f;
    int sensorId;

    AmqpClient amqp;

    void Start()
    {

        amqp = this.gameObject.AddComponent<AmqpClient>();

        amqp.OnConnected = new AmqpClientUnityEvent();
        amqp.OnDisconnected = new AmqpClientUnityEvent();
        amqp.OnReconnecting = new AmqpClientUnityEvent();
        amqp.OnBlocked = new AmqpClientUnityEvent();
        amqp.OnSubscribedToExchange = new AmqpExchangeSubscriptionUnityEvent();
        amqp.OnUnsubscribedFromExchange = new AmqpExchangeSubscriptionUnityEvent();

        amqp.OnConnected.AddListener(HandleConnected);
        amqp.OnDisconnected.AddListener(HandleDisconnected);
        amqp.OnReconnecting.AddListener(HandleReconnecting);
        amqp.OnBlocked.AddListener(HandleBlocked);
        amqp.OnSubscribedToExchange.AddListener(HandleExchangeSubscribed);
        amqp.OnUnsubscribedFromExchange.AddListener(HandleExchangeUnsubscribed);
        amqp.Connection = "localhost";
        amqp.WriteToConsole = false;

        if (inside)
        {
            sensorFamily = sensorType + "_inside";
        }
        else
        {
            sensorFamily = sensorType + "_outside";
        }
        sensorId = this.GetHashCode();
        amqp.ConnectToHost();
        SensorUpdate();
    }

    void SensorUpdate()
    {

        if (inside)
        {
            sensorValue = env.inner_temperature + Random.Range(-0.1f, 0.1f);
        }
        else
        {
            sensorValue = env.outer_temperature + Random.Range(-0.1f, 0.1f);
        }

        if (amqp.IsConnected)
        {
            SensorData data = new SensorData();
            data.id = sensorId;
            data.family = sensorFamily;
            data.value = sensorValue;
            data.type = "temp";

            string json = JsonUtility.ToJson(data);
            amqp.PublishToExchange("sensorData", "", json);
        }
    }


    #region Event Handlers

    // Handles a connection event
    void HandleConnected(AmqpClient client)
    {
        Debug.Log("Connected temp: " + sensorId);
        SensorData data = new SensorData();
        data.id = sensorId;
        data.family = sensorFamily;
        data.value = sensorValue;
        data.type = "add";

        string json = JsonUtility.ToJson(data);
        amqp.PublishToExchange("sensorData", "", json);
    }


    // Handles a disconnection event
    void HandleDisconnected(AmqpClient client)
    {
        Debug.Log("Disconnected");
    }

    // Handles a reconnecting event
    void HandleReconnecting(AmqpClient client)
    {

    }

    // Handles a blocked event
    void HandleBlocked(AmqpClient client)
    {

    }

    // Handles exchange subscribes
    void HandleExchangeSubscribed(AmqpExchangeSubscription subscription)
    {
        // Add it to the local list
        //exSubscriptions.Add(subscription);
    }

    // Handles exchange unsubscribes
    void HandleExchangeUnsubscribed(AmqpExchangeSubscription subscription)
    {
        // Add it to the local list
        //exSubscriptions.Remove(subscription);
    }

    #endregion Event Handlers
}
