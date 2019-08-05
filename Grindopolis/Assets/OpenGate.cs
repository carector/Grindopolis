using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenGate : MonoBehaviour
{
    public bool openGate;
    public InteractableNPC npc;
    public AnnouncementsScript an;
    public float stoppingPoint;
    public AudioClip gateSound;
    AudioSource audio;
    bool hasPlayedSound;
    

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(npc.lines.Length == 1)
        {
            openGate = true;
        }

        if(openGate)
        {
            if(!hasPlayedSound)
            {
                audio.PlayOneShot(gateSound);
                hasPlayedSound = true;
            }

            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.05f);


            if(transform.position.z >= stoppingPoint)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, stoppingPoint);
                an.RpcIronGateSound();
                this.enabled = false;
            }
        }
    }
}
