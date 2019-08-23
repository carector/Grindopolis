using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PickupControlNoNet : MonoBehaviour
{
    bool hasHitGround;

    Rigidbody rb;

    public Vector3 difference;
    public Transform focusedTransform;
    public int collideDamage = 15;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (focusedTransform != null)
        {
            if (Vector3.Distance(transform.position, focusedTransform.position) <= 3f)
            {
                difference = transform.position - focusedTransform.position;

                rb.useGravity = false;
                transform.parent = focusedTransform;

                rb.AddForce(-difference.normalized * 0.85f, ForceMode.VelocityChange);

                rb.angularVelocity = Vector3.zero;


                transform.position = Vector3.Lerp(transform.position, focusedTransform.transform.position, 0.1f);
            }
            else
            {
                focusedTransform = null;
            }
        }
        else
        {
            rb.useGravity = true;
            transform.parent = null;

        }
    }

    public void UpdateTransform(Transform t)
    {
        focusedTransform = t;
    }

    public void DropObject(Vector3 newPosition)
    {
        transform.parent = null;
        hasHitGround = false;

        rb.angularVelocity = Vector3.zero;
        transform.position = newPosition;

        focusedTransform = null;

        rb.useGravity = true;
    }

    public void LaunchObject(Vector3 newPosition, Vector3 newVelocity)
    {
        transform.parent = null;
        hasHitGround = false;

        rb.AddForce(-newVelocity, ForceMode.VelocityChange);
        rb.angularVelocity = Vector3.zero;
        transform.position = newPosition;

        focusedTransform = null;

        rb.useGravity = true;
    }

    // Used to deal damage to enemies
    void OnTriggerEnter(Collider other)
    {
        if (hasHitGround)
            return;

        if (other.gameObject.tag == "Terrain")
            hasHitGround = true;

        if (rb.velocity.magnitude >= 10 && other.gameObject.GetComponent<EnemyControl>() != null)
        {
            EnemyControl e = other.GetComponent<EnemyControl>();

            e.StartCoroutine(e.ReceiveObjectDamage(collideDamage));
        }
    }
}
