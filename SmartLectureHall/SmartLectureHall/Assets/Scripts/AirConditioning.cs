using UnityEngine;

public class AirConditioning : MonoBehaviour
{
    public Animator anim;
    public Environment env;
    private ParticleSystem ps;

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

	// Start is called before the first frame update
    public void Start()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        ps.Stop();
    }

	// Update is called once per frame
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            anim.ResetTrigger("off");
            TurnOn(24f, 0f, 0f); // TODO add real values for humidity and co2 // this is just for testing, so, no
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            anim.ResetTrigger("on");
            TurnOff();
        }
    }
}
