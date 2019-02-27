using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateNPCLines : MonoBehaviour
{
    public InteractableNPC npc;
    public string line1;
    public string line2;
    public string line3;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "ClientPlayer")
        {
            /*
            npc.line1 = line1;
            npc.line2 = line2;
            npc.line3 = line3;
            */
        }
    }
}
