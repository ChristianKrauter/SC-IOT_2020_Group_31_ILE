using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    public Animator anim;
    public GlobalVariables glob;

    public void Open()
    {
        anim.SetTrigger("open");
        print("window opened");
        glob.ventilating = true;
    }

    public void Close()
    {
        anim.SetTrigger("close");
        print("window closed");
        glob.ventilating = false;
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
