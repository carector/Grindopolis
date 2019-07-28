using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PissControl : MonoBehaviour
{
    public Transform camera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = camera.transform.rotation;    
    }
}
