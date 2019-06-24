using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks // Allows us to use more pun-related methods
{
    bool isConnecting;
    string gameVersion = "1"; // Used to prevent version mismatches

    [SerializeField]
    private byte maxPlayersPerRoom = 4;

    [SerializeField]
    private GameObject controlPanel; // Panel that holds player name field and connect button

    [SerializeField]
    private GameObject progressLabel; // Text that tells us if we're connecting

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // Causes all clients to sync the loaded level with the master client
    }

    // Start is called before the first frame update
    void Start()
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
    }



    public void Connect()
    {
        isConnecting = true; 

        progressLabel.SetActive(true);
        controlPanel.SetActive(false);

        // If we are connected, join a random room
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }

        // If we aren't connected, try reconnecting - but search for games running the same version we are
        else
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        print("hell ya. am connected.");

        if(isConnecting) // Check to see if we're attempting to connect - if this isn't here, we load into a random room automatically after we've left another game (cause we're already connected)
            PhotonNetwork.JoinRandomRoom();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        print("fella left :(");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        print("can't join. making room");

        PhotonNetwork.CreateRoom("poopyhead", new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        print("we're in.");

        // Load the room level
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            PhotonNetwork.LoadLevel("Room for 1");
        }
    }
}
