using UnityEngine;

public class AirConditioning : MonoBehaviour
{
    public Animator anim;
    public Environment env;
    private ParticleSystem ps;

    public void Start()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        ps.Stop();
    }

    public void TurnOn(float temp, float humidity, float co2)
    {
        anim.SetTrigger("on");
        env.airConditioningTemp = temp;
        env.airConditioningHumid = humidity;
        env.airConditioningCO2 = co2;
        env.airConditioning = true;

        ps.Play();
    }

    public void TurnOff()
    {
        anim.SetTrigger("off");
        env.airConditioning = false;
        ps.Stop();
    }

    // Change manually
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            anim.ResetTrigger("off");
            TurnOn(24f, 50f, 6f);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            anim.ResetTrigger("on");
            TurnOff();
        }
    }
}
