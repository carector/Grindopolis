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
            this.GetComponent<Rigidbody>().isKinematic = true;
            Vector3 explosionPos = transform.position;

            Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
            foreach (Collider hit in colliders)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();

                if (rb != null && rb != this.GetComponent<Rigidbody>() && rb != playerRb)
                    rb.AddExplosionForce(explosionPower, explosionPos, explosionRadius, 1f);
            }
            StartCoroutine(Countdown());
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        //if(playerRb.gameObject != null && other.gameObject != playerRb.gameObject)
        explode = true;
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(0.35f);

        if(photonView.IsMine)
            PhotonNetwork.Destroy(this.gameObject);
    }
}
