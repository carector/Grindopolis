using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Photon.Pun;

public class FireballScript : MonoBehaviourPunCallbacks
{
    public Rigidbody playerRb;
    public float startSpeed;
    public float explosionRadius = 5;
    public float explosionPower = 15;

    bool explode;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().AddRelativeForce(-startSpeed, 0, 0, ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        if (explode)
        {
            
            StartCoroutine(Countdown());
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<EnemyControl>() != null)
        {
            other.gameObject.GetComponent<EnemyControl>().ReceiveDamage(10);
            if (photonView.IsMine)
                PhotonNetwork.Destroy(this.gameObject);
        }
        explode = true;
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(1);

        if(photonView.IsMine)
            PhotonNetwork.Destroy(this.gameObject);
    }
}
