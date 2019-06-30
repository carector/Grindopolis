using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Photon.Pun;

public class PickupScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Send data
        if (stream.IsWriting)
        {
            stream.SendNext(isHeld);
            stream.SendNext(holderName);
        }
        // Recieve data
        else
        {
            isHeld = (bool)stream.ReceiveNext();
            holderName = (string)stream.ReceiveNext();
        }
    }

    public bool isHeld;

    public string pickupName;

    public GameObject holder;

    Transform focus;
    string holderName;
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
        if (isHeld && holder.GetPhotonView().IsMine)
        {
            holder = GameObject.Find(holderName);
            if (holder != null)
            {
                focus = holder.GetComponent<PlayerControllerRigidbody>().heldObjectFocus;
                transform.position = Vector3.Lerp(transform.position, focus.position, 0.25f);
                transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(0, focus.rotation.y, 0, focus.rotation.w), 0.25f);
            }

            rb.useGravity = false;
            rb.isKinematic = true;
            col.enabled = false;

            transform.parent = focus;

            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.5f);
            rb.angularVelocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.5f);
        }
        else
        {
            focus = null;
            holder = null;
            transform.parent = null;

            if(col.enabled == false)
            {
                col.enabled = true;
            }
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }
    public void SetPickupState(bool heldState, GameObject ply)
    {
        holder = ply;
        holderName = ply.name;
        photonView.RPC("RpcSetPickupState", RpcTarget.All, heldState);
    }
    // Used to signal that the object has been picked up by some client
    [PunRPC]
    void RpcSetPickupState(bool heldState)
    {
        isHeld = heldState;

        if(!heldState)
        {
            rb.useGravity = true;
            transform.parent = null;
            focus = null;
            holder = null;
        }
        else
        {
            rb.useGravity = false;
            transform.parent = holder.transform;
            focus = holder.GetComponent<PlayerControllerRigidbody>().heldObjectFocus;

        }
    }
    
}
