using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class seatDisplay : MonoBehaviour
{
    public void changeDisplay(int seatNumber, string color = "green")
    {
        print("display updated at " + seatNumber + " to " + color);
    }
}
