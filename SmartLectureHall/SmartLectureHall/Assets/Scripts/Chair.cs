using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chair : MonoBehaviour
{
    private Animator anim;
    private Transform indicator;
    private Material red_material;
    private Material green_material;

    public void LockChair()
    {
        anim.SetTrigger("lock");
        //print("chair locked");
    }

    public void UnlockChair()
    {
        anim.SetTrigger("unlock");
        //print("chair unlocked");
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
        Renderer meshRenderer = indicator.GetComponent<Renderer>();
        if (color == "green")
        {
            meshRenderer.material = green_material;
        }
        else
        {
            meshRenderer.material = red_material;
        }
        //print("chair changed to " + color);
    }

    public void Start()
    {
        red_material = Resources.Load<Material>("Materials/red_alert");
        green_material = Resources.Load<Material>("Materials/green_alert");
        anim = GetComponent<Animator>();
        indicator = transform.Find("student_indicator");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            anim.ResetTrigger("unlock");
            ChangeIndicator("red");
            LockChair();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            anim.ResetTrigger("lock");
            ChangeIndicator("green");
            UnlockChair();
        }
    }
}
