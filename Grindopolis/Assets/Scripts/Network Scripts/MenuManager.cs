using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class MenuManager : MonoBehaviourPunCallbacks
{
    bool isConnecting;
    string gameVersion = "1"; // Used to prevent version mismatches

    [SerializeField]
    private byte maxPlayersPerRoom = 4;

    public RectTransform enterButton;
    public RectTransform optionsButton;
    public RectTransform quitButton;
    public RectTransform title;
    public RectTransform loadingText;
    public RectTransform screenCenter;
    public RectTransform inactivePos;
    public RectTransform inactiveTitlePos;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // Causes all clients to sync the loaded level with the master client
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    private void FixedUpdate()
    {
        if(isConnecting)
        {
            LerpBelow(enterButton);
            LerpBelow(quitButton);
            LerpBelow(optionsButton);
            LerpAbove(title);
            LerpCenter(loadingText);
        }
    }

    void LerpBelow (RectTransform rect)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, inactivePos.anchoredPosition, 0.15f);
    }
    void LerpAbove(RectTransform rect)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, inactiveTitlePos.anchoredPosition, 0.05f);
    }
    void LerpCenter(RectTransform rect)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, screenCenter.anchoredPosition, 0.25f);
    }

    public void Connect()
    {
        isConnecting = true;

        print(PhotonNetwork.CountOfRooms);

        // If we are connected, join a random room
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }

        // If we aren't connected, try reconnecting - but search for games running the same version we are
        else
        {
            if (PhotonNetwork.CountOfRooms == 0)
            {
                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
            else
            {
                Application.LoadLevel(Application.loadedLevel);
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        print("Connected to master");

        if (isConnecting) // Check to see if we're attempting to connect - if this isn't here, we load into a random room automatically after we've left another game (cause we're already connected)
            PhotonNetwork.JoinRandomRoom();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        print("fella left :(");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        print("can't join. making room");

        PhotonNetwork.CreateRoom("Grindworld", new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        print("we're in.");

        PhotonNetwork.LoadLevel("GrindworldPhoton");

    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
