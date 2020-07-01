using UnityEngine;

public class Window : MonoBehaviour
{
    public Animator anim;
    public Environment env;

    public void Open()
    {
        anim.SetTrigger("open");
        print("window opened");
        env.ventilating = true;
        anim.ResetTrigger("close");
    }

    public void Close()
    {
        anim.SetTrigger("close");
        print("window closed");
        env.ventilating = false;
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
