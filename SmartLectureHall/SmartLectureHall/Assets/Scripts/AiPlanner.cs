using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
    public float wantedHumidity = 0.45f; // humidity in % at the desired temperature
    public float wantedCO2 = 0.5f; // CO2
    public float refreshTimer = 60.0f;
    private float timer = 0.0f;

    public GameObject seat_rows;
    private bool isWindowOpen = false;

    private bool avtivateAirConditionFlag =false;
    private bool openWindowFlag = true;

    public float temperatureTolerance = 0.5f;
    public float humidityTolerance = 1/4 /100; // 0.25%
    public float CO2Tolerance = 1/10/100; // = 0.1% //0-6% ok, 8% ohnmacht, 12% tot

    private int[,] currentSeatingPlan;
    private readonly Chair[,] chairs = new Chair[19, 14];
    
    public SeatDisplay display;
    private readonly Led[,] displayLEDs = new Led[19, 14];

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
        currentSeatingPlan = (int[,])Deserialize("Assets/SeatingPlans/" + seatingPlan + ".sp");
        StartCoroutine(WaitForLoading());
    }

    IEnumerator WaitForLoading()
    {
        yield return 10;
        ApplySeatingPlan();

    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > refreshTimer)
        {
            OpenWindowControl();            
            ApplySeatingPlan();
            UpdateSeatingDisplay();

            timer = 0;
        }
    }

    void UpdateSeatingDisplay()
    {
        print("Update display");
        occupancie = broker.GetSeatOccupancy();
        for (int i = 0; i < 19; i++)
        {
            for (int j = 0; j < 14; j++)
            {
                if (chairs[i,j].isLocked || !occupancie[i,j])
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
        print("Air conditioning activated: " + wantedTemperature);
        airCon.TurnOn(wantedTemperature);
        airCon2.TurnOn(wantedTemperature);
        airCon3.TurnOn(wantedTemperature);
        airCon4.TurnOn(wantedTemperature);
    }

    void DeactivateAirCondition()
    {
        print("Air conditioning deactivated");
        airCon.TurnOff();
        airCon2.TurnOff();
        airCon3.TurnOff();
        airCon4.TurnOff();
    }

    void OpenWindowControl() // überarbeiten für aircondition deaktivierung
    {        
        this.openWindowFlag = true;
        this.avtivateAirConditionFlag = false;
        OpenWindowTemperatureControl();
        OpenWindowHumidityControl();
        OpenWindowCO2Control();

        if (this.openWindowFlag)
        {
            if (isWindowOpen)
            {
                //OpenWindows(); // if it is open we dont have to open it.
                ActivateAirCondition();
            }
            else
            {
                OpenWindows();
            }
        }
        else
        {
            CloseWindows();

            if (this.avtivateAirConditionFlag)
            {
                DeactivateAirCondition();
            }
        }
        
    }
    bool InWantedTemperatureRange(float currentTemp) {
        float minTemp = wantedTemperature - temperatureTolerance;
        float maxTemp = wantedTemperature + temperatureTolerance;

        if (currentTemp >= minTemp && currentTemp <= maxTemp) {
            return true;
        }
        return false;
    }

    void OpenWindowTemperatureControl()
    {

        float Temp_IN = broker.GetTemperatureInside();
        float Temp_OUT = broker.GetTemperatureOutside();

        if (InWantedTemperatureRange(Temp_IN) && Temp_IN != Temp_OUT )
        {
            if (this.openWindowFlag)
            {
                if ( (wantedTemperature > Temp_IN && Temp_IN > Temp_OUT) /*too cold outside*/ || (Temp_OUT > Temp_IN && Temp_IN > wantedTemperature) /*too hot outside*/ )
                {
                    //deteriorating conditions
                    this.openWindowFlag = false;
                    this.avtivateAirConditionFlag = true;
                }
                else
                {
                    this.openWindowFlag = true;
                }
            }
            else
            {
                this.openWindowFlag = true;
            }
        }
    }

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

    void OpenWindowHumidityControl()
    {
        float Humidity_IN = broker.GetHumidityInside();
        float Humidity_OUT = broker.GetHumidityOutside();

        if (InWantedHumidityRange(Humidity_IN) && Humidity_IN != Humidity_OUT)
        {
            if (this.openWindowFlag)
            {
                if ( (wantedHumidity > Humidity_IN && Humidity_IN > Humidity_OUT) /*too dry air outside*/ || (Humidity_OUT > Humidity_IN && Humidity_IN > wantedHumidity) /*too hazy outside*/)
                {
                    //deteriorating conditions
                    this.openWindowFlag = false;
                    this.avtivateAirConditionFlag = true;
                }
                else
                {
                    this.openWindowFlag = true;
                }
            }
            else
            {
                this.avtivateAirConditionFlag = true;
            }
        }
    }

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

    void OpenWindowCO2Control()
    {
        float CO2_IN = broker.GetCO2Inside();
        float CO2_OUT = broker.GetCO2Outside();

        if (InWantedCO2Range(CO2_IN) && CO2_IN != CO2_OUT)
        {
            if (this.openWindowFlag)
            {
                if ( (wantedCO2 > CO2_IN && CO2_IN > CO2_OUT) /*not stuffy enough air outside*/ || (CO2_OUT > CO2_IN && CO2_IN > wantedCO2) /*too stuffy air outside*/)
                {
                    //deteriorating conditions
                    this.openWindowFlag = false;
                    this.avtivateAirConditionFlag = true;
                }
                else
                {
                    this.openWindowFlag = true;
                }
            }
            else
            {
                this.avtivateAirConditionFlag = true;
            }
        }
    }




}
