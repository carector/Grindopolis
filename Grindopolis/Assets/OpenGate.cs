using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class OpenGate : MonoBehaviourPunCallbacks, IPunObservable
{
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Send data
        if (stream.IsWriting)
        {
            stream.SendNext(openGate);
        }
        // Receive data
        else
        {
            openGate = (bool)stream.ReceiveNext();
        }

    }

    public bool openGate;
    public InteractableNPC npc;
    public AnnouncementsScript an;
    public float stoppingPoint;
    public AudioClip gateSound;
    AudioSource audio;
    bool hasPlayedSound;
    bool completedCycle;
    

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        if (!completedCycle)
        {
            if (npc.lines.Length == 1)
            {
                openGate = true;
            }

            if (openGate)
            {
                if (!hasPlayedSound)
                {
                    audio.PlayOneShot(gateSound);
                    hasPlayedSound = true;
                }

                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.05f);


                if (transform.position.z >= stoppingPoint)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y, stoppingPoint);
                    if (an.photonView.IsMine)
                        an.RpcIronGateSound();

                    completedCycle = true;
                }
            }
        }
    }
}
