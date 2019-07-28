using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HealthBubbleSpell : MonoBehaviourPunCallbacks
{
    Rigidbody rb;
    public float startSpeed;
    public GameObject bubble;
    bool createBubble;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddRelativeForce(-startSpeed, 0, 0, ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        if (createBubble && Vector3.Distance(Vector3.zero, rb.velocity) <= 1f)
        {
            StartCoroutine(Countdown());
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        //if(playerRb.gameObject != null && other.gameObject != playerRb.gameObject)
        if(other.gameObject.tag == "Terrain")
            createBubble = true;
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(0.1f);

        if (photonView.IsMine)
        {
            PhotonNetwork.Instantiate(bubble.name, transform.position, transform.rotation);
            PhotonNetwork.Destroy(this.gameObject);
        }
    }
}
