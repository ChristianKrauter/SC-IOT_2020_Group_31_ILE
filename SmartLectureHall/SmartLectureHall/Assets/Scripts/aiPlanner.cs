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
            openWindowFlag = openWindowTemperatureControl();
            openWindowFlag = openWindowHumidityControl();
            openWindowFlag = openWindowCO2Control();

            if (openWindowFlag)
            {
                if (isWindowOpen)
                {
                    //TODO: openWindow();
                    //TODO: activateAireCondition
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

    bool openWindowTemperatureControl()
    {
        return true;
    }

    bool openWindowHumidityControl()
    {
        return true;
    }

    bool openWindowCO2Control()
    {
        return true;
    }

    


}
