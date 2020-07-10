using CymaticLabs.Unity3D.Amqp;
using System.Collections.Generic;
using UnityEngine;

public class Chair : MonoBehaviour
{
    private Animator anim;
    private Transform indicator;
    private Material red_material;
    private Material green_material;
    private Material orange_material;
    public Broker broker;

    [HideInInspector]
    public bool isLocked;
    [HideInInspector]
    public bool isOccupied;

    private Transform student;
    string sensorId;
    bool sensorValue;

    ChairRowController chairRowController;
   

    public void Start()
    {

        

        this.isOccupied = false;
        red_material = Resources.Load<Material>("Materials/red_alert");
        green_material = Resources.Load<Material>("Materials/green_alert");
        orange_material = Resources.Load<Material>("Materials/orange_alert");
        anim = GetComponent<Animator>();
        indicator = transform.Find("student_indicator");
        student = transform.Find("student");
        student.gameObject.SetActive(false);
        sensorId = transform.parent.name.Split('_')[2] + "-" + this.name.Split('r')[1];
        sensorValue = isOccupied;

        chairRowController = this.gameObject.GetComponentInParent<ChairRowController>();
    }

    public void LockChair()
    {
        this.isLocked = true;
        if (student.gameObject.activeSelf == true)
        {
            ChangeIndicator("orange");
        }
        else
        {
            anim.ResetTrigger("unlock");
            ChangeIndicator("red");
            anim.SetTrigger("lock");
        }
    }

    public void UnlockChair()
    {
        this.isLocked = false;
        anim.ResetTrigger("lock");
        ChangeIndicator("green");
        anim.SetTrigger("unlock");
    }

    public void OccupyChair()
    {
        isOccupied = true;
        student.gameObject.SetActive(true);
        sensorValue = true;

        SensorData data = new SensorData();
        data.position = sensorId;
        data.family = "";
        data.occupancie = isOccupied;
        data.type = "chair";

        chairRowController.sendData(data);
        
    }

    public void EmptyChair()
    {
        isOccupied = false;
        student.gameObject.SetActive(false);
        sensorValue = false;

        SensorData data = new SensorData();
        data.position = sensorId;
        data.family = "";
        data.occupancie = isOccupied;
        data.type = "chair";

        chairRowController.sendData(data);
    }

    public void ChangeIndicator(string color = "green")
    {
        Renderer meshRenderer = indicator.GetComponent<Renderer>();
        if (color == "green")
        {
            meshRenderer.material = green_material;
        }
        else if (color == "red")
        {
            meshRenderer.material = red_material;
        }
        else if (color == "orange")
        {
            meshRenderer.material = orange_material;
        }
    }

    // Change manually
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            anim.ResetTrigger("unlock");
            LockChair();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            anim.ResetTrigger("lock");
            UnlockChair();
        }
    }

    
}
