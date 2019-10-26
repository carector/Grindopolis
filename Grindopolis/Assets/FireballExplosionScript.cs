using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FireballExplosionScript : MonoBehaviourPunCallbacks
{
    public int damage;

    private void Start()
    {
        StartCoroutine(Countdown());
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<EnemyControl>() != null)
        {
            EnemyControl e = other.gameObject.GetComponent<EnemyControl>();
            e.ReceiveDamage(damage, this.transform);
        }
        else if (other.gameObject.GetComponent<PlayerControllerRigidbody>() != null)
        {
            PlayerControllerRigidbody p = other.gameObject.GetComponent<PlayerControllerRigidbody>();
            if (other.name != "ClientPlayer")
                p.ReceiveDamage(damage);

            p.GetComponent<Rigidbody>().AddExplosionForce(500, transform.position, 100);
        }
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(0.5f);

        if (photonView.IsMine)
            PhotonNetwork.Destroy(this.gameObject);
    }
}
