using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;

public class AiPlanner : MonoBehaviour
{
    public Window window;
    public Window window2;
    public AirConditioning airCon;
    public AirConditioning airCon2;
    public AirConditioning airCon3;
    public AirConditioning airCon4;

    public float wantedTemperature = 20f; // centigrade
    public float wantedHumidity = 45f; // humidity in % at the desired temperature
    public float wantedCO2 = 0.1f; // == 0.1 % CO2

    public float refreshTimer = 60.0f;
    private float timer = 0.0f;

    public GameObject seat_rows;
    private bool isWindowOpen = false;

    // One entry for each Sensorfamalie
    private bool[] activateAirConditionFlag;
    private bool[] openWindowFlag;

    public float temperatureTolerance = 0.5f; // in °C 
    public float humidityTolerance = 0.0025f; // 0.25%
    public float CO2Tolerance = 0.001f; // = 0.1% //0-6% ok, 8% ohnmacht, 12% tot

    private int[,] currentSeatingPlan;
    private readonly Chair[,] chairs = new Chair[19, 14];

    public SeatDisplay display;
    private readonly Led[,] displayLEDs = new Led[19, 14];

    public GameObject infoDisplay;

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
        //Serialize(newSeatingPlan, "Assets/SeatingPlans/smallclass.sp");
        var ledRows = display.gameObject.transform.Find("LEDs");
        var seatRows = seat_rows.gameObject.transform;

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
        // currentSeatingPlan = (int[,])Deserialize("Assets/SeatingPlans/" + seatingPlan + ".sp");
        StartCoroutine(WaitForLoading());
    }

    IEnumerator WaitForLoading()
    {
        yield return 10;
        LoadSeatingPlan(seatingPlan);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInfoDisplay();
        timer += Time.deltaTime;
        if (timer > refreshTimer)
        {
            AirQualityControl();
            ApplySeatingPlan();
            UpdateSeatingDisplay();

            timer = 0;
        }

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

    void UpdateInfoDisplay()
    {
        infoDisplay.transform.GetChild(0).Find("tempIn").GetComponent<TextMeshProUGUI>().text = Math.Round(broker.GetTemperatureInside(),1).ToString("#0.0");
        infoDisplay.transform.GetChild(0).Find("tempOut").GetComponent<TextMeshProUGUI>().text = Math.Round(broker.GetTemperatureOutside(), 1).ToString("#0.0");

        infoDisplay.transform.GetChild(0).Find("humidIn").GetComponent<TextMeshProUGUI>().text = Math.Round(broker.GetHumidityInside(), 1).ToString("#0.0");
        infoDisplay.transform.GetChild(0).Find("humidOut").GetComponent<TextMeshProUGUI>().text = Math.Round(broker.GetHumidityOutside(), 1).ToString("#0.0");

        infoDisplay.transform.GetChild(0).Find("CO2In").GetComponent<TextMeshProUGUI>().text = Math.Round(broker.GetCO2Inside(), 1).ToString("#0.0");
        infoDisplay.transform.GetChild(0).Find("CO2Out").GetComponent<TextMeshProUGUI>().text = Math.Round(broker.GetCO2Outside(), 1).ToString("#0.0");
    }

    void LoadSeatingPlan(string name)
    {
        currentSeatingPlan = (int[,])Deserialize("Assets/SeatingPlans/" + name + ".sp");
        ApplySeatingPlan();
        UpdateSeatingDisplay();
    }

    void UpdateSeatingDisplay()
    {
        print("Update display");
        occupancie = broker.GetSeatOccupancy();
        for (int i = 0; i < 19; i++)
        {
            for (int j = 0; j < 14; j++)
            {
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

    public static void Serialize(object t, string path)
    {
        using (Stream stream = File.Open(path, FileMode.Create))
        {
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(stream, t);
        }
    }

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
        isWindowOpen = true;
    }

    void CloseWindows()
    {
        print("Windows closed");
        window.Close();
        window2.Close();
        isWindowOpen = false;
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
        openWindowFlag = new bool[3]; //Sett all entries to false
        activateAirConditionFlag = new bool[3]; //Sett all entries to false

        //Check enviromental
        TemperatureControl(); //check temperature
        HumidityControl(); //check humidity
        CO2Control(); //check carbon dioxide 
        
        if (openWindowFlag[0] && openWindowFlag[1] && openWindowFlag[2])
        {
            if (activateAirConditionFlag[0] && activateAirConditionFlag[1] && activateAirConditionFlag[2])
            {// Control released for all Value
                CloseWindows();
                DeactivateAirCondition();
            }
            else
            {// Window open wanted for at least one value and the rest released control
                OpenWindows();
                DeactivateAirCondition();
            }        
        }
        else
        {// window closed for at least one value
            CloseWindows();
        }

        if (activateAirConditionFlag[0] && activateAirConditionFlag[1] && activateAirConditionFlag[2])
        {
            if (!openWindowFlag[0] || !openWindowFlag[1] || !openWindowFlag[2])
            {// Window close wanted for at least one value and the rest released control
                CloseWindows();
                ActivateAirCondition();
            }//else-block not nessesary; same as first inner block 
     
        }
        else
        {// deaktivate air condition for all values
            DeactivateAirCondition();
        }

    }

    //Checks if given value is in temparature range
    bool InWantedTemperatureRange(float currentTemp)
    {
        float minTemp = wantedTemperature - temperatureTolerance;
        float maxTemp = wantedTemperature + temperatureTolerance;

        if (currentTemp >= minTemp && currentTemp <= maxTemp)
        {
            return true;
        }
        return false;
    }

    void TemperatureControl()
    {

        float Temp_IN = broker.GetTemperatureInside();
        float Temp_OUT = broker.GetTemperatureOutside();

        //Check if opening the window makes the temperature worse
        if ((wantedTemperature > Temp_IN && Temp_IN > Temp_OUT) /*too cold outside*/ || (Temp_OUT > Temp_IN && Temp_IN > wantedTemperature) /*too hot outside*/ )
        {
            //outside values too bad

            this.activateAirConditionFlag[0] = true;
            print("Temp: Air conditioning flag set");
        }
        else
        {   //external values good enough

            if (InWantedTemperatureRange(Temp_IN))
            {// Value is in Range but window open is not nesessary. Release window for the othe values
                this.activateAirConditionFlag[0] = true;
                this.openWindowFlag[0] = true;
                print("Temp: Window released for other Values");
            }
            else
            {
                this.openWindowFlag[0] = true;
                print("Temp: Window flag set");
            }
        }
    }

    //Checks if given value is in humidity range
    bool InWantedHumidityRange(float currentHum)
    {
        float minHum = wantedHumidity - humidityTolerance;
        float maxHum = wantedHumidity + humidityTolerance;

        if (currentHum >= minHum && currentHum <= maxHum)
        {
            return true;
        }
        return false;
    }

    void HumidityControl()
    {
        float Humidity_IN = broker.GetHumidityInside();
        float Humidity_OUT = broker.GetHumidityOutside();

        //Check if opening the window makes the humidity worse
        if ((wantedHumidity > Humidity_IN && Humidity_IN > Humidity_OUT) /*too dry air outside*/ || (Humidity_OUT > Humidity_IN && Humidity_IN > wantedHumidity) /*too hazy outside*/)
        {
            //outside values too bad   

            this.activateAirConditionFlag[1] = true;
            print("Humidity: Air conditioning flag set");
        }
        else
        {   //external values good enough

            if (InWantedHumidityRange(Humidity_IN))
            {// Value is in Range but window open is not nesessary. Release window for the othe values
                this.activateAirConditionFlag[1] = true;
                this.openWindowFlag[1] = true;
                print("Humidity: Window released for other Values");
            }
            else
            {
                this.openWindowFlag[1] = true;
                print("Humidity: Window flag set");
            }
        }
    }

    //Checks if given value is in CO2 range
    bool InWantedCO2Range(float currentCO2)
    {
        float minCO2 = wantedCO2 - CO2Tolerance;
        float maxCO2 = wantedCO2 + CO2Tolerance;

        if (currentCO2 >= minCO2 && currentCO2 <= maxCO2)
        {
            return true;
        }
        return false;
    }

    void CO2Control()
    {
        float CO2_IN = broker.GetCO2Inside();
        float CO2_OUT = broker.GetCO2Outside();

        //Check if opening the window makes the CO2 worse
        if ((wantedCO2 > CO2_IN && CO2_IN > CO2_OUT) /*not stuffy enough air outside*/ || (CO2_OUT > CO2_IN && CO2_IN > wantedCO2) /*too stuffy air outside*/)
        {
            //outside values too bad

            this.activateAirConditionFlag[2] = true;
            print("CO2: Air conditioning flag set");
        }
        if (InWantedCO2Range(CO2_IN))
        {// Value is in Range but window open is not nesessary. Release window for the othe values
            this.activateAirConditionFlag[2] = true;
            this.openWindowFlag[2] = true;
            print("CO2: Window released for other Values");
        }
        else
        {
            this.openWindowFlag[2] = true;
            print("CO2: Window flag set");
        }
    }




}
