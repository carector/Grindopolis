using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevitationResetter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
       
        if(other.gameObject.tag == "Pickup")
        {
            print(other.name + " left");
            other.GetComponent<Rigidbody>().useGravity = true;
        }
    }
}
