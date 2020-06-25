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
    public float wantedHumidity = 0.55f; // humidity in % at the desired temperature
    public float wantedCO2 = 0.5f; // CO2 
    private float timer = 0.0f;

    public GameObject seat_rows;
    private bool isWindowOpen = false;
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
        if (timer > 10)
        {
            // print("hi");
            OpenWindowControl();
            // print("ho");
            
            UpdateSeatingDisplay();
            //ApplySeatingPlan();

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
                // Todo get occupation status from broker / Solved?
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
        window.Open();
        window2.Open();
        isWindowOpen = true;
    }

    void ColoseWindows()
    {
        window.Close();
        window2.Close();
        isWindowOpen = false;
    }

    void ActivateAirCondition()
    {
        airCon.TurnOn(wantedTemperature);
        airCon2.TurnOn(wantedTemperature);
        airCon3.TurnOn(wantedTemperature);
        airCon4.TurnOn(wantedTemperature);
    }

    void OpenWindowControl()
    {
        bool openWindowFlag;

        
        openWindowFlag = true;
        openWindowFlag = OpenWindowTemperatureControl(openWindowFlag);
        openWindowFlag = OpenWindowHumidityControl(openWindowFlag);
        openWindowFlag = OpenWindowCO2Control(openWindowFlag);

        if (openWindowFlag)
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
            ColoseWindows();
        }
        
    }

    bool OpenWindowTemperatureControl(bool openWindowFlag)
    {

        float Temp_IN = broker.GetTemperatureInside();
        float Temp_OUT = broker.GetTemperatureOutside();

        if (Temp_IN != Temp_OUT)
        {
            if (openWindowFlag)
            {
                print("Temp_OUT: " + Temp_OUT + " Temp_IN " + Temp_IN + " wantedTemp " + wantedTemperature);
                if ( (wantedTemperature > Temp_IN && Temp_IN > Temp_OUT) /*too cold outside*/ || (Temp_OUT > Temp_IN && Temp_IN > wantedTemperature) /*too hot outside*/ )
                {
                    //deteriorating conditions
                    openWindowFlag = false;
                    ActivateAirCondition();
                    print("Temp_worse openWindow? " + openWindowFlag);
                }
                else
                {
                    //openWindowFlag = true;
                    print("Temp_not_worse openWindow? " + openWindowFlag);
                }
            }
            else
            {
                ActivateAirCondition();
            }
        }
        print("Temp openWindow? " + openWindowFlag);
        return openWindowFlag;
    }

    bool OpenWindowHumidityControl(bool openWindowFlag)
    {
        float Humidity_IN = broker.GetHumidityInside();
        float Humidity_OUT = broker.GetHumidityOutside();

        if (Humidity_IN != Humidity_OUT)
        {
            if (openWindowFlag)
            {
                if ( (wantedHumidity > Humidity_IN && Humidity_IN > Humidity_OUT) /*too dry air outside*/ || (Humidity_OUT > Humidity_IN && Humidity_IN > wantedHumidity) /*too hazy outside*/)
                {
                    //deteriorating conditions
                    openWindowFlag = false;
                    ActivateAirCondition();
                }
                else
                {
                    openWindowFlag = true;
                }
            }
            else
            {
                ActivateAirCondition();
            }
        }
        print("Humidity openWindow?" + openWindowFlag);
        return openWindowFlag;
    }

    bool OpenWindowCO2Control(bool openWindowFlag)
    {
        float CO2_IN = broker.GetCO2Inside();
        float CO2_OUT = broker.GetCO2Outside();

        if (CO2_IN != CO2_OUT)
        {
            if (openWindowFlag)
            {
                if ( (wantedCO2 > CO2_IN && CO2_IN > CO2_OUT) /*not stuffy enough air outside*/ || (CO2_OUT > CO2_IN && CO2_IN > wantedCO2) /*too stuffy air outside*/)
                {
                    //deteriorating conditions
                    openWindowFlag = false;
                    ActivateAirCondition();
                }
                else
                {
                    openWindowFlag = true;
                }
            }
            else
            {
                ActivateAirCondition();
            }
        }
        print("CO2 openWindow?" + openWindowFlag);
        return openWindowFlag;
    }




}
