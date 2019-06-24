using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;


public class PlayerNameInputField : MonoBehaviour
{
    const string playerNamePrefKey = "PlayerName"; // Stored in playerPrefs so name is remembered thru sessions

    // Start is called before the first frame update
    void Start()
    {
        string defaultName = string.Empty;
        InputField inf = this.GetComponent<InputField>();

        if(PlayerPrefs.HasKey(playerNamePrefKey))
        {
            // Retrieve our player's name from playerprefs if we have stored it there
            defaultName = PlayerPrefs.GetString(playerNamePrefKey);
            inf.text = defaultName;
        }

        PhotonNetwork.NickName = defaultName;
        
    }

    public void SetPlayerName(string value)
    {
        if(string.IsNullOrEmpty(value))
        {
            return;
        }

        PhotonNetwork.NickName = value;

        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
}
