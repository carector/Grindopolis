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
    public RectTransform[] spellSlots;
    public string[] spellNames;
    public RectTransform sidebar;
    public GameObject player;
    public Renderer playerBodyRenderer;
    public Text spellNameText;
    public Text spellDescText;
    public bool dialogBoxOpen;
    public Text healthText;
    public Text manaText;
    public Text hintText;
    public Text cashText;
    public Image sidebarBackground;
    public Image screenBlackout;

    bool sidebarOpen;
    public int currentSidebarIndex;
    float sidebarOpenTime = 4;

    public float magicMissleCountdown;
    public float healthBubbleCountdown;

    InteractableNPC storedNpc;
    AudioSource audio;
    PlayerControllerRigidbody pc;
    PlayerLook pl;
    RectTransform dialogBg;
    Image dialogNameBg;
    Image crosshair;
    Image pressEIndicator;

    RectTransform cursor;
    Text line1;
    Text line2;
    Text line3;
    Text lineName;

    //Text speedText;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //speedText = GameObject.Find("SpeedText").GetComponent<Text>();
        line1 = GameObject.Find("DialogLine1").GetComponent<Text>();
        line2 = GameObject.Find("DialogLine2").GetComponent<Text>();
        line3 = GameObject.Find("DialogLine3").GetComponent<Text>();
        lineName = GameObject.Find("DialogNameLine").GetComponent<Text>();
        dialogBg = GameObject.Find("DialogBG").GetComponent<RectTransform>();
        dialogNameBg = GameObject.Find("DialogNameBG").GetComponent<Image>();
        crosshair = GameObject.Find("CrosshairImage").GetComponent<Image>();
        cursor = GameObject.Find("Cursor").GetComponent<RectTransform>();

        pressEIndicator = GameObject.Find("PressEIndicator").GetComponent<Image>();
        pressEIndicator.color = Color.clear;

        audio = GetComponent<AudioSource>();
        inputf = GetComponentInChildren<InputField>();
        inputf.enabled = false;
        pc = player.GetComponent<PlayerControllerRigidbody>();
        pl = player.GetComponentInChildren<PlayerLook>();
        drop = GetComponentInChildren<Dropdown>();
        drop.enabled = false;
        menuCanvas = GameObject.Find("MenuCanvas").GetComponent<Canvas>();
        hudCanvas = GameObject.Find("HUDCanvas").GetComponent<Canvas>();

        sidebarBackground.color = Color.clear;
        menuCanvas.enabled = false;

        EnableCrosshair();
    }

    // Update is called once per frame
    void Update()
    {
        // Update our text values
        healthText.text = pc.combatSettings.health.ToString();
        manaText.text = pc.combatSettings.mana.ToString();
        cashText.text = "Cash: " + (Mathf.Round(pc.combatSettings.cash * 100) / 100f).ToString();

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
                Cursor.lockState = CursorLockMode.None;
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
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        // Opens sidebar / spell selection
        if (Input.mouseScrollDelta.y != 0 && !Input.GetMouseButton(0))
        {
            sidebarOpenTime = 4;

            if (!sidebarOpen)
                sidebarOpen = true;

            else
            {
                if (currentSidebarIndex == 0 && Input.mouseScrollDelta.y > 0)
                {
                    currentSidebarIndex = 3;
                }
                else if (currentSidebarIndex == 3 && Input.mouseScrollDelta.y < 0)
                {
                    currentSidebarIndex = 0;
                }
                else
                {
                    currentSidebarIndex -= Mathf.RoundToInt(Input.mouseScrollDelta.y);
                }
            }
        }

        // Close our sidebar after 4 seconds if we don't continue to scroll
        if (sidebarOpen)
        {
            // Update cursor and spell slot positions
            LerpSidebar(true);
            LerpSidebarCursor(currentSidebarIndex);
            LerpSpellSlots(currentSidebarIndex);
            UpdateSpellName(currentSidebarIndex);

            sidebarOpenTime -= Time.deltaTime;

            if (sidebarOpenTime <= 0 || (Input.GetMouseButton(0)))
            {
                sidebarOpen = false;
            }
        }
        else
        {
            LerpSidebar(false);
            spellNameText.text = "";
        }

        UpdateSpellDesc(currentSidebarIndex);

        // Check to see if the player has moved far enough away from the NPC it has just interacted with - this is in order to reset the dialog box
        if (storedNpc != null && Vector3.Distance(pc.transform.position, storedNpc.transform.position) >= storedNpc.GetComponent<NPCAnimations>().minRotateDistance)
        {
            ResetDialogWindow();
        }

    }
    void LerpSidebar(bool active)
    {
        if (active)
        {
            sidebar.anchoredPosition = Vector3.Lerp(sidebar.anchoredPosition, new Vector3(47.1f, sidebar.anchoredPosition.y), 0.5f);
            sidebarBackground.color = Color.Lerp(sidebarBackground.color, new Color(0, 0, 0, 100 / 255f), 0.5f);
        }
        else
        {
            sidebar.anchoredPosition = Vector3.Lerp(sidebar.anchoredPosition, new Vector3(-140, sidebar.anchoredPosition.y), 0.5f);
            sidebarBackground.color = Color.Lerp(sidebarBackground.color, Color.clear, 0.5f);
        }
    }
    void LerpSidebarCursor(int index)
    {
        if (index < 5)
            cursor.anchoredPosition = Vector3.Lerp(cursor.anchoredPosition, new Vector3(cursor.anchoredPosition.x, spellSlots[index].anchoredPosition.y), 0.5f);
    }

    void UpdateSpellName(int index)
    {
        if (index < 5)
            spellNameText.text = spellNames[currentSidebarIndex];
    }

    void UpdateSpellDesc(int index)
    {
        if (magicMissleCountdown > 0)
        {
            magicMissleCountdown -= Time.deltaTime;
        }
        if (healthBubbleCountdown > 0)
        {
            healthBubbleCountdown -= Time.deltaTime;
        }

        if (!sidebarOpen)
        {
            spellDescText.text = "";
            return;
        }

        // Countdowns are automatically set by playerControllerRigidbody

        if (magicMissleCountdown > 0)
        {
            if (index == 0)
            {
                spellDescText.text = "Resting... " + Mathf.RoundToInt(magicMissleCountdown);
                return;
            }
        }

        if (healthBubbleCountdown > 0)
        {
            if (index == 3)
            {
                spellDescText.text = "Resting... " + Mathf.RoundToInt(healthBubbleCountdown);
                return;
            }
        }

        spellDescText.text = "";
    }

    void LerpSpellSlots(int index)
    {
        if (index < 4)
        {
            // For active spellslot, lerp it into view
            spellSlots[index].anchoredPosition = Vector3.Lerp(spellSlots[index].anchoredPosition, new Vector3(cursor.anchoredPosition.x, spellSlots[index].anchoredPosition.y), 0.5f);

            // For all inactive spellslots, lerp them into our sidebar, out of view
            for (int i = 0; i <= 4; i++)
            {
                if (i != index)
                    spellSlots[i].anchoredPosition = Vector3.Lerp(spellSlots[i].anchoredPosition, new Vector3(25, spellSlots[i].anchoredPosition.y), 0.5f);
            }
        }
    }

    public void ResetDialogWindow()
    {
        StopAllCoroutines();
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

        player.GetComponent<PlayerControllerRigidbody>().UpdatePlayerInfo(playerColor, playerName);
    }

    public void EnableCrosshair()
    {
        crosshair.enabled = true;
    }
    public void DisableCrosshair()
    {
        crosshair.enabled = false;
    }

    /*
    public void DisplaySpeed(float x, float z)
    {
        speedText.text = x + ", " + z;
    }
    */

    public void DisplayDialog(InteractableNPC npc)
    {
        StartCoroutine(DisplayDialogLine(npc, 0));
    }
    IEnumerator DisplayDialogLine(InteractableNPC npc, int lineIndex)
    {
        pressEIndicator.color = Color.clear;
        dialogBoxOpen = true;
        storedNpc = npc;

        line1.text = "";
        line2.text = "";
        line3.text = "";

        if (npc.npcName == "")
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
        if (lineIndex == 0)
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

            // Update all lines referenced in our NPC
            if (npc.updateLinesWhenDone)
            {
                foreach (DisplayNewLines l in npc.linesToUpdate)
                {
                    l.UpdateLines();
                }
            }

            dialogBg.anchoredPosition = new Vector2(267.8f, -192.1f);
            dialogBoxOpen = false;
        }
    }

    IEnumerator WaitForKeyPress()
    {
        while (!Input.GetKeyDown(KeyCode.E))
        {
            yield return null;
        }
    }

    public void LeaveGame()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().LeaveRoom();
    }
}
