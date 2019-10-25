using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;

public class MenuManager : MonoBehaviourPunCallbacks, ILobbyCallbacks
{
    // The screen that's being displayed
    // 0 = top menu
    // 1 = Game creator
    // 2 = Server list
    // 3 = Options
    // 4 = Quit confirmation
    // 5 = Loading screen

    int screen;

    bool isConnecting;
    public bool isHosting;
    public bool isJoining;
    string gameVersion = "1"; // Used to prevent version mismatches

    [System.Serializable]
    public class ServerSettings
    {
        public string serverName;
        public byte serverPlayerCount;
    }

    public ServerSettings customServerSettings;
    public InputField nameInput;
    public InputField maxPlayerInput;

    public RectTransform serverCreator;
    public RectTransform serverList;
    public RectTransform options;
    public RectTransform quitConfirmation;
    public RectTransform serverMenu;

    public RectTransform hostButton;
    public RectTransform joinButton;
    public RectTransform optionsButton;
    public RectTransform quitButton;
    public RectTransform title;
    public RectTransform loadingText;
    public Text loadingTextElement;

    public RectTransform screenCenter;
    public RectTransform belowPos;
    public RectTransform abovePos;
    public RectTransform leftPos;
    public RectTransform rightPos;

    // Room list information
    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // Causes all clients to sync the loaded level with the master client
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void FixedUpdate()
    {
        // Top menu control
        if(screen == 0)
        {
            LerpToPos(hostButton, new Vector2(0, 143));
            LerpToPos(joinButton, new Vector2(0, -16));
            LerpToPos(optionsButton, new Vector2(0, -175));
            LerpToPos(quitButton, new Vector2(0, -334));
        }
        else
        {
            LerpLeft(hostButton);
            LerpLeft(joinButton);
            LerpLeft(optionsButton);
            LerpLeft(quitButton);
        }

        // Server Creation control
        if (screen == 1)
        {
            LerpCenter(serverCreator);
        }
        else if (screen == 5)
        {
            LerpAbove(serverCreator);
        }
        else
        {
            LerpRight(serverCreator);
        }

        // Server list control
        if(screen == 2)
        {
            LerpCenter(serverList);
        }
        else if (screen == 5)
        {
            LerpAbove(serverCreator);
        }
        else
        {
            LerpRight(serverList);
        }

        // Options control
        if(screen == 3)
        {
            LerpCenter(options);
        }
        else
        {
            LerpRight(options);
        }

        // Quit confirmation control
        if (screen == 4)
        {
            LerpCenter(quitConfirmation);
        }
        else
        {
            LerpRight(quitConfirmation);
        }

        // Loading screen control
        if(screen == 5)
        {
            LerpCenter(loadingText);
        }
        else
        {
            LerpBelow(loadingText);
        }

        // Multiplayer screen control
        if (screen == 6)
        {
            LerpCenter(serverMenu);
        }
        else if(screen == 1)
        {
            LerpLeft(serverMenu);
        }
        else
        {
            LerpRight(serverMenu);
        }
    }

    void LerpBelow (RectTransform rect)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, belowPos.anchoredPosition, 0.15f);
    }
    void LerpAbove(RectTransform rect)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, abovePos.anchoredPosition, 0.05f);
    }
    void LerpCenter(RectTransform rect)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, screenCenter.anchoredPosition, 0.25f);
    }
    void LerpLeft(RectTransform rect)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, leftPos.anchoredPosition, 0.25f);
    }
    void LerpRight(RectTransform rect)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, rightPos.anchoredPosition, 0.25f);
    }
    void LerpToPos(RectTransform rect, Vector2 pos)
    {
        rect.anchoredPosition = Vector3.Lerp(rect.anchoredPosition, pos, 0.25f);
    }

    public void ViewTopMenu()
    {
        screen = 0;
    }
    public void ViewServerMaker()
    {
        screen = 1;
    }
    public void ViewServerList()
    {
        screen = 2;
    }
    public void ViewOptions()
    {
        screen = 3;
    }
    public void ViewQuitConfirmation()
    {
        screen = 4;
    }
    public void ViewLoadingScreen()
    {
        screen = 5;
    }
    public void ViewServerMenu()
    {
        screen = 6;
    }

    public void ConnectSingleplayer()
    {
        loadingTextElement.text = "Entering Grindopolis...";
        SceneManager.LoadSceneAsync(2);
    }

    // Joins the lobby so we can see rooms
    public void OnRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    // Clears room list 
    private void ClearRoomListView()
    {
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        roomListEntries.Clear();
    }

    // Retrieves room listings
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {

    }
    


    public void HostGame()
    {
        isHosting = true;
        Connect();
    }

    public void JoinGame()
    {
        isJoining = true;
        Connect();
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
    public void CreateRoom()
    {

    }
    public override void OnConnectedToMaster()
    {
        print("Connected to master");

        if (isConnecting && isJoining) // Check to see if we're attempting to connect - if this isn't here, we load into a random room automatically after we've left another game (cause we're already connected)
            PhotonNetwork.JoinRandomRoom();

        else if(isConnecting && isHosting)
        {
            PhotonNetwork.CreateRoom(customServerSettings.serverName, new RoomOptions { MaxPlayers = customServerSettings.serverPlayerCount });
        }
            
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        print("fella left :(");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // Exit to main screen with error message
    }

    public override void OnJoinedRoom()
    {
        print("we're in.");

        PhotonNetwork.LoadLevel("MultiplayerArena");

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void UpdateServerName()
    {
        customServerSettings.serverName = nameInput.text;
    }
    public void UpdateServerMaxPlayers()
    {
        customServerSettings.serverPlayerCount = byte.Parse(maxPlayerInput.text);
    }
}
