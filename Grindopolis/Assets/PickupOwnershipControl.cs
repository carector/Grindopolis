using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PickupOwnershipControl : MonoBehaviourPunCallbacks, IPunObservable
{
    int storedID;

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

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateOwnership(int id)
    {
        photonView.RPC("UpdateOwnershipRPC", RpcTarget.All);
        /*
        // Check to see if this ID already owns this pickup
        if (id == 0)
        {
            storedID = id;
            
        }

        else
        {
            print("Pickup is already owned!");
        }
        */
    }

    [PunRPC]
    void UpdateOwnershipRPC()
    {
        this.photonView.TransferOwnership(storedID);
    }
}
