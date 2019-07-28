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
                PhotonNetwork.Destroy(this.gameObject);
            }
        }
        else
        {
            audio.volume = Mathf.Lerp(audio.volume, 1, 0.25f);
        }
    }
}
