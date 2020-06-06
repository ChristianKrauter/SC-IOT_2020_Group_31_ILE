using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seat : MonoBehaviour
{
    private Animator anim;

    public void LockChair()
    {
        anim.SetTrigger("lock");
        print("chair locked");
    }

    public void UnlockChair()
    {
        anim.SetTrigger("unlock");
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

    public void ChangeIndicator(string color = "green")
    {
        print("chair chaded to " + color);
    }

    public void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            anim.ResetTrigger("unlock");
            LockChair();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            anim.ResetTrigger("lock");
            UnlockChair();
        }
    }
}
