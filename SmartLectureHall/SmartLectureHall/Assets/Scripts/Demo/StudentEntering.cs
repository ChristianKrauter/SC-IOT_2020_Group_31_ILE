using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudentEntering : MonoBehaviour
{
    private bool enter;
    private float enteringTimer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (enter)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(-5.931f, 21.228f, 38.686f), .01f);
        }
  
        if (Input.GetKeyDown(KeyCode.R))
        {
            this.transform.position = new Vector3(-5.152f, 21.228f, 40.185f);
            enter = false;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            enter = true;
        }
    }
}
