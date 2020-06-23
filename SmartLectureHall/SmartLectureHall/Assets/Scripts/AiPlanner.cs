using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AiPlanner : MonoBehaviour
{
    public GlobalVariables glob;
    public Window window;
    public Window window2;
    public AirConditioning airCon;
    public AirConditioning airCon2;
    public AirConditioning airCon3;
    public AirConditioning airCon4;

    public float wantedTemperature = 20f; // centigrade
    public float wantedHumidity = 0.55f; // humidity in % at the desired temperature
    public float wantedCO2 = 0.5f; // CO2 

    public GameObject seat_rows;
    private bool isWindowOpen = false;
    private int[,] currentSeatingPlan;
    private readonly Chair[,] chairs = new Chair[19, 14];
    private float timer = 0.0f;

    [Header("exam, pandemic")]
    [Header("smallClass, mcTest")]
    [Header("standard, frontHalf")]
    [Header("Available seating plans:")]
    public string seatingPlan = "standard";

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

        for (int i = 0; i < 19; i++)
        {
            var row = seat_rows.gameObject.transform.GetChild(i);
            for (int j = 0; j < 14; j++)
            {
                chairs[i, j] = row.gameObject.transform.GetChild(j).GetComponent<Chair>();
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
        if (timer > 5)
        {
            print("hi");
            OpenWindowControl();
            print("ho");
            timer = 0;
            UpdateSeatingDisplay();
            //ApplySeatingPlan();
            
        }
    }

    void UpdateSeatingDisplay()
    {

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
        //TODO GetData form Broker
        float Temp_IN = glob.inner_base_temperature;
        float Temp_OUT = glob.outer_temperature;

        if (Temp_IN != Temp_OUT)
        {
            if (openWindowFlag)
            {
                if (wantedTemperature > Temp_IN && Temp_IN > Temp_OUT)
                {
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
        return openWindowFlag;
    }

    bool OpenWindowHumidityControl(bool openWindowFlag)
    {
        //TODO GetData form Broker
        float Humidity_IN = glob.inner_humidity;
        float Humidity_OUT = glob.outer_humidity;

        if (Humidity_IN != Humidity_OUT)
        {
            if (openWindowFlag)
            {
                if (wantedHumidity > Humidity_IN && Humidity_IN > Humidity_OUT)
                {
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
        return openWindowFlag;
    }

    bool OpenWindowCO2Control(bool openWindowFlag)
    {
        //TODO GetData form Broker
        float CO2_IN = glob.inner_co2;
        float CO2_OUT = glob.outer_co2;

        if (CO2_IN != CO2_OUT)
        {
            if (openWindowFlag)
            {
                if (wantedCO2 > CO2_IN && CO2_IN > CO2_OUT)
                {
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
        return openWindowFlag;
    }




}
