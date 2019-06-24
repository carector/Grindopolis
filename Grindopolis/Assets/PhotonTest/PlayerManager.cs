using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject beams;
    public float health = 1f;

    bool isFiring;

    private void Awake()
    {
        if (beams == null)
        {

        }
        else
        {
            beams.SetActive(false); // Make sure the beams aren't visible when the game starts
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            ProcessInputs(); // Only process inputs if we're the client player
        }

        if(beams != null && isFiring != beams.activeSelf)
        {
            beams.SetActive(isFiring);
        }

        if(health <= 0f)
        {
            GameManager.instance.LeaveRoom();
        }
    }

    void ProcessInputs()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isFiring)
            {
                isFiring = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isFiring)
            {
                isFiring = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!photonView.IsMine)
        {
            return; // Ignore beam collider if the beams don't belong to the local player
        }

        if(!other.name.Contains("Beam"))
        {
            return;
        }

        health -= 0.1f * Time.deltaTime;
    }
}
