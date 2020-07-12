using CymaticLabs.Unity3D.Amqp;
using UnityEngine;

public class ChairRowController : MonoBehaviour
{
    AmqpClient amqp;
    // Start is called before the first frame update
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

        amqp.ConnectToHost();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void sendData(SensorData data)
    {
        if (amqp.IsConnected)
        {
            string json = JsonUtility.ToJson(data);
            amqp.PublishToExchange("chairs", "", json);
        }

    }

    #region Event Handlers

    // Handles a connection event
    void HandleConnected(AmqpClient client)
    {

    }


    // Handles a disconnection event
    void HandleDisconnected(AmqpClient client)
    {
        //Debug.Log("Disconnected");
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
