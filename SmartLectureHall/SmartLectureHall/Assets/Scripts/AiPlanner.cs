using CymaticLabs.Unity3D.Amqp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;

public class AiPlanner : MonoBehaviour
{
    
    // Actuators
    public Window window;
    public Window window2;
    public AirConditioning airCon;
    public AirConditioning airCon2;
    public AirConditioning airCon3;
    public AirConditioning airCon4;
    public GameObject seat_rows;

    // Target values
    public float wantedTemperature = 20.0f; // centigrade
    public float wantedHumidity = 45.0f; // humidity in % at the desired temperature
    public float wantedCO2 = 3.5f; // == 3.5 % CO2

    // Tolerances
    public float temperatureTolerance = 0.5f; // in °C 
    public float humidityTolerance = 2.5f; // 0.25%
    public float CO2Tolerance = 0.1f; // = 0.1% //0-6% ok, 8% ohnmacht, 12% tot

    public float aiUpdateRate = 60.0f;
    private float aiUpdateTimer = 0.0f;
    //private bool isWindowOpen = false;

    // One entry for each sensor family
    enum SensorFamalies
    {
        Temperature,
        Humidity,
        CO2,
        SensorFamaliesCOUNT
    }

    enum AirQualityActions 
    { 
        dontOpenWindow, // ==0 =>default
        openWindow,
        activateAirCon,
        noMatter,
        AirQualityActionsCOUNT
    }
    private AirQualityActions[] aqActions;

    // Seating control and displays
    public GameObject infoDisplay;
    public SeatDisplay display;
    private int[,] currentSeatingPlan;
    private readonly Chair[,] chairs = new Chair[19, 14];
    private readonly Led[,] displayLEDs = new Led[19, 14];

    // Seating plans
    [Header("exam, pandemic")]
    [Header("smallClass, mcTest")]
    [Header("standard, frontHalf")]
    [Header("Available seating plans:")]
    public string seatingPlan = "standard";

    private bool[,] occupancie = new bool[19, 14];

    AmqpClient amqp;
    // Placeholder to quickly create new seating Plans
    /*private static readonly int[,] newSeatingPlan = new int[19, 14] {
        {0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0},
        {0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0},
        {0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0},
        {0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0},
        {1, 1, 0, 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0},
        {1, 1, 0, 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0},
        {0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 0},
        {0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0},
        {0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0},
        {0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0},
        {0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0},
        {1, 1, 0, 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0},
        {1, 1, 0, 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0},
        {0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 0},
        {0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
    };*/

    // Data Processing
    ArrayList sensorFamilies = new ArrayList();
    Dictionary<string, ArrayList> sensorValues = new Dictionary<string, ArrayList>();
    Dictionary<int, Sensor> sensors = new Dictionary<int, Sensor>();
    //float temperature;
    //float humidity;
    //float CO2;

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

        amqp.ConnectToHost();
        // Uncomment to create new seating plan
        // Serialize(newSeatingPlan, "Assets/SeatingPlans/conway.sp");
        var ledRows = display.gameObject.transform.Find("LEDs");
        var seatRows = seat_rows.gameObject.transform;

        // Build arrays to access chairs and seating display
        for (int i = 0; i < 19; i++)
        {
            var seatRow = seatRows.GetChild(i);
            var ledRow = ledRows.GetChild(i);

            for (int j = 0; j < 14; j++)
            {
                chairs[i, j] = seatRow.gameObject.transform.GetChild(j).GetComponent<Chair>();
                displayLEDs[i, j] = ledRow.gameObject.transform.GetChild(j).GetComponent<Led>();
                occupancie[i, j] = false;
            }
        }
        // Wait for objects to be loaded before loading first seating plan
        StartCoroutine(WaitForLoading());

        
        

    }

    IEnumerator WaitForLoading()
    {
        yield return new WaitForSeconds(10);
        LoadSeatingPlan(seatingPlan);
    }

    void Update()
    {
        
        
            UpdateInfoDisplay();
            aiUpdateTimer += Time.deltaTime;

            // Start AI planner update
            if (aiUpdateTimer > aiUpdateRate)
            {
                AirQualityControl();
                ApplySeatingPlan();
                UpdateSeatingDisplay();

                aiUpdateTimer = 0;
            }

            // Handle user input for seating plan changes
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                LoadSeatingPlan("standard");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                LoadSeatingPlan("frontHalf");
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                LoadSeatingPlan("smallClass");
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                LoadSeatingPlan("mcTest");
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                LoadSeatingPlan("exam");
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                LoadSeatingPlan("pandemic");
            }   
    }

    // Get values from broker to update information display
    void UpdateInfoDisplay()
    {
        infoDisplay.transform.GetChild(0).Find("tempIn").GetComponent<TextMeshProUGUI>().text = Math.Round(GetTemperatureInside(),1).ToString("#0.0");
        infoDisplay.transform.GetChild(0).Find("tempOut").GetComponent<TextMeshProUGUI>().text = Math.Round(GetTemperatureOutside(), 1).ToString("#0.0");

        infoDisplay.transform.GetChild(0).Find("humidIn").GetComponent<TextMeshProUGUI>().text = Math.Round(GetHumidityInside(), 1).ToString("#0.0");
        infoDisplay.transform.GetChild(0).Find("humidOut").GetComponent<TextMeshProUGUI>().text = Math.Round(GetHumidityOutside(), 1).ToString("#0.0");

        infoDisplay.transform.GetChild(0).Find("CO2In").GetComponent<TextMeshProUGUI>().text = Math.Round(GetCO2Inside(), 1).ToString("#0.0");
        infoDisplay.transform.GetChild(0).Find("CO2Out").GetComponent<TextMeshProUGUI>().text = Math.Round(GetCO2Outside(), 1).ToString("#0.0");
    }

    // Load selected seating plan, apply, update display
    void LoadSeatingPlan(string name)
    {
        currentSeatingPlan = (int[,])Deserialize("Assets/SeatingPlans/" + name + ".sp");
        ApplySeatingPlan();
        UpdateSeatingDisplay();
    }

    // Update seating display according to seating plan and students
    void UpdateSeatingDisplay()
    {
        
        print("Update display");
        occupancie = GetSeatOccupancy();
        for (int i = 0; i < 19; i++)
        {
            for (int j = 0; j < 14; j++)
            {
                // Check for each chair if it is locked or occupied
                if (chairs[i, j].isLocked || occupancie[i, j])
                {
                    displayLEDs[i, j].ChangeColor("red");
                }
                else
                {
                    displayLEDs[i, j].ChangeColor("green");
                }
            }
        }

    }

    // Reset all chairs and lock according to seating plan
    void ApplySeatingPlan()
    {
        print("Apply seating plan");
        // Reset
        for (int i = 0; i < 19; i++)
        {
            for (int j = 0; j < 14; j++)
            {
                chairs[i, j].UnlockChair();
            }
        }

        // Set
        for (int i = 0; i < 19; i++)
        {
            for (int j = 0; j < 14; j++)
            {
                if (currentSeatingPlan[i, j] == 0)
                {
                    chairs[i, j].LockChair();
                }
            }
        }
    }

    // Helper to save seating plan to disk
    public static void Serialize(object t, string path)
    {
        using (Stream stream = File.Open(path, FileMode.Create))
        {
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(stream, t);
        }
    }

    // Helper to load seating plan from disk
    public static object Deserialize(string path)
    {
        using (Stream stream = File.Open(path, FileMode.Open))
        {
            BinaryFormatter bformatter = new BinaryFormatter();
            return bformatter.Deserialize(stream);
        }
    }

    void OpenWindows()
    {
        print("Windows opened");
        window.Open();
        window2.Open();
        //isWindowOpen = true;
    }

    void CloseWindows()
    {
        print("Windows closed");
        window.Close();
        window2.Close();
        //isWindowOpen = false;
    }

    void ActivateAirCondition()
    {
        print("Air conditioning activated: ");
        airCon.TurnOn(wantedTemperature, wantedHumidity, wantedCO2);
        airCon2.TurnOn(wantedTemperature, wantedHumidity, wantedCO2);
        airCon3.TurnOn(wantedTemperature, wantedHumidity, wantedCO2);
        airCon4.TurnOn(wantedTemperature, wantedHumidity, wantedCO2);
    }

    void DeactivateAirCondition()
    {
        print("Air conditioning deactivated");
        airCon.TurnOff();
        airCon2.TurnOff();
        airCon3.TurnOff();
        airCon4.TurnOff();
    }

    void AirQualityControl()
    {
        //Init
        this.aqActions = new AirQualityActions[(int)SensorFamalies.SensorFamaliesCOUNT]; // Set all entries to default

        //Check enviromental
        TemperatureControl(); //check temperature
        HumidityControl(); //check humidity
        CO2Control(); //check carbon dioxide 



        bool AcPulsAny = this.aqActions[(int)SensorFamalies.Temperature].Equals(AirQualityActions.activateAirCon) || this.aqActions[(int)SensorFamalies.Humidity].Equals(AirQualityActions.activateAirCon) || this.aqActions[(int)SensorFamalies.CO2].Equals(AirQualityActions.activateAirCon);

        // Combination of "openWindow" and "dontOpenWindow" -> AC
        bool openWindowPlusDontOpenWindow = (this.aqActions[(int)SensorFamalies.Temperature].Equals(AirQualityActions.openWindow) && (this.aqActions[(int)SensorFamalies.Humidity].Equals(AirQualityActions.dontOpenWindow) || this.aqActions[(int)SensorFamalies.CO2].Equals(AirQualityActions.dontOpenWindow))) 
            || (this.aqActions[(int)SensorFamalies.Humidity].Equals(AirQualityActions.openWindow) && (this.aqActions[(int)SensorFamalies.Temperature].Equals(AirQualityActions.dontOpenWindow) || this.aqActions[(int)SensorFamalies.CO2].Equals(AirQualityActions.dontOpenWindow))) 
            || (this.aqActions[(int)SensorFamalies.CO2].Equals(AirQualityActions.openWindow) && (this.aqActions[(int)SensorFamalies.Humidity].Equals(AirQualityActions.dontOpenWindow) || this.aqActions[(int)SensorFamalies.Temperature].Equals(AirQualityActions.dontOpenWindow)));

        if (AcPulsAny || openWindowPlusDontOpenWindow)
        {// (AC + any) or (openWindow + dontOpenWindow) -> AC
            CloseWindows();
            ActivateAirCondition();
            return;
        }

        // Combination of "noMatter" and "dontOpenWindow" -> do nothing or close all
        bool noMatterOrDoNothing = (this.aqActions[(int)SensorFamalies.Temperature].Equals(AirQualityActions.noMatter) || this.aqActions[(int)SensorFamalies.Temperature].Equals(AirQualityActions.dontOpenWindow))
            && (this.aqActions[(int)SensorFamalies.Humidity].Equals(AirQualityActions.noMatter) || this.aqActions[(int)SensorFamalies.Humidity].Equals(AirQualityActions.dontOpenWindow))
            && (this.aqActions[(int)SensorFamalies.CO2].Equals(AirQualityActions.noMatter) || this.aqActions[(int)SensorFamalies.CO2].Equals(AirQualityActions.dontOpenWindow));

        if (noMatterOrDoNothing)
        {//  only "noMatter" or "dontOpenWindow" for all values -> close all
            CloseWindows();
            DeactivateAirCondition();
            return;
        }

        // only "openWindow" or "noMatter"
        bool windowPlusNoMatter = (this.aqActions[(int)SensorFamalies.Temperature].Equals(AirQualityActions.openWindow) || this.aqActions[(int)SensorFamalies.Temperature].Equals(AirQualityActions.noMatter))
            && (this.aqActions[(int)SensorFamalies.Humidity].Equals(AirQualityActions.openWindow) || this.aqActions[(int)SensorFamalies.Humidity].Equals(AirQualityActions.noMatter))
            && (this.aqActions[(int)SensorFamalies.CO2].Equals(AirQualityActions.openWindow) || this.aqActions[(int)SensorFamalies.CO2].Equals(AirQualityActions.noMatter));

        if (windowPlusNoMatter)
        {// open Window
            OpenWindows();
            DeactivateAirCondition();
            return;
        }

        // do nothing
        CloseWindows();
        DeactivateAirCondition();
    }

    //Checks if given value is in temperature range
    // Two temperature values are equals if they are in Range to each other
    bool IsTemp1InRangeOfTemp2(float temp1, float temp2)
    {
        float minTemp = temp2 - temperatureTolerance;
        float maxTemp = temp2 + temperatureTolerance;

        return (temp1 >= minTemp && temp1 <= maxTemp);
    }

    void TemperatureControl()
    {

        float Temp_IN = GetTemperatureInside();
        float Temp_OUT = GetTemperatureOutside();

        bool InOutEquals = IsTemp1InRangeOfTemp2(Temp_IN, Temp_OUT);
        bool InWantedEquals = IsTemp1InRangeOfTemp2(Temp_IN, wantedTemp);
        bool OutWantedEquals = IsTemp1InRangeOfTemp2(Temp_OUT, wantedTemp);

        if (!InOutEquals && !InWantedEquals && !OutWantedEquals)
        {//worse OR openWin

            bool tooHotOutside = wantedTemp > Temp_IN && Temp_IN > Temp_OUT;
            bool tooColdOutside = Temp_OUT > Temp_IN && Temp_IN > wantedTemp;

            if (tooHotOutside || tooColdOutside)
            {//outside value make it worse. -> dont open Window
                this.aqActions[(int)SensorFamalies.Temperature] = AirQualityActions.dontOpenWindow;
                print("Temp:  Value is in Range but outside value make it worse");
            }
            else
            {//open window neessary
                this.aqActions[(int)SensorFamalies.Temperature] = AirQualityActions.openWindow;
                print("Temp: Window flag set");
            }
            return;
        }

        if ((!InOutEquals && !InWantedEquals && OutWantedEquals) && (InOutEquals && !InWantedEquals && !OutWantedEquals))
        {//AC
            this.aqActions[(int)SensorFamalies.Temperature] = AirQualityActions.activateAirCon;
            print("Temp: activate AirCondition");
            return;
        }

        if (!InOutEquals && InWantedEquals && !OutWantedEquals)
        {//dontOpenWin
            this.aqActions[(int)SensorFamalies.Temperature] = AirQualityActions.dontOpenWindow;
            print("Temp:  Value is in Range but outside value make it worse");
            return;
        }

        if (InOutEquals && InWantedEquals && OutWantedEquals)
        {//doNothing
            this.aqActions[(int)SensorFamalies.Temperature] = AirQualityActions.dontOpenWindow;
            print("Temp:  Value is in Range but outside value make it worse");
            return;
        }
    }

    //Checks if given value is in humidity range
    // Two Humidity values are equals if they are in Range to each other
    bool IsHum1InRangeOfHum2(float hum1, float hum2)
    {
        float minHum = hum2 - humidityTolerance;
        float maxHum = hum2 + humidityTolerance;

        return (hum1 >= minHum && hum1 <= maxHum);
    }

    void HumidityControl()
    {
        float Humidity_IN = GetHumidityInside();
        float Humidity_OUT = GetHumidityOutside();

        bool InOutEquals = IsHum1InRangeOfHum2(Humidity_IN, Humidity_OUT);
        bool InWantedEquals = IsHum1InRangeOfHum2(Humidity_IN, wantedHumidity);
        bool OutWantedEquals = IsHum1InRangeOfHum2(Humidity_OUT, wantedHumidity);

        if (!InOutEquals && !InWantedEquals && !OutWantedEquals) 
        {//worse OR openWin

            bool tooDryAirOutside = wantedHumidity > Humidity_IN && Humidity_IN > Humidity_OUT;
            bool tooHazyAirOutside = Humidity_OUT > Humidity_IN && Humidity_IN > wantedHumidity;

            if (tooDryAirOutside || tooHazyAirOutside)
            {//outside value make it worse. -> dont open Window
                this.aqActions[(int)SensorFamalies.Humidity] = AirQualityActions.dontOpenWindow;
                print("Humidity:  Value is in Range but outside value make it worse");
            }
            else
            {//open window neessary
                this.aqActions[(int)SensorFamalies.Humidity] = AirQualityActions.openWindow;
                print("Humidity: Window flag set");
            }
            return;
        }

        if ((!InOutEquals && !InWantedEquals && OutWantedEquals) && (InOutEquals && !InWantedEquals && !OutWantedEquals) )
        {//AC
            this.aqActions[(int)SensorFamalies.Humidity] = AirQualityActions.activateAirCon;
            print("Humidity: activate AirCondition");
            return;
        }

        if (!InOutEquals && InWantedEquals && !OutWantedEquals)
        {//dontOpenWin
            this.aqActions[(int)SensorFamalies.Humidity] = AirQualityActions.dontOpenWindow;
            print("Humidity:  Value is in Range but outside value make it worse");
            return;
        }

        if (InOutEquals && InWantedEquals && OutWantedEquals)
        {//doNothing
            this.aqActions[(int)SensorFamalies.Humidity] = AirQualityActions.dontOpenWindow;
            print("Humidity:  Value is in Range but outside value make it worse");
            return;
        }
    }

    //Checks if given value is in CO2 range
    // Two CO2 values are equals if they are in Range to each other
    bool IsCO2_1InRangeOfCO2_2(float CO2_1, float CO2_2)
    {
        float minCO2 = CO2_2 - CO2Tolerance;
        float maxCO2 = CO2_2 + CO2Tolerance;

        return (CO2_1 >= minCO2 && CO2_1 <= maxCO2);
    }

    void CO2Control()
    {
        float CO2_IN = GetCO2Inside();
        float CO2_OUT = GetCO2Outside();

        bool InOutEquals = IsCO2_1InRangeOfCO2_2(CO2y_IN, CO2_OUT);
        bool InWantedEquals = IsCO2_1InRangeOfCO2_2(CO2y_IN, wantedCO2);
        bool OutWantedEquals = IsCO2_1InRangeOfCO2_2(CO2_OUT, wantedCO2);

        if (!InOutEquals && !InWantedEquals && !OutWantedEquals)
        {//worse OR openWin

            bool tooStuffyAir = wantedHumidity > Humidity_IN && Humidity_IN > Humidity_OUT;
            bool tooCleanAir = Humidity_OUT > Humidity_IN && Humidity_IN > wantedHumidity;

            if (tooStuffyAir || tooCleanAir)
            {//outside value make it worse. -> dont open Window
                this.aqActions[(int)SensorFamalies.CO2] = AirQualityActions.dontOpenWindow;
                print("CO2:  Value is in Range but outside value make it worse");
            }
            else
            {//open window neessary
                this.aqActions[(int)SensorFamalies.CO2] = AirQualityActions.openWindow;
                print("Humidity: Window flag set");
            }
            return;
        }

        if ((!InOutEquals && !InWantedEquals && OutWantedEquals) && (InOutEquals && !InWantedEquals && !OutWantedEquals))
        {//AC
            this.aqActions[(int)SensorFamalies.CO2] = AirQualityActions.activateAirCon;
            print("CO2: activate AirCondition");
            return;
        }

        if (!InOutEquals && InWantedEquals && !OutWantedEquals)
        {//dontOpenWin
            this.aqActions[(int)SensorFamalies.Co2] = AirQualityActions.dontOpenWindow;
            print("CO2:  Value is in Range but outside value make it worse");
            return;
        }

        if (InOutEquals && InWantedEquals && OutWantedEquals)
        {//doNothing
            this.aqActions[(int)SensorFamalies.CO2] = AirQualityActions.dontOpenWindow;
            print("CO2:  Value is in Range but outside value make it worse");
            return;
        }
    }

    #region Event Handlers

    // Handles a connection event
    void HandleConnected(AmqpClient client)
    {
        var subscription = new UnityAmqpExchangeSubscription("sensorData", AmqpExchangeTypes.Direct, "", null, handleIncomingMessage);
        amqp.SubscribeToExchange(subscription);

        subscription = new UnityAmqpExchangeSubscription("chairs", AmqpExchangeTypes.Direct, "", null, handleIncomingMessageChairs);
        amqp.SubscribeToExchange(subscription);
    }

    void handleIncomingMessage(AmqpExchangeSubscription subscription, IAmqpReceivedMessage message)
    {
        SensorData data = JsonUtility.FromJson<SensorData>(ByteArrayToString(message.Body));
        if(data.type == "add")
        {
            AddSensor(data.family, data.id, data.value);
        } else
        {
            Sensor sensor = sensors[data.id];
            sensorValues[sensor.sensorFamily][sensor.sensorSlot] = data.value;
        }
    }

    void handleIncomingMessageChairs(AmqpExchangeSubscription subscription, IAmqpReceivedMessage message)
    {
        SensorData data = JsonUtility.FromJson<SensorData>(ByteArrayToString(message.Body));
        int row = int.Parse(data.position.Split('-')[0]);
        int number = int.Parse(data.position.Split('-')[1]);
        occupancie[row - 1, number - 1] = data.occupancie;
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


    #region Utilities
    private string ByteArrayToString(byte[] arr)
    {
        System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
        return enc.GetString(arr);
    }
    #endregion

    #region data processing

   

    public void AddSensor(string sensorFamily, int sensorId, float sensorData)
    {
        if (!sensorFamilies.Contains(sensorFamily))
        {
            sensorFamilies.Add(sensorFamily);
            sensorValues.Add(sensorFamily, new ArrayList());
        }
        if (!sensors.ContainsKey(sensorId))
        {
            int sensorSlot = sensorValues[sensorFamily].Add(sensorData);
            Sensor sensor = new Sensor(sensorId, sensorFamily, sensorSlot);
            sensors.Add(sensor.id, sensor);
        }
        
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
        return occupancie;
    }

    #endregion
}
