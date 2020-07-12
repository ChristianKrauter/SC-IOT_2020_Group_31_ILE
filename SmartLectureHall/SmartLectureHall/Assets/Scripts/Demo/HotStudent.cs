using UnityEngine;

public class HotStudent : MonoBehaviour
{
    private Renderer bodyRenderer;
    private Renderer headRenderer;
    private Renderer armRenderer;
    private Renderer arm2Renderer;

    private float heatingTimer;
    private float coolingTimer;
    private bool heating = false;
    private bool cooling = false;

    void Start()
    {
        headRenderer = this.GetComponent<Transform>().GetChild(0).GetComponent<Renderer>();
        bodyRenderer = this.GetComponent<Transform>().GetChild(1).GetComponent<Renderer>();
        armRenderer = this.GetComponent<Transform>().GetChild(2).GetComponent<Renderer>();
        arm2Renderer = this.GetComponent<Transform>().GetChild(3).GetComponent<Renderer>();
    }

    void Update()
    {
        if (heating)
        {
            Color colorStart = new Color(1f, 0.8953152f, 0.7028302f, 1);
            Color colorEnd = Color.red;
            float duration = 5.0f;
            float lerp = heatingTimer / duration;
            headRenderer.material.color = Color.Lerp(colorStart, colorEnd, lerp);
            bodyRenderer.material.color = Color.Lerp(colorStart, colorEnd, lerp);
            armRenderer.material.color = Color.Lerp(colorStart, colorEnd, lerp);
            arm2Renderer.material.color = Color.Lerp(colorStart, colorEnd, lerp);
            heatingTimer += Time.deltaTime;
        }
        if (cooling)
        {
            Color colorStart = Color.red;
            Color colorEnd = new Color(1f, 0.8953152f, 0.7028302f, 1);
            float duration = 5.0f;
            float lerp = coolingTimer / duration;
            headRenderer.material.color = Color.Lerp(colorStart, colorEnd, lerp);
            bodyRenderer.material.color = Color.Lerp(colorStart, colorEnd, lerp);
            armRenderer.material.color = Color.Lerp(colorStart, colorEnd, lerp);
            arm2Renderer.material.color = Color.Lerp(colorStart, colorEnd, lerp);
            coolingTimer += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            heatingTimer = 0;
            cooling = false;
            heating = true;
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            coolingTimer = 0;
            heating = false;
            cooling = true;
        }
    }
}
