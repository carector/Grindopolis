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
            stream.SendNext(vortexEmissionState);
            stream.SendNext(illuminationLight.intensity);
        }
        // Recieve data
        else
        {
            emissionState = (bool)stream.ReceiveNext();
            vortexEmissionState = (bool)stream.ReceiveNext();
            illuminationLight.intensity = (float)stream.ReceiveNext();
        }
    }

    public GameObject staffPosition;
    public Light illuminationLight;

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
    bool vortexEmissionState;
    public ParticleSystem vortexPart;

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
        if (emissionState)
        {
            part.startColor = Color.Lerp(part.startColor, Color.white, 0.4f);
        }
        else
        {
            part.startColor = Color.Lerp(part.startColor, Color.clear, 0.4f);
        }

        if (vortexEmissionState)
        {
            vortexPart.startColor = Color.Lerp(vortexPart.startColor, Color.white, 0.4f);
        }
        else
        {
            vortexPart.startColor = Color.Lerp(vortexPart.startColor, Color.clear, 0.4f);
        }

        if (!photonView.IsMine)
            return;

        if (emissionState)
        {
            staffZPos = 0.35f;
        }
        else
        {
            staffZPos = 0.475f;
        }

        CalculateLookMovement();

        staffZPos = 0.35f;
    }
    public void Illuminate(bool state)
    {
        if(state==true)
        {
            illuminationLight.intensity = Mathf.Lerp(illuminationLight.intensity, 2.5f, 0.25f);
        }
        else
        {
            illuminationLight.intensity = Mathf.Lerp(illuminationLight.intensity, 0, 0.25f);
        }
    }
    public void StaffEmissions(bool state)
    {
        emissionState = state;
    }
    public void VortexEmissions(bool state)
    {
        vortexEmissionState = state;
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
            //transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0.15f);
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
