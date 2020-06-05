using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class seat : MonoBehaviour
{
    public void LockChair()
    {
        print("chair locked");
    }

    public void UnlockChair()
    {
        print("chair unlocked");
    }

    public void OccupyChair()
    {
        print("chair occupied");
    }

    public void EmptyChair()
    {
        print("chair emptied");
    }

    public void changeIndicator(string color = "green")
    {
        print("chair chaded to " + color);
    }

}
