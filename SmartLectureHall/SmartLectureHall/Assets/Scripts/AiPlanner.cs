﻿using System;
using System.Collections;
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

    public Broker broker;
    private bool[,] occupancie;

    // Placeholder to quickly create new seating Plans
    /*private static readonly int[,] newSeatingPlan = new int[19, 14] {
        {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
    };*/

    // Start is called before the first frame update
    void Start()
    {
        // Uncomment to create new seating plan
        // Serialize(newSeatingPlan, "Assets/SeatingPlans/smallclass.sp");
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
            }
        }
        // Wait for objects to be loaded before loading first seating plan
        StartCoroutine(WaitForLoading());
    }

    IEnumerator WaitForLoading()
    {
        yield return 10;
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
        infoDisplay.transform.GetChild(0).Find("tempIn").GetComponent<TextMeshProUGUI>().text = Math.Round(broker.GetTemperatureInside(),1).ToString("#0.0");
        infoDisplay.transform.GetChild(0).Find("tempOut").GetComponent<TextMeshProUGUI>().text = Math.Round(broker.GetTemperatureOutside(), 1).ToString("#0.0");

        infoDisplay.transform.GetChild(0).Find("humidIn").GetComponent<TextMeshProUGUI>().text = Math.Round(broker.GetHumidityInside(), 1).ToString("#0.0");
        infoDisplay.transform.GetChild(0).Find("humidOut").GetComponent<TextMeshProUGUI>().text = Math.Round(broker.GetHumidityOutside(), 1).ToString("#0.0");

        infoDisplay.transform.GetChild(0).Find("CO2In").GetComponent<TextMeshProUGUI>().text = Math.Round(broker.GetCO2Inside(), 1).ToString("#0.0");
        infoDisplay.transform.GetChild(0).Find("CO2Out").GetComponent<TextMeshProUGUI>().text = Math.Round(broker.GetCO2Outside(), 1).ToString("#0.0");
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
        occupancie = broker.GetSeatOccupancy();
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

        float Temp_IN = broker.GetTemperatureInside();
        float Temp_OUT = broker.GetTemperatureOutside();

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
        float Humidity_IN = broker.GetHumidityInside();
        float Humidity_OUT = broker.GetHumidityOutside();

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
        float CO2_IN = broker.GetCO2Inside();
        float CO2_OUT = broker.GetCO2Outside();

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
}
