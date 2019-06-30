using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StaffAnimate : MonoBehaviourPunCallbacks, IPunObservable
{
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Send data
        if (stream.IsWriting)
        {
            stream.SendNext(emissionState);
        }
        // Recieve data
        else
        {
            emissionState = (bool)stream.ReceiveNext();
        }
    }

    public GameObject staffPosition;

    PlayerControllerRigidbody pcr;
    PlayerLook pLook;
    Vector3 startingPosition;
    Vector3 camMovementX;
    Vector3 camMovementY;

    ParticleSystem part;
    Light lgt;

    float staffZPos;
    bool paused;
    bool emissionState;

    // Start is called before the first frame update
    void Start()
    {
        pcr = GetComponentInParent<PlayerControllerRigidbody>();
        pLook = pcr.GetComponentInChildren<PlayerLook>();
        part = GetComponentInChildren<ParticleSystem>();
        lgt = part.GetComponent<Light>();
        startingPosition = transform.localPosition;

        camMovementX = pcr.transform.rotation.eulerAngles;
        camMovementY = pLook.transform.rotation.eulerAngles;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        part.enableEmission = emissionState;

        if (!photonView.IsMine)
            return;

        CalculateLookMovement();

        // Particle calculations
        if (Input.GetMouseButton(0) && !paused)
        {
            emissionState = true;
            lgt.range = Mathf.Lerp(lgt.range, 10f, 0.25f);
            staffZPos = 0.35f;
        }
        else
        {
            emissionState = false;
            lgt.range = Mathf.Lerp(lgt.range, 0, 0.25f);
            staffZPos = 0.475f;
        }
    }
    private void Update()
    {
        if (!photonView.IsMine)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!paused)
            {
                paused = true;
            }
            else
            {
                paused = false;
            }
        }

        if (Input.GetMouseButtonUp(0) && !paused)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0.15f);
        }
    }

    // How much to "drag" the staff behind by when the player looks around
    void CalculateLookMovement()
    {
        Vector3 previousCamMovementX = camMovementX;
        Vector3 previousCamMovementY = camMovementY;
        camMovementX = pcr.transform.rotation.eulerAngles;
        camMovementY = pLook.transform.rotation.eulerAngles;

        transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(startingPosition.x - Mathf.Clamp(((camMovementX.y - previousCamMovementX.y) * 0.2f), -0.2f, 0.2f), startingPosition.y + Mathf.Clamp(((camMovementY.x - previousCamMovementY.x) * 0.2f), -0.2f, 0.2f), staffZPos), 0.1f);
        //transform.rotation = Quaternion.Lerp(transform.rotation, staffPosition.transform.rotation, 0.4f);
    }
}
