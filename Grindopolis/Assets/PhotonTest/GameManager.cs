using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    [SerializeField]
    private GameObject playerPrefab;

    private void Start()
    {
        instance = this;

        if(playerPrefab != null)
        {
            if(PlayerControllerRigidbody.LocalPlayerInstance == null) // Will be true by default if the scene we loaded into already has a player in it, which tells us we need to create a player
            {
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(Random.Range(-10, 10), 1.2f, Random.Range(-5, -15)), Quaternion.identity, 0);
            }
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0); // Load our lobby scene
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    // current problem source
    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient) // If we aren't the player who started the game
        {
            Debug.LogError("PhotonNetwork: shit myself ouch");
        }

        //PhotonNetwork.LoadLevel("GrindworldPhoton");
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
        if (other.IsMasterClient)
        {
            LeaveRoom();
        }
    }
}
