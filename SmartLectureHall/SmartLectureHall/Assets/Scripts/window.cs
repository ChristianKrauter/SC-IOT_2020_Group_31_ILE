using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    public Animator anim;

    public void Open()
    {
        anim.SetTrigger("open");
        print("window opened");
    }

    public void Close()
    {
        anim.SetTrigger("close");
        print("window closed");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            anim.ResetTrigger("close");
            Open();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            anim.ResetTrigger("open");
            Close();
        }
    }
}
