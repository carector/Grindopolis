using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HealthBubbleActual : MonoBehaviourPunCallbacks, IPunObservable
{
    public float time;

    AudioSource audio;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Send data
        if (stream.IsWriting)
        {
            stream.SendNext(time);
        }
        else
        {
            time = (float)stream.ReceiveNext();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
        StartCoroutine(RestoreHealth());
    }

    // Update is called once per frame
    void Update()
    {

        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(11, 11, 11), 0.5f);

        time -= Time.deltaTime;

        if (time <= 0)
        {

            audio.volume = Mathf.Lerp(audio.volume, 0, 0.1f);

            if (audio.volume <= 0.1f)
            {
                StopAllCoroutines();
                PhotonNetwork.Destroy(this.gameObject);
            }
        }
        else
        {
            audio.volume = Mathf.Lerp(audio.volume, 1, 0.25f);
        }
    }

    IEnumerator RestoreHealth()
    {
        yield return new WaitForSeconds(0.35f/2f);

        Collider[] col = Physics.OverlapSphere(transform.position, 4.5f);

        foreach(Collider c in col)
        {
            if(c.gameObject.tag == "Player" && c.GetComponent<PhotonView>().IsMine)
            {
                // Check to make sure our player is alive...
                if (c.GetComponent<PlayerControllerRigidbody>().combatSettings.health > 0)
                {
                    if (c.GetComponent<PlayerControllerRigidbody>().combatSettings.health + 2 > 100)
                    {
                        c.GetComponent<PlayerControllerRigidbody>().combatSettings.health = 100;
                    }
                    else
                    {
                        c.GetComponent<PlayerControllerRigidbody>().combatSettings.health += 2;
                    }
                }
            }
        }

        StartCoroutine(RestoreHealth());
    }
}
