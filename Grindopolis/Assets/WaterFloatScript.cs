using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterFloatScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.GetComponent<Rigidbody>() != null)
        {
            other.GetComponent<Rigidbody>().AddForce(Vector3.up * 50 );
            other.GetComponent<Rigidbody>().drag = 0.05f;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null)
        {
            other.GetComponent<Rigidbody>().AddForce(-Vector3.up * 1000);
            other.GetComponent<Rigidbody>().drag = 0;
        }
    }
}
