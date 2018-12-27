using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

// thanks to Acacia Developer on youtube for providing the tutorial used to make the base for this

public class PlayerMove : NetworkBehaviour
{
    // Public vars
    public bool isGrounded;
    public bool canDoubleJump;
    public bool isOnLadder;
    public float gravity = 1;
    public float maxFallSpeed = 100;
    public float jumpForce;
    public float runSpeed;
    public float walkSpeed;
    public float slopeMax;
    public GameObject onPlatform;
    public float fallingSpeed;


    // Private vars
    private GameObject storedPlatform;

    // Vars
    bool jumping; // Used for the first frame of when the player jumps - prevents charControl.isGrounded from overwriting value before charControl.Move() is called
    bool hasDoubleJumped;
    float remainingJump;

    Camera cam;
    CharacterController charControl;
    NetworkManagerScript nms;
    PlayerSounds pSounds;
    Vector3 moveDirHoriz;
    Vector3 moveDirVert;
    Vector3 moveDirUp;



    // Use this for initialization
    void Start()
    {
        nms = GameObject.Find("NetworkManager").GetComponent<NetworkManagerScript>();
        charControl = GetComponent<CharacterController>();
        pSounds = GetComponentInChildren<PlayerSounds>();

        // Make our jump and move values smaller so you don't have to enter miniscule values in the editor - it's stupid but it's not like anyone else is gonna find this (or will they? °.√•)
        walkSpeed /= 30;
        runSpeed /= 30;
        jumpForce /= 30;

        // Disable our camera if this is not our client player, as well as our audiolistener
        // if the player is the server, for whatever reason the camera won't disable due to the player not having authority
        // to bypass this, the server also checks to make sure there's at least one player connected before disabling the camera, in this case the server (i know this is fucking ASS but just roll with it)
        if (!hasAuthority && nms.numConnectedPlayers != 0)
        {
            GetComponentInChildren<AudioListener>().enabled = false;
            GetComponentInChildren<Camera>().enabled = false;

            return;
        }

        nms.numConnectedPlayers++;
    }
    void OnEnable()
    {
        // Reset our rotation each time the player is enabled. This can fix problems that can occur when the player is above or below an NPC when they begin talking to it
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
    }

    // Input and jumping controlled via Update so nothing is missed
    void Update()
    {
        // Make sure this is our client's player
        if (!hasAuthority)
        {
            return;
        }

        isGrounded = charControl.isGrounded;

        if (Input.GetButtonDown("Jump"))
        {
            if (charControl.isGrounded)
            {
                Jump(false);
            }
            else if (!hasDoubleJumped)
            {
                Jump(true);
                fallingSpeed = 0; // Reset stored falling speed to 0
                hasDoubleJumped = true;
            }
        }

        // Also check to see if we should receive falling damage
        // Increase falling speed if our current velocity is greater than what we have stored
        if (charControl.velocity.y < fallingSpeed)
        {
            fallingSpeed = charControl.velocity.y;
        }

        // If we double jump, be sure to reset our falling speed

        // Play "hit ground" sound when hitting ground
        if (isGrounded)
        {
            if (fallingSpeed <= -35 && fallingSpeed >= -50)
            {
                // Heavier damage sound
            }
        }

    }

    // Movement calculation and physics are controlled with FixedUpdate
    private void FixedUpdate()
    {
        // Make sure this is our client's player
        if (!hasAuthority)
            return;

        // If we're sprinting, move the player with our sprint speed.
        if (Input.GetButton("Sprint"))
            MovePlayer(runSpeed);
        // Otherwise, move with the walk speed.
        else
            MovePlayer(walkSpeed);
    }
    void Jump(bool isDoubleJump)
    {
        if (!isDoubleJump)
        {
            jumping = true;
            moveDirUp.y = jumpForce;
            pSounds.PlayJumpSound();
        }
        else if (canDoubleJump)
        {
            moveDirUp.y = jumpForce * 1.25f;
            pSounds.PlayDoubleJumpSound();
        }

    }
    void MovePlayer(float speed)
    {
        // Get references to our movement axes - X and Z
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");


        // horizontal movement and vertical movement are set up like this because PlayerLook modifies player rotation
        moveDirHoriz = transform.right * horizontal * speed;
        moveDirVert = transform.forward * vertical * speed;

        // Different if we're standing on a ladder
        if (isOnLadder)
        {
            moveDirUp = transform.up * vertical * speed;

            // Clamp vertical at 0 so we don't fall down the ladder
            if (vertical <= 0)
            {
                moveDirVert = Vector3.zero;
            }
        }


        // If we're grounded and the player hasn't just pressed the jump key...
        if ((charControl.isGrounded && !jumping && !isOnLadder))
        {
            // Call SlopeCheck to see if we're standing on a sloping surface
            if (SlopeCheck())
            {
                // Set our downward movement speed to equal to gravity if we're standing on a slope. This way, we won't end up "skipping" from having our downwards force be too low.
                moveDirUp.y = -gravity;
            }
            else
            {
                // Set our downward movement speed to be almost insignificant, but still enough that we stay grounded. This way, we won't have a large downwards speed the instant we fall off a platform.
                moveDirUp.y = -0.01f;
            }

            // Reset hasDoubleJumped once we're grounded
            hasDoubleJumped = false;

        }
        // Otherwise, subtract gravity from our downward speed
        else if (!isOnLadder)
        {
            // Subtract gravity from our downward speed
            if (moveDirUp.y <= maxFallSpeed)
            {
                moveDirUp.y -= gravity * Time.deltaTime;
            }
            jumping = false;
        }
        if (onPlatform != null)
        {
            if (storedPlatform != onPlatform)
            {
                // For some reason, reloading the PlayerMove script will sort of reset the Character Controller's frame of reference, and it will move with playforms
                // If you're having troubles sticking the player to a platform, make sure that 1. The scale is a perfect Vector3.one, 2. There aren't any conflicting colliders also being detected by the player
                transform.parent = onPlatform.transform;
                this.enabled = false;
                this.enabled = true;
                storedPlatform = onPlatform;
            }
        }
        else
        {
            transform.parent = null;
            storedPlatform = null;

            // Also reset our rotation and scale, because getting on a moving platform probably messed that up
            transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
            transform.localScale = Vector3.one;
        }
        // charControl moves player in all 3 axes
        if (charControl.collisionFlags != CollisionFlags.Sides)
        {
            charControl.Move(moveDirHoriz);
            charControl.Move(moveDirVert);
            charControl.Move(moveDirUp);
        }
        else
        {

        }

    }

    // Boosts the player in a direction in midair
    void DoubleJumpBoost()
    {

    }
    bool SlopeCheck() // Returns true if a slope is located below the player. Returns false if otherwise, so we can fall normally.
    {
        RaycastHit hit;

        // rayStart begins at 1.01 units lower than the player to make sure we don’t detect the player in our raycast
        Vector3 rayStart = new Vector3(transform.position.x, transform.position.y - 1.01f, transform.position.z);

        // Raycast should extend to the maximum angle the player can walk up (45 degrees)
        if (Physics.Raycast(rayStart, -transform.up, out hit, 0.2f)) // 0.2f is a rough estimate
        {
            // First, check to see if we're on a moving platform
            if (hit.collider.tag == "Platform")
            {
                onPlatform = hit.collider.gameObject;
            }
            else
            {
                onPlatform = null;
            }

            // If the surface has an angle, set our gravity to be much larger
            if (hit.transform.rotation.z != 0)
            {
                return true;
            }
            // Otherwise, give ourselves a small gravity
            else
            {
                return false;
            }
        }
        else
        {
            // If we don't hit anything, fall normally
            return false;
        }

    }
}
