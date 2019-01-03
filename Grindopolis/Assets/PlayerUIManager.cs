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
    Canvas hudCanvas;

    public Material[] playerColors;

    public GameObject player;
    public Renderer playerBodyRenderer;

    PlayerController pc;
    PlayerLook pl;

    // Start is called before the first frame update
    void Start()
    {
        inputf = GetComponentInChildren<InputField>();
        pc = player.GetComponent<PlayerController>();
        pl = player.GetComponentInChildren<PlayerLook>();
        drop = GetComponentInChildren<Dropdown>();
        hudCanvas = GetComponent<Canvas>();
        hudCanvas.enabled = false;
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

                hudCanvas.enabled = true;
                menuOpen = true;
            }
            else
            {
                inputf.text = playerName;

                pl.enabled = true;
                pc.movementSettings.canMove = true;

                hudCanvas.enabled = false;
                menuOpen = false;
            }
        }

    }
    public void UpdateColor()
    {
        playerColor = drop.value;
    }
    public void UpdateName()
    {
        playerName = inputf.text;
    }
    public void UpdatePlayerInfo()
    {
        // Grab our color and name values and send them to our player network script
        UpdateColor();
        UpdateName();

        player.GetComponent<PlayerController>().CmdUpdatePlayerInfo(playerColor, playerName);
    }
}
