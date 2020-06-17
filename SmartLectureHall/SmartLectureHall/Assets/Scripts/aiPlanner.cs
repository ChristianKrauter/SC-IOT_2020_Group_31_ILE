using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlanner : MonoBehaviour
{
    public Window window;
    public Window window2;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OpenWindows()
    {
        window.Open();
        window2.Open();
    }

    void openWindowControl()
    {
        bool openWindowFlag;
        bool isWindowOpen = false; //TODO: get isWindowOpen

        while (true){
            openWindowFlag = true;
            openWindowFlag = openWindowTemperatureControl(openWindowFlag);
            openWindowFlag = openWindowHumidityControl(openWindowFlag);
            openWindowFlag = openWindowCO2Control(openWindowFlag);

            if (openWindowFlag)
            {
                if (isWindowOpen)
                {
                    //TODO: openWindow();
                    OpenWindows();
                    //TODO: activateAireCondition()
                }
                else {
                    //TODO: openWindow();
                }
            }
            else {
                //TODO: closeWindow();
            }
        }
    }

    bool openWindowTemperatureControl(bool openWindowFlag)
    {
        float Temp_IN = 18.0f; //TODO get Temp_IN
        float Temp_OUT = 22.0f; //TODO get Temp_OUT
        float wantedTemp = 22.5f; //TODO get wantedTemp

        if (Temp_IN != Temp_OUT) {
            if (openWindowFlag)
            {
                if (wantedTemp > Temp_IN && Temp_IN > Temp_OUT)
                {
                    openWindowFlag = false;
                    //TODO: activateAireCondition()
                }
                else {
                    openWindowFlag = true;
                }
            }
            else {
                //TODO: activateAireCondition()
            }
        }
        return openWindowFlag;
    }

    bool openWindowHumidityControl(bool openWindowFlag)
    {
        float Humidity_IN = 18.0f; //TODO get Temp_IN
        float Humidity_OUT = 22.0f; //TODO get Temp_OUT
        float wantedHumidity = 22.5f; //TODO get wantedTemp

        if (Humidity_IN != Humidity_OUT)
        {
            if (openWindowFlag)
            {
                if (wantedHumidity > Humidity_IN && Humidity_IN > Humidity_OUT)
                {
                    openWindowFlag = false;
                    //TODO: activateAireCondition()
                }
                else
                {
                    openWindowFlag = true;
                }
            }
            else
            {
                //TODO: activateAireCondition()
            }
        }
        return openWindowFlag;
    }

    bool openWindowCO2Control(bool openWindowFlag)
    {
        float CO2_IN = 18.0f; //TODO get Temp_IN
        float CO2_OUT = 22.0f; //TODO get Temp_OUT
        float wantedCO2 = 22.5f; //TODO get wantedTemp

        if (CO2_IN != CO2_OUT)
        {
            if (openWindowFlag)
            {
                if (wantedCO2 > CO2_IN && CO2_IN > CO2_OUT)
                {
                    openWindowFlag = false;
                    //TODO: activateAireCondition()
                }
                else
                {
                    openWindowFlag = true;
                }
            }
            else
            {
                //TODO: activateAireCondition()
            }
        }
        return openWindowFlag;
    }




}
