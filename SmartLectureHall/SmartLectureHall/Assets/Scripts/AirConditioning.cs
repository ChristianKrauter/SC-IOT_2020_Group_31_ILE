using UnityEngine;

public class AirConditioning : MonoBehaviour
{
    public Animator anim;
    public GlobalVariables glob;

    // Start is called before the first frame update
    public void TurnOn(float temp)
    {
        anim.SetTrigger("on");
        glob.airConditioning = true;
        glob.airConditioningTemp = temp;
        print("Turned air conditioning on to " + temp);
    }

    // Update is called once per frame
    public void TurnOff()
    {
        anim.SetTrigger("off");
        glob.airConditioning = false;
        print("Turned air conditioning off");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            anim.ResetTrigger("off");
            TurnOn(24f);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            anim.ResetTrigger("on");
            TurnOff();
        }
    }
}
