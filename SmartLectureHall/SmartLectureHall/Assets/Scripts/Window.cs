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
        anim.ResetTrigger("close");
    }

    public void Close()
    {
        anim.SetTrigger("close");
        print("window closed");
        glob.ventilating = false;
        anim.ResetTrigger("open");
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
