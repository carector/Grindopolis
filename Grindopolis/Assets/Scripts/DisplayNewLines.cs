using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayNewLines : MonoBehaviour
{
    public enum ActivationType
    {
        collision,
        endOfLines
    }

    public ActivationType activationType;
    public InteractableNPC npc;
    public InteractableNPC.NPCLines[] newLines;
    public bool updateLinesWhenDone;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "ClientPlayer" && activationType == ActivationType.collision)
        {
            UpdateLines();
        }
    }
    public void UpdateLines()
    {
        // Replace our current NPCLines with our updated ones (newLines)
        npc.lines = newLines;
        npc.updateLinesWhenDone = updateLinesWhenDone;
    }
}
