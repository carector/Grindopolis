using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupScript : MonoBehaviour
{
    public string pickupName;

    public Transform focus;

    Rigidbody rb;
    Collider col;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.parent != null && transform.parent.name == "ClientPlayer")
        {
            rb.useGravity = false;
            col.enabled = false;
            transform.position = Vector3.Lerp(transform.position, focus.position, 0.25f);
            transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(0, focus.rotation.y, 0, focus.rotation.w), 0.25f);
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.5f);
            rb.angularVelocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.5f);
        }
        else
        {
            col.enabled = true;
            rb.useGravity = true;
        }
    }
}
