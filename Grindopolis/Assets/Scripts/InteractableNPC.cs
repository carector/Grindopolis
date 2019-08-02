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
    }

    public string npcName;
    public bool revertToOriginalLines = true;
    public bool updateLinesWhenDone = true;
    public NPCLines[] lines;
    public DisplayNewLines[] linesToUpdate;

    NPCLines[] storedLines;

    // Start is called before the first frame update
    void Start()
    {
        storedLines = lines;
    }
}
