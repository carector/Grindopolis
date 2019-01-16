using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

// thanks to Acacia Developer on youtube for providing the tutorial used to make the base for this

public class PlayerController : NetworkBehaviour
{
    // Public vars
    [System.Serializable]
    public class PlayerMovementSettings
    {
        public bool isGrounded;
        public bool canMove = true;
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
    }

    public PlayerMovementSettings movementSettings;
    public GameObject HUDPrefab;
    public GameObject nameDisplayerPrefab;
    public GameObject heldObject;
    public Material[] playerColors;

    public Renderer bodyRenderer;
    public PlayerNetworkScript pns; // lmao penis
    public Transform heldObjectFocus;

    // Private vars
    private GameObject storedPlatform;


    // Vars
    bool finishedSetup;
    bool isHoldingObject;
    bool jumping; // Used for the first frame of when the player jumps - prevents charControl.isGrounded from overwriting value before charControl.Move() is called
    bool hasDoubleJumped;
    float remainingJump;


    Camera cam;
    CharacterController charControl;

    
    NetworkManagerScript nms;
    PlayerSounds pSounds;
    PlayerUIManager uiMan;

    Vector3 moveDirHoriz;
    Vector3 moveDirVert;
    Vector3 moveDirUp;

    Vector3 currentPos;
    Vector3 previousPos;
    Vector3 storedVelocity;

    Vector3 footstepPos;
    

    // Use this for initialization
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        nms = GameObject.Find("NetworkManager").GetComponent<NetworkManagerScript>();
        pns = GameObject.Find("PlayerConnection(Clone)").GetComponent<PlayerNetworkScript>() ;
        pns.name = "PlayerConnection" + nms.numConnectedPlayers;

        // Initialize positions
        currentPos = transform.position;
        previousPos = transform.position;

        StartCoroutine(OnStartCoroutine());
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
        if (!hasAuthority || !finishedSetup)
        {       
            return;
        }

        movementSettings.isGrounded = charControl.isGrounded;

        // Update our current position
        currentPos = transform.position;

        // Update our velocity based on our position values
        storedVelocity = (currentPos - previousPos) / Time.deltaTime;

        // Check for footstep or hit ground sfx
        if (movementSettings.isGrounded)
        {
            movementSettings.fallingSpeed = 0;

            if ((Vector3.Distance(footstepPos, transform.position) >= 3.5f))
            {    
                footstepPos = transform.position;
                CmdPlaySFX(0);
            }
        }

        if (Input.GetButtonDown("Jump") && movementSettings.canMove)
        {
            if (charControl.isGrounded)
            {
                Jump(false);
            }
            else if (!hasDoubleJumped)
            {
                Jump(true);
                hasDoubleJumped = true;
            }
        }

        // Also check to see if we should receive falling damage
        // Increase falling speed if our current velocity is greater than what we have stored
        if (charControl.velocity.y < movementSettings.fallingSpeed)
        {
            movementSettings.fallingSpeed = charControl.velocity.y;
        }

        // Constantly use raycast to check for grabbable objects and NPCs
        RaycastCheck();

        // If we're holding an object, we can either drop it or throw it
        if(isHoldingObject)
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log(storedVelocity);

                heldObject.transform.parent = null;
                heldObject.GetComponent<Rigidbody>().velocity = storedVelocity;
                heldObject = null;
                isHoldingObject = false;
            }
            else if(Input.GetKeyDown(KeyCode.F))
            {
                heldObject.transform.parent = null;
                heldObject.GetComponent<Rigidbody>().velocity = storedVelocity;
                heldObject.GetComponent<Rigidbody>().AddForce(cam.transform.TransformDirection(Vector3.forward * 500));
                heldObject = null;
                isHoldingObject = false;
            }
        }

        // Update our previous position with our current position, since it will no longer be current next frame
        previousPos = currentPos;

    }

    // Movement calculation and physics are controlled with FixedUpdate
    private void FixedUpdate()
    {
        // Make sure this is our client's player
        if (!hasAuthority || !finishedSetup)
            return;

        // Make sure we can actually move first
        if (movementSettings.canMove)
        {
            // If we're sprinting, move the player with our sprint speed.
            if (Input.GetButton("Sprint"))
                MovePlayer(movementSettings.runSpeed);
            // Otherwise, move with the walk speed.
            else
                MovePlayer(movementSettings.walkSpeed);
        }
    }
    void Jump(bool isDoubleJump)
    {
        if (!isDoubleJump)
        {
            jumping = true;
            moveDirUp.y = movementSettings.jumpForce;
            CmdPlaySFX(1);
        }
        else if (movementSettings.canDoubleJump)
        {
            moveDirUp.y = movementSettings.jumpForce * 1.25f;
            CmdPlaySFX(1);
        }

        movementSettings.fallingSpeed = 0;

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
        if (movementSettings.isOnLadder)
        {
            moveDirUp = transform.up * vertical * speed;

            // Clamp vertical at 0 so we don't fall down the ladder
            if (vertical <= 0)
            {
                moveDirVert = Vector3.zero;
            }
        }


        // If we're grounded and the player hasn't just pressed the jump key...
        if ((charControl.isGrounded && !jumping && !movementSettings.isOnLadder))
        {
            // Call SlopeCheck to see if we're standing on a sloping surface
            if (SlopeCheck())
            {
                // Set our downward movement speed to equal to gravity if we're standing on a slope. This way, we won't end up "skipping" from having our downwards force be too low.
                moveDirUp.y = -movementSettings.gravity;
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
        else if (!movementSettings.isOnLadder)
        {
            // Subtract gravity from our downward speed
            if (moveDirUp.y <= movementSettings.maxFallSpeed)
            {
                moveDirUp.y -= movementSettings.gravity * Time.deltaTime;
            }
            jumping = false;
        }
        if (movementSettings.onPlatform != null)
        {
            if (storedPlatform != movementSettings.onPlatform)
            {
                // For some reason, reloading the PlayerMove script will sort of reset the Character Controller's frame of reference, and it will move with playforms
                // If you're having troubles sticking the player to a platform, make sure that 1. The scale is a perfect Vector3.one, 2. There aren't any conflicting colliders also being detected by the player
                transform.parent = movementSettings.onPlatform.transform;
                this.enabled = false;
                this.enabled = true;
                storedPlatform = movementSettings.onPlatform;
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
                movementSettings.onPlatform = hit.collider.gameObject;
            }
            else
            {
                movementSettings.onPlatform = null;
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

    void RaycastCheck()
    {
        RaycastHit hit;
        Debug.Log("Checking...");
        Debug.DrawRay(cam.transform.position, cam.transform.TransformDirection(Vector3.forward * 2.5f), Color.red);

        // Automatically disable crosshair - if something is hit, it will override this
        uiMan.DisableCrosshair();

        if(!isHoldingObject)
            uiMan.DisplayHintText("");

        if (Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out hit, 8))
        {
            if (hit.collider.tag == "Pickup" && !isHoldingObject)
            {
                uiMan.EnableCrosshair();
                PickupScript pickup = hit.collider.GetComponent<PickupScript>();

                uiMan.EnableCrosshair();
                uiMan.DisplayHintText("Press E to pick up " + pickup.pickupName);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    uiMan.DisplayHintText("Press R to drop Press F to throw");
                    pickup.focus = heldObjectFocus;
                    heldObject = pickup.gameObject;
                    pickup.transform.parent = this.transform;

                    isHoldingObject = true;
                }
            }

            else if(hit.collider.tag == "NPC")
            {
                uiMan.EnableCrosshair();
                InteractableNPC npc = hit.collider.GetComponent<InteractableNPC>();

                uiMan.DisplayHintText("Press E to talk to " + npc.name);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    uiMan.DisplayDialog(npc);
                }
            }
            else
            {
                uiMan.ResetDialogWindow();
                uiMan.StopAllCoroutines();
            }
        }
    }


    [Command]
    void CmdPlaySFX(int a)
    {
        Debug.Log("CmdPlaySFX: Playing SFX " + a);
        RpcPlaySFX(a);
    }

    [Command]
    public void CmdUpdatePlayerInfo(int materialIndex, string name)
    {
        RpcUpdatePlayerInfo(materialIndex, name);
    }

    [Command]
    void CmdPickupObject()
    {
        RpcPickupObject();
    }

    [ClientRpc]
    void RpcPlaySFX(int a)
    {
        // Play different sfx based on int
        // 0 = Footsteps
        // 1 = Jump
        // 2 = Double jump
        if (a == 0)
        {
            pSounds.PlayFootstepSound();
        }
        else if (a == 1)
        {
            pSounds.PlayJumpSound();
        }

    }

    [ClientRpc]
    void RpcUpdatePlayerInfo(int materialIndex, string name)
    {
        pns.playerName = name;
        pns.playerMatIndex = materialIndex;

        bodyRenderer.material = playerColors[materialIndex];
    }

    [ClientRpc]
    void RpcPickupObject()
    {
       //pickup.transform.parent = this.transform;
    }

    IEnumerator OnStartCoroutine()
    {
        yield return new WaitForSeconds(0.05f);

        Debug.Log("Player joined");
        charControl = GetComponent<CharacterController>();
        pSounds = GetComponentInChildren<PlayerSounds>();

        // Make our jump and move values smaller so you don't have to enter miniscule values in the editor - it's stupid but it's not like anyone else is gonna find this (or will they? °.√•)
        movementSettings.walkSpeed /= 30;
        movementSettings.runSpeed /= 30;
        movementSettings.jumpForce /= 30;

        // Disable our camera if this is not our client player, as well as our audiolistener
        // if the player is the server, for whatever reason the camera won't disable due to the player not having authority
        // to bypass this, the server also checks to make sure there's at least one player connected before disabling the camera, in this case the server (i know this is fucking ASS but just roll with it)
        if (!hasAuthority)
        {
            // Change our name so other spawned players know we're a specific client's player
            this.gameObject.name = "NonclientPlayer";

            Debug.Log("Non-client player joined");
            GetComponentInChildren<AudioListener>().enabled = false;

            // Update our material based on what our playerNetworkScript has stored
            bodyRenderer.material = playerColors[pns.playerMatIndex];

            // Instantiate name displayer on any players that aren't the client's player
            GameObject nameDisplayer = Instantiate(nameDisplayerPrefab, this.transform);
            nameDisplayer.GetComponent<PlayerNameDisplayer>().thisPlayer = this;
            nameDisplayer.transform.localPosition = new Vector3(0, 1.75f, 0);
        }
        // This code runs if the current playercontroller is the one actually being controlled by a client
        else if (hasAuthority || nms.numConnectedPlayers == 0)
        {
            cam.enabled = true;

            // Change our name so other spawned players know we're a specific client's player
            this.gameObject.name = "ClientPlayer";

            GameObject hud = Instantiate(HUDPrefab, this.transform);
            uiMan = hud.GetComponent<PlayerUIManager>();
            uiMan.player = this.gameObject;

            /*
            uiMan.hudCanvas.worldCamera = cam;
            uiMan.hudCanvas.planeDistance = 0.15f;
            uiMan.menuCanvas.worldCamera = cam;
            uiMan.menuCanvas.planeDistance = 0.15f;
            */
            
        }

        nms.numConnectedPlayers++;
        print("Player " + nms.numConnectedPlayers + " setup complete.");

        if(pns.playerName == "")
            pns.playerName = "Grinder " + nms.numConnectedPlayers;

        finishedSetup = true;
    }
}
