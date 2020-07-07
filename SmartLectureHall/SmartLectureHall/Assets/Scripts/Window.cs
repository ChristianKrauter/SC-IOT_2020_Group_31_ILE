using UnityEngine;

public class Window : MonoBehaviour
{
    public Animator anim;
    public Environment env;

    public void Open()
    {
        anim.SetTrigger("open");
        env.ventilating = true;
        anim.ResetTrigger("close");
    }

    public void Close()
    {
        anim.SetTrigger("close");
        env.ventilating = false;
        anim.ResetTrigger("open");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            anim.ResetTrigger("close");
            Open();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            anim.ResetTrigger("open");
            Close();
        }
    }
}
