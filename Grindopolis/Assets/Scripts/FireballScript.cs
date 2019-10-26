using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Photon.Pun;

public class FireballScript : MonoBehaviourPunCallbacks
{
    public GameObject player;
    public float startSpeed;
    public float explosionPower = 15;
    bool explode;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Countdown());
        GetComponent<Rigidbody>().AddRelativeForce(-startSpeed, 0, 0, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (photonView.IsMine && other.name != "ClientPlayer")
        {
            PhotonNetwork.Instantiate("ProjectileExplosion", transform.position, transform.rotation);
            PhotonNetwork.Destroy(this.gameObject);
        }
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(5);

        if(photonView.IsMine)
            PhotonNetwork.Destroy(this.gameObject);
    }
}
