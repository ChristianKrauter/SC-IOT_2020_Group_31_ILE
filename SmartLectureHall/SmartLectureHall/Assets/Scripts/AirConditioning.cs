using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirConditioning : MonoBehaviour
{
    // Start is called before the first frame update
    public void TurnOn(int temp)
    {
        print("Turned air conditioning on to " + temp);
    }

    // Update is called once per frame
    public void TurnOff()
    {
        print("Turned air conditioning off");
    }
}
