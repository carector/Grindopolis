using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerNetworkScript : NetworkBehaviour { // Not MonoBehavior holy shit

    public GameObject PlayerControllerPrefab;

    // SyncVars are variables where if their value changes on the SERVER, all clients are automatically informed of the new value
    // SyncVars are convenient, but RPCs are much more explicit and easy to understand
    // Hooks call functions when a value is changed - SYNCVAR WITH HOOK IS THE SAME AS AN RPC
    //[SyncVar(hook ="OnPlayerNameChanged")]

    public GameObject pObject;
    NetworkManagerScript nms;

    [SyncVar]
    public string playerName;

    [SyncVar]
    public int playerMatIndex;

    // Use this for initialization
    void Start() {

        nms = GameObject.Find("NetworkManager").GetComponent<NetworkManagerScript>();

        // Check to make sure this is my OWN LOCAL PlayerConnection OBJECT (important!)
        if (!isLocalPlayer)
        {
            return;
        }

        // Make sure we reset the number of players when we start a new server
        if (isServer)
        {
            nms.numConnectedPlayers = 0;
        }

        // Tell the server to spawn our PlayerController
        CmdSpawnPlayerController();
	}
	
	// Update is called once per frame
	void Update () {

        if (!isLocalPlayer)
        {
            return;
        }
    }

    // COMMANDS //
    //
    // Commands are special functions that ONLY get executed on the server

    [Command] 
    void CmdSpawnPlayerController()
    {
        pObject = Instantiate(PlayerControllerPrefab, new Vector3(Random.Range(-10, 10), 1, Random.Range(-5, -15)), Quaternion.identity);
        pObject.GetComponent<PlayerController>().pns = this;

        // Tell the server to spawn the playercontroller for the player who just joined
        // "Spawn" instantiates an object for ALL CLIENTS
        // SpawnWithClientAuthority tells everyone that one specific client owns their specific player controller
        NetworkServer.SpawnWithClientAuthority(pObject, connectionToClient);
    }

    // RPC //
    //
    // RPCs are special functions that ONLY get executed on the clients
    // Useful for functions, but if its just changing a variable SyncVar is more appropriate

    
}
