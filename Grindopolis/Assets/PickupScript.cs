using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PickupScript : NetworkBehaviour
{
    public bool isHeld;

    public string pickupName;

    public GameObject holder;

    Transform focus;
    bool hasSetToHeld;
    Rigidbody rb;
    Collider col;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

        // Check to make sure the rigidbody has gravity disabled
        // This is done so when a new player joins the game,
        // any held objects will automatically update isHeld to true
        if (!rb.useGravity)
        {
            isHeld = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isHeld)
        {
            if (holder != null)
            {
                focus = holder.GetComponent<PlayerController>().heldObjectFocus;
                transform.position = Vector3.Lerp(transform.position, focus.position, 0.25f);
                transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(0, focus.rotation.y, 0, focus.rotation.w), 0.25f);
            }

            rb.useGravity = false;
            col.enabled = false;
            
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.5f);
            rb.angularVelocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.5f);

            // Send the command to signal isHeld is true
            // Also check to make sure we haven't done this already so we aren't constantly sending data
            if (!hasSetToHeld)
            {
                CmdSendHeldBool(true);
                hasSetToHeld = true;
            }
        }
        else
        {
            focus = null;
            holder = null;
            transform.parent = null;

            if(col.enabled == false)
            {
                col.enabled = false;
                col.enabled = true;
            }
            rb.useGravity = true;

            // Set isHeld to false, and send the command to set it to false
            if (hasSetToHeld)
            {
                CmdSendHeldBool(false);
                hasSetToHeld = false;
            }
        }
    }

    // Used to signal that the object has been picked up by some client
    [Command]
    void CmdSendHeldBool(bool heldState)
    {
        RpcSendHeldBool(heldState);
    }

    [ClientRpc]
    void RpcSendHeldBool(bool heldState)
    {
        isHeld = heldState;

        if(!heldState)
        {
            holder = null;
        }
    }
    
}
