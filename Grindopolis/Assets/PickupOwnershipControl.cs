using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PickupOwnershipControl : MonoBehaviourPunCallbacks, IPunObservable
{
    int storedID;
    Rigidbody rb;

    public Vector3 difference;
    public Transform focusedTransform;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Send data
        if (stream.IsWriting)
        {
            stream.SendNext(GetComponent<Rigidbody>().useGravity);
        }
        else
        {
            GetComponent<Rigidbody>().useGravity = (bool)stream.ReceiveNext();
        }
    }

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
            if (Vector3.Distance(transform.position, focusedTransform.position) <= 2.5f)
            {
                difference = transform.position - focusedTransform.position;

                rb.useGravity = false;
                transform.parent = focusedTransform;

                if(photonView.IsMine)
                    rb.AddForce(-difference.normalized * 0.75f, ForceMode.VelocityChange);

                rb.angularVelocity = Vector3.zero;


                transform.position = Vector3.Lerp(transform.position, focusedTransform.transform.position, 0.08f);
            }
            else
            {
                focusedTransform = null;
                transform.parent = null;
            }
        }
        else
        {
            rb.useGravity = true;
        }
    }

    [PunRPC]
    void RpcUpdateOwnership(int id, string str)
    {
        storedID = id;
        this.photonView.TransferOwnership(id);
        focusedTransform = GameObject.Find(str).transform;
    }

    // Drop the object with stored velocity and position
    [PunRPC]
    void RpcDropObject(Vector3 newPosition, Vector3 newVelocity)
    {
        transform.parent = null;

        rb.AddForce(-newVelocity, ForceMode.VelocityChange);
        rb.angularVelocity = Vector3.zero;
        transform.position = newPosition;

        focusedTransform = null;
        
        rb.useGravity = true;
    }
}
