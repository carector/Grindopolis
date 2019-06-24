using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayNewLines : MonoBehaviour
{
    public InteractableNPC npc;
    public InteractableNPC.NPCLines[] newLines;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "ClientPlayer")
        {
            UpdateLines();
        }
    }

    public void UpdateLines()
    {
        // Replace our current NPCLines with our updated ones (newLines)
        npc.lines = newLines;
    }
}
