using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddFunds : MonoBehaviour
{
    public float amount;
    public InteractableNPC npc;
    public string lineToCheckFor;
    public AudioClip cashSound;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Script checks to see if NPC's lines have been changed.
        // If the first line is equal to the line we're looking for, 
        // we add funds because we know the player has interacted with the NPC.
        if (npc.lines[0].line1 == lineToCheckFor)
        {
            GameObject.Find("ClientPlayer").GetComponent<PlayerControllerRigidbody>().combatSettings.cash += amount;
            GetComponent<AudioSource>().PlayOneShot(cashSound);
            this.enabled = false;
        }
    }
}
