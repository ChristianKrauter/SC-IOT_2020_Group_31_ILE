using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlanner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
        return true;
    }

    bool openWindowCO2Control(bool openWindowFlag)
    {
        return true;
    }

    


}
