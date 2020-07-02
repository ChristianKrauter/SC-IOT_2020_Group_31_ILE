using UnityEngine;

public class AirConditioning : MonoBehaviour
{
    public Animator anim;
    public Environment env;
    private ParticleSystem ps;
    
    public void TurnOn(float temp, float humidity, float co2)
    {
		humidity = humidity; //TODO air condition changes enviroment humidity
		co2 = co2; // TODO air condition changes enviroment co2
        anim.SetTrigger("on");
        env.airConditioning = true;
        env.airConditioningTemp = temp;
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
            TurnOn(24f);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            anim.ResetTrigger("on");
            TurnOff();
        }
    }
}
