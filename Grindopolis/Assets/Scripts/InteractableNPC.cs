using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableNPC : MonoBehaviour
{
    // Stores all NPC lines in our dialog
    [System.Serializable]
    public class NPCLines
    {
        public string line1;
        public string line2;
        public string line3;

        public string altLine1;
        public string altLine2;
        public string altLine3;
    }

    public string npcName;
    public bool revertToOriginalLines = true;
    public NPCLines[] lines;
    

    // Start is called before the first frame update
    void Start()
    {
        /*
        storedLine1 = line1;
        storedLine2 = line2;
        storedLine3 = line3;
        */
    }
}
