using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AnnouncementsScript : MonoBehaviourPunCallbacks
{
    public AudioClip welcomeSound;
    public AudioClip[] joinedSounds;
    int storedPlayerCount;

    AudioSource audio;

    // Start is called before the first frame update
    void Awake()
    {
        audio = GetComponent<AudioSource>();
        storedPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
    }

    public void RpcWelcomeSound()
    {
        audio.PlayOneShot(welcomeSound);
    }

    public void FixedUpdate()
    {
        if (storedPlayerCount < PhotonNetwork.CurrentRoom.PlayerCount)
        {
            audio.PlayOneShot(joinedSounds[Random.Range(0, joinedSounds.Length)]);
            storedPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        }
    }

    private void OnPlayerConnected()
    {
        print("PLAYA JOINED");
    }
}
