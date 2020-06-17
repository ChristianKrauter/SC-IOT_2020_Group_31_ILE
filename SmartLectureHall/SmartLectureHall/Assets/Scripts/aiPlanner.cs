using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlanner : MonoBehaviour
{
    public Window window;
    public Window window2;
    public GlobalVariables glob;
    public AirConditioning airCon;

    public float wantedTemperature = 20.5f; // centigrade
    public float wantedHumidity = 0.55f; // humidity in % at the desired temperature
    public float wantedCO2 = 0.5f; // CO2 

    private bool isWindowOpen = false;

    // Start is called before the first frame update
    void Start()
    {}

    // Update is called once per frame
    void Update()
    {}

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
        airCon.TurnOn((int)wantedTemperature);
    }

    void openWindowControl()
    {
        bool openWindowFlag;

        while (true){
            openWindowFlag = true;
            openWindowFlag = openWindowTemperatureControl(openWindowFlag);
            openWindowFlag = openWindowHumidityControl(openWindowFlag);
            openWindowFlag = openWindowCO2Control(openWindowFlag);

            if (openWindowFlag)
            {
                if (isWindowOpen)
                {
                    //OpenWindows(); // if it is open we dont have to open it.
                    ActivateAirCondition();
                }
                else {
                    OpenWindows();
                }
            }
            else {
                ColoseWindows();
            }
        }
    }

    bool openWindowTemperatureControl(bool openWindowFlag)
    {
        float Temp_IN = glob.inner_base_temperature;
        float Temp_OUT = glob.outer_temperature;

        if (Temp_IN != Temp_OUT) {
            if (openWindowFlag)
            {
                if (wantedTemperature > Temp_IN && Temp_IN > Temp_OUT)
                {
                    openWindowFlag = false;
                    ActivateAirCondition();
                }
                else {
                    openWindowFlag = true;
                }
            }
            else {
                ActivateAirCondition();
            }
        }
        return openWindowFlag;
    }

    bool openWindowHumidityControl(bool openWindowFlag)
    {
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

    bool openWindowCO2Control(bool openWindowFlag)
    {
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
