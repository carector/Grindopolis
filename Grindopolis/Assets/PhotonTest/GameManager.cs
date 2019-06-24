using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    private void Start()
    {
        instance = this;
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0); // Load our lobby scene
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient) // If we aren't the player who started the game
        {
            Debug.LogError("PhotonNetwork: shit myself ouch");
        }

        PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount); // Loads a room depending on the number of players in game
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        if(PhotonNetwork.IsMasterClient) // Arena is only loaded if the master client enters the room - causes arena to load across all clients
        {
            LoadArena();
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


            LoadArena();
        }
    }
}
