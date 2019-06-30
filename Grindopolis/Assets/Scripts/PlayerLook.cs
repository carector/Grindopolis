using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class PlayerLook : MonoBehaviourPunCallbacks
{
    // Reference to player
    public Transform playerBody;
    public float mouseSensitivity;
    public bool lockCursor;

    PlayerControllerRigidbody parentMove;
    float xAxisClamp = 0.0f;

    // Awake is called when the gameObject is activated in-game
    void Awake()
    {
        // Lock our cursor so it doesn't move to other parts of the screen while we're playing
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void Start()
    {
        parentMove = GetComponentInParent<PlayerControllerRigidbody>();
    }

    // Each frame, update the camera's rotation
    void Update()
    {
        // Make sure this is our client's player
        if (!parentMove.photonView.IsMine)
            return;

        RotateCamera();
    }

    void RotateCamera()
    {
        // Get references to our mouse's X and Y positions
        float lookX = Input.GetAxis("Look X");
        float lookY = Input.GetAxis("Look Y");

        // Multiply mouse positions by mouse sensitivity
        float rotAmountX = lookX * mouseSensitivity;
        float rotAmountY = lookY * mouseSensitivity;

        // Subtract our rotation amount in the y axis from our clamp value (more on this below)
        xAxisClamp -= rotAmountY;

        // Create directional values we can send to our camera so we can rotate it
        Vector3 targetRotCam = transform.rotation.eulerAngles;
        Vector3 targetRotBody = playerBody.rotation.eulerAngles;

        // Now subtract our mouse's change in position from our directional values

        targetRotCam.x -= rotAmountY;
        targetRotCam.z = 0;
        targetRotBody.y += rotAmountX;

        // Clamp the rotation in the X axis so our player can't accidentally look backwards and upside down
        if (xAxisClamp > 90)
        {
            xAxisClamp = 90;
            targetRotCam.x = 90;
        }
        else if (xAxisClamp < -90)
        {
            xAxisClamp = -90;
            targetRotCam.x = 270;
        }

        // Finally, rotate our player
        transform.rotation = Quaternion.Euler(targetRotCam);
        playerBody.rotation = Quaternion.Euler(targetRotBody);


    }
}
