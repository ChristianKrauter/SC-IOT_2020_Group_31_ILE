using UnityEngine;

public class SeatDisplay : MonoBehaviour
{
    public void ChangeDisplay(int seatNumber, string color = "green")
    {
        print("display updated at " + seatNumber + " to " + color);
    }
}
