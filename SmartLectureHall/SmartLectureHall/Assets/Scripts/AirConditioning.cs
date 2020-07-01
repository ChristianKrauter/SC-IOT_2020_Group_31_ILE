using UnityEngine;

public class AirConditioning : MonoBehaviour
{
    public Animator anim;
    public Environment env;

    // Start is called before the first frame update
    public void TurnOn(float temp)
    {
        anim.SetTrigger("on");
        env.airConditioning = true;
        env.airConditioningTemp = temp;
    }

    // Update is called once per frame
    public void TurnOff()
    {
        anim.SetTrigger("off");
        env.airConditioning = false;
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
