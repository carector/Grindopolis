using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootColliderScript : MonoBehaviour
{
    public bool isGrounded;

    public List<Collider> cols;
    public float maxSlope;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag != "Enemy" && other.tag != "Pickup" && other.tag != "Water" && other.tag != "Projectile" && other.tag != "Tree" && other.tag != "MusicTrigger")
            cols.Add(other);
    }
    private void OnTriggerExit(Collider other)
    {
        if (cols.Contains(other))
            cols.Remove(other);
    }
    

    private void FixedUpdate()
    {
        RaycastHit hit;
        Ray r = new Ray(transform.position, Vector3.down);

        // Check to make sure none of our colliders we've hit are too slope-y

        foreach (Collider c in cols)
        {
            if (c.Raycast(r, out hit, 10000f))
            {
                if(Vector3.Angle(hit.normal, transform.up) > maxSlope)
                {
                    isGrounded = false;
                    return;
                }
            }
        }

        if (cols.Count > 0)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }        
    }
}
