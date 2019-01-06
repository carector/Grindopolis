using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public bool menuOpen;

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
    

    AudioSource audio;
    PlayerController pc;
    PlayerLook pl;
    RectTransform dialogBg;
    Image dialogNameBg;
    Image crosshair;
    Text line1;
    Text line2;
    Text line3;
    Text lineName;
    Text hintText;

    // Start is called before the first frame update
    void Start()
    {
        line1 = GameObject.Find("DialogLine1").GetComponent<Text>();
        line2 = GameObject.Find("DialogLine2").GetComponent<Text>();
        line3 = GameObject.Find("DialogLine3").GetComponent<Text>();
        hintText = GameObject.Find("HintText").GetComponent<Text>();
        lineName = GameObject.Find("DialogNameLine").GetComponent<Text>();
        dialogBg = GameObject.Find("DialogBG").GetComponent<RectTransform>();
        dialogNameBg = GameObject.Find("DialogNameBG").GetComponent<Image>();
        crosshair = GameObject.Find("CrosshairImage").GetComponent<Image>();

        audio = GetComponent<AudioSource>();
        inputf = GetComponentInChildren<InputField>();
        pc = player.GetComponent<PlayerController>();
        pl = player.GetComponentInChildren<PlayerLook>();
        drop = GetComponentInChildren<Dropdown>();
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
                pl.enabled = false;
                pc.movementSettings.canMove = false;

                menuCanvas.enabled = true;
                menuOpen = true;
            }
            else
            {
                inputf.text = playerName;

                pl.enabled = true;
                pc.movementSettings.canMove = true;

                menuCanvas.enabled = false;
                menuOpen = false;
            }
        }
    }
    public void ResetDialogWindow()
    {
        dialogBg.anchoredPosition = new Vector2(267.8f, -192.1f);
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

    public void DisplayDialog(string name, string a, string b, string c)
    {
        StartCoroutine(DisplayDialogEnum(name, a, b, c));
    }
    IEnumerator DisplayDialogEnum(string name, string a, string b, string c)
    {
        line1.text = "";
        line2.text = "";
        line3.text = "";

        if(name == "")
        {
            dialogNameBg.color = Color.clear;
            lineName.text = "";
        }
        else
        {
            dialogNameBg.color = Color.white;
            lineName.text = name;
        }

        dialogBg.anchoredPosition = new Vector2(267.8f, -192.1f);

        while (dialogBg.anchoredPosition.x >= -265f)
        {
            dialogBg.anchoredPosition = Vector2.Lerp(dialogBg.anchoredPosition, new Vector2(-267.8f, -192.1f), 0.5f);
            yield return null;
        }

        dialogBg.anchoredPosition = new Vector2(-267.8f, -192.1f);

        line1.text = a;
        audio.PlayOneShot(typeSound);
        yield return new WaitForSeconds(0.35f);

        if (b != "")
        {
            line2.text = b;
            audio.PlayOneShot(typeSound);
            yield return new WaitForSeconds(0.35f);
        }

        if (c != "")
        {
            line3.text = c;
            audio.PlayOneShot(typeSound);
            yield return new WaitForSeconds(0.35f);
        }

    }
}
