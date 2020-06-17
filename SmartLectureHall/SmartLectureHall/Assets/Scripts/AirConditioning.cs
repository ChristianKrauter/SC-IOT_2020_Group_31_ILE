using UnityEngine;

public class AirConditioning : MonoBehaviour
{

    public Animator anim;

    // Start is called before the first frame update
    public void TurnOn(int temp)
    {
        anim.SetTrigger("on");
        print("Turned air conditioning on to " + temp);
    }

    // Update is called once per frame
    public void TurnOff()
    {
        anim.SetTrigger("off");
        print("Turned air conditioning off");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            anim.ResetTrigger("off");
            TurnOn(24);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            anim.ResetTrigger("on");
            TurnOff();
        }
    }
}
