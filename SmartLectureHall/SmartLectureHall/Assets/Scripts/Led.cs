using UnityEngine;

public class Led : MonoBehaviour
{
    private Material red_material;
    private Material green_material;

    void Start()
    {
        red_material = Resources.Load<Material>("Materials/red_alert");
        green_material = Resources.Load<Material>("Materials/green_alert");
    }

    public void ChangeColor(string color = "green")
    {
        Renderer meshRenderer = this.GetComponent<Renderer>();
        if (color == "green")
        {
            meshRenderer.material = green_material;
        }
        else if (color == "red")
        {
            meshRenderer.material = red_material;
        }
    }

    // Change manually
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            ChangeColor("red");
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            ChangeColor("green");
        }
    }
}
