using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public bool menuOpen;
    public bool talkingToNPC;
    public string playerName;

    public int playerColor;

    Dropdown drop;
    InputField inputf;
    public Canvas menuCanvas;
    public Canvas hudCanvas;

    public AudioClip typeSound;
    public Material[] playerColors;

    public GameObject player;
    public Renderer playerBodyRenderer;

    public bool dialogBoxOpen;

    InteractableNPC storedNpc;
    AudioSource audio;
    PlayerController pc;
    PlayerLook pl;
    RectTransform dialogBg;
    Image dialogNameBg;
    Image crosshair;
    Image pressEIndicator;
    Text line1;
    Text line2;
    Text line3;
    Text lineName;
    Text hintText;
    

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;

        line1 = GameObject.Find("DialogLine1").GetComponent<Text>();
        line2 = GameObject.Find("DialogLine2").GetComponent<Text>();
        line3 = GameObject.Find("DialogLine3").GetComponent<Text>();
        hintText = GameObject.Find("HintText").GetComponent<Text>();
        lineName = GameObject.Find("DialogNameLine").GetComponent<Text>();
        dialogBg = GameObject.Find("DialogBG").GetComponent<RectTransform>();
        dialogNameBg = GameObject.Find("DialogNameBG").GetComponent<Image>();
        crosshair = GameObject.Find("CrosshairImage").GetComponent<Image>();
        pressEIndicator = GameObject.Find("PressEIndicator").GetComponent<Image>();
        pressEIndicator.color = Color.clear;

        audio = GetComponent<AudioSource>();
        inputf = GetComponentInChildren<InputField>();
        inputf.enabled = false;
        pc = player.GetComponent<PlayerController>();
        pl = player.GetComponentInChildren<PlayerLook>();
        drop = GetComponentInChildren<Dropdown>();
        drop.enabled = false;
        menuCanvas = GameObject.Find("MenuCanvas").GetComponent<Canvas>();
        hudCanvas = GameObject.Find("HUDCanvas").GetComponent<Canvas>();
        menuCanvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Opens and closes options menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!menuOpen)
            {
                inputf.enabled = true;
                drop.enabled = true;

                pl.enabled = false;
                pc.movementSettings.canMove = false;

                menuCanvas.enabled = true;
                menuOpen = true;
                Cursor.visible = true;
            }
            else
            {
                inputf.enabled = false;
                drop.enabled = false;

                inputf.text = playerName;

                pl.enabled = true;
                pc.movementSettings.canMove = true;

                menuCanvas.enabled = false;
                menuOpen = false;
                Cursor.visible = false;
            }
        }

        // Check to see if the player has moved far enough away from the NPC it has just interacted with - this is in order to reset the dialog box
        if (storedNpc != null && Vector3.Distance(pc.transform.position, storedNpc.transform.position) >= 10)
        {
            ResetDialogWindow();
        }

    }
    public void ResetDialogWindow()
    {
        dialogBg.anchoredPosition = new Vector2(267.8f, -192.1f);
        dialogBoxOpen = false;
    }
    public void UpdateColor()
    {
        playerColor = drop.value;
    }
    public void UpdateName()
    {
        playerName = inputf.text;
    }

    public void DisplayHintText(string t)
    {
        hintText.text = t;
    }

    public void UpdatePlayerInfo()
    {
        // Grab our color and name values and send them to our player network script
        UpdateColor();
        UpdateName();

        player.GetComponent<PlayerController>().CmdUpdatePlayerInfo(playerColor, playerName);
    }

    public void EnableCrosshair()
    {
        crosshair.enabled = true;
    }
    public void DisableCrosshair()
    {
        crosshair.enabled = false;
    }

    public void DisplayDialog(InteractableNPC npc)
    {
        StartCoroutine(DisplayDialogLine(npc, 0));
    }
    IEnumerator DisplayDialogLine(InteractableNPC npc, int lineIndex)
    {
        dialogBoxOpen = true;
        storedNpc = npc;

        line1.text = "";
        line2.text = "";
        line3.text = "";

        if(npc.npcName == "")
        {
            dialogNameBg.color = Color.clear;
            lineName.text = "";
        }
        else
        {
            dialogNameBg.color = Color.white;
            lineName.text = npc.npcName;
        }

        // Don't hide the dialog box it's already showing - used for when we have multiple lines in our line array
        if(dialogBg.anchoredPosition.x <= -265)
            dialogBg.anchoredPosition = new Vector2(267.8f, -192.1f);

        while (dialogBg.anchoredPosition.x >= -265f)
        {
            dialogBg.anchoredPosition = Vector2.Lerp(dialogBg.anchoredPosition, new Vector2(-267.8f, -192.1f), 0.5f);
            yield return null;
        }

        dialogBg.anchoredPosition = new Vector2(-267.8f, -192.1f);

        // Display the first line of the current NPCLines
        line1.text = npc.lines[lineIndex].line1;
        audio.PlayOneShot(typeSound);
        yield return new WaitForSeconds(0.25f);

        if (npc.lines[lineIndex].line2 != "")
        {
            line2.text = npc.lines[lineIndex].line2;
            audio.PlayOneShot(typeSound);
            yield return new WaitForSeconds(0.25f);
        }

        if (npc.lines[lineIndex].line3 != "")
        {
            line3.text = npc.lines[lineIndex].line3;
            audio.PlayOneShot(typeSound);
            yield return new WaitForSeconds(0.25f);
        }

        // Wait for the player to press the E key
        pressEIndicator.color = Color.white;
        yield return WaitForKeyPress();
        pressEIndicator.color = Color.clear;

        // If there are additional NPCLines in our array, call DisplayDialogLine again
        if (lineIndex < npc.lines.Length - 1)
        {
            StartCoroutine(DisplayDialogLine(npc, lineIndex + 1));
        }

        // Otherwise, just hide the dialog from the screen
        else
        {
            while (dialogBg.anchoredPosition.x <= 265f)
            {
                dialogBg.anchoredPosition = Vector2.Lerp(dialogBg.anchoredPosition, new Vector2(267.8f, -192.1f), 0.5f);
                yield return null;
            }

            dialogBg.anchoredPosition = new Vector2(267.8f, -192.1f);
            dialogBoxOpen = false;
        }
    }

    IEnumerator WaitForKeyPress()
    {
        while(!Input.GetKeyDown(KeyCode.E))
        {
            yield return null;
        }
    }
}
