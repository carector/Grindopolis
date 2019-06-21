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
        public bool isCrouching = false;
        public bool canMove = true;
        public bool canDoubleJump;
        public bool isOnLadder;
        public float gravity = 1;
        public float maxFallSpeed = 100;
        public float jumpForce;
        public float runSpeed;
        public float walkSpeed;
        public float slopeMax;
        public float maxSpeed;
        public GameObject onPlatform;
        public float fallingSpeed;
    }

    [System.Serializable]
    public class PlayerCombatSettings
    {
        public bool canAttack = true;
        public GameObject spellObject1;
        public GameObject spellObject2;

    }

    public bool canPickupObjects;
    public PlayerMovementSettings movementSettings;
    public PlayerCombatSettings combatSettings;
    public GameObject HUDPrefab;
    public GameObject nameDisplayerPrefab;
    public GameObject heldObject;
    public Material[] playerColors;

    public Renderer bodyRenderer;
    public PlayerNetworkScript pns; // lmao penis
    public Transform heldObjectFocus;
    public GameObject leftMouseProjectile;
    public Transform projectilePosition;

    // Private vars
    private GameObject storedPlatform;


    // Vars
    bool canStandUp;
    bool leftAttackCharged;
    bool rightAttackCharged;
    bool finishedSetup;
    bool isHoldingObject;
    bool jumping; // Used for the first frame of when the player jumps - prevents charControl.isGrounded from overwriting value before charControl.Move() is called
    bool hasDoubleJumped;
    bool adjustedMidairMovement;
    float remainingJump;

    Rigidbody pickupToJumpOn;

    Camera cam;
    CharacterController charControl;


    NetworkManagerScript nms;
    PlayerSounds pSounds;
    PlayerUIManager uiMan;

    Vector3 moveDirHoriz;
    Vector3 moveDirVert;
    Vector3 moveDirUp;

    Vector3 storedMoveDirHoriz;
    Vector3 storedMoveDirVert;

    Vector3 currentPos;
    Vector3 previousPos;
    Vector3 storedVelocity;

    Vector3 footstepPos;

    PickupScript pickup;

    // Use this for initialization
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        nms = GameObject.Find("NetworkManager").GetComponent<NetworkManagerScript>();
        pns = GameObject.Find("PlayerConnection(Clone)").GetComponent<PlayerNetworkScript>();
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
            if (charControl.isGrounded && !SlopeCheck())
            {
                Jump(false);
            }
            else if (!hasDoubleJumped)
            {
                Jump(true);
                hasDoubleJumped = true;
            }
        }

        canStandUp = CeilingRaycastCheck();

        // If we're crouching, set that to be true and play our sound effect
        if (Input.GetKeyDown(KeyCode.LeftControl) && !movementSettings.isCrouching)
        {
            CmdPlaySFX(3);
            CmdSetCrouchStatus(true);
            movementSettings.isCrouching = true;
        }
        else if (canStandUp && Input.GetKeyUp(KeyCode.LeftControl))
        {
            CmdPlaySFX(4);
            CmdSetCrouchStatus(false);
            movementSettings.isCrouching = false;
        }
        else if (!Input.GetKey(KeyCode.LeftControl) && movementSettings.isCrouching)
        {
            if (canStandUp)
            {
                CmdPlaySFX(4);
                CmdSetCrouchStatus(false);
                movementSettings.isCrouching = false;
            }
        }

        if (Input.GetButtonDown("Fire1") && heldObject == null && movementSettings.canMove && leftAttackCharged)
        {
            CmdLaunchProjectile(0);
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
        if (isHoldingObject)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log(storedVelocity);

                heldObject.transform.parent = null;
                heldObject.GetComponent<Rigidbody>().velocity = storedVelocity;
                heldObject = null;

                DropObject();
                isHoldingObject = false;
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                heldObject.transform.parent = null;
                heldObject.GetComponent<Rigidbody>().velocity = storedVelocity;
                heldObject.GetComponent<Rigidbody>().AddForce(cam.transform.TransformDirection(Vector3.forward * 500));
                heldObject = null;

                DropObject();
                isHoldingObject = false;
            }
        }

        // Update our previous position with our current position, since it will no longer be current next frame
        previousPos = currentPos;

    }

    // Movement calculation and physics are controlled with FixedUpdate
    private void FixedUpdate()
    {
        // If we're crouching, adjust our player's height and camera position to give the illusion of crouching
        if (movementSettings.isCrouching)
        {
            charControl.center = new Vector3(0, -0.5f, 0);
            charControl.height = 1;
            bodyRenderer.transform.localPosition = Vector3.Lerp(bodyRenderer.transform.localPosition, new Vector3(0, -0.5307f, 0), 0.25f);
            bodyRenderer.transform.localScale = Vector3.Lerp(bodyRenderer.transform.localScale, new Vector3(0.34343f, 0.7482614f, 0.34343f), 0.25f);

            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, Vector3.zero, 0.25f);
        }
        else
        {
            charControl.center = Vector3.zero;
            charControl.height = 2;

            bodyRenderer.transform.localPosition = Vector3.Lerp(bodyRenderer.transform.localPosition, new Vector3(0, -0.0936f, 0), 0.25f);
            bodyRenderer.transform.localScale = Vector3.Lerp(bodyRenderer.transform.localScale, new Vector3(0.34343f, 1.18542f, 0.34343f), 0.25f);

            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, new Vector3(0, 1, 0), 0.25f);
        }

        // All future code is only run clientside
        // Make sure this is our client's player
        if (!hasAuthority || !finishedSetup)
            return;

        // Make sure we can actually move first
        if (movementSettings.canMove)
        {
            // If we're sprinting, move the player with our sprint speed.
            // Make sure we aren't crouching too.
            if (Input.GetButton("Sprint") && !movementSettings.isCrouching)
                MovePlayer(movementSettings.runSpeed);
            // Otherwise, move with the walk speed.
            else
                MovePlayer(movementSettings.walkSpeed);
        }


    }
    void Jump(bool isDoubleJump)
    {
        // Check to see if this is a midair platform we're jumping off of

        RaycastHit hit;
        Vector3 rayStart = new Vector3(transform.position.x, transform.position.y - 1.01f, transform.position.z);

        if (Physics.Raycast(rayStart, -transform.up, out hit, 1f)) // 0.2f is a rough estimate
        {
            if (hit.collider.tag == "Pickup")
            {
                pickupToJumpOn = hit.collider.GetComponent<Rigidbody>();
                pickupToJumpOn.AddForce(-Vector3.up * 100);
                pickupToJumpOn.AddTorque(Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100));
            }
        }
        if (!isDoubleJump)
        {
            jumping = true;
            moveDirUp.y = movementSettings.jumpForce;
        }
        else if (movementSettings.canDoubleJump)
        {
            moveDirUp.y = movementSettings.jumpForce * 1.25f;
        }

        CmdPlaySFX(1);
        movementSettings.fallingSpeed = 0;

    }
    void MovePlayer(float speed)
    {
        // Get references to our movement axes - X and Z
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");


        // horizontal movement and vertical movement are set up like this because PlayerLook modifies player rotation
        // check first to see if we're in midair - if we are, don't take in any input and maintain what our velocity was before we jumped
        if (charControl.isGrounded)
        {
            moveDirHoriz = transform.right * horizontal * speed;
            moveDirVert = transform.forward * vertical * speed;

            // Store our current speeds in case jump at some point
            storedMoveDirHoriz = moveDirHoriz;
            storedMoveDirVert = moveDirVert;
        }
        else
        {
            // Start by checking to see if we've double jumped - this will allow us to change our position in mid-air
            if (hasDoubleJumped && !adjustedMidairMovement)
            {
                storedMoveDirHoriz = transform.right * horizontal * speed / 5;
                storedMoveDirVert = transform.forward * vertical * speed / 5;
                adjustedMidairMovement = true;
            }

            // Set our moveDir vectors to whatever we were just at when we were on the ground
            // Add input slightly so we have a tiny bit of control
            storedMoveDirHoriz = Vector3.Lerp(storedMoveDirHoriz, transform.right * horizontal * speed * 0.5f, 0.25f);
            storedMoveDirVert = Vector3.Lerp(storedMoveDirVert, transform.forward * vertical * speed * 0.5f, 0.25f);

            moveDirHoriz = storedMoveDirHoriz;
            moveDirVert = storedMoveDirVert;

            // If we aren't providing any input, we can slow our movement in that axis (useful when landing so you don't overshoot into the giant shit puddle)
            if (horizontal == 0 || Mathf.Sign(horizontal) != Mathf.Sign(transform.InverseTransformDirection(moveDirHoriz).x))
            {
                storedMoveDirHoriz = Vector3.Lerp(storedMoveDirHoriz, Vector3.zero, 0.1f);
            }
            if(vertical == 0 || Mathf.Sign(horizontal) != Mathf.Sign(transform.InverseTransformDirection(moveDirVert).z))
            {
                storedMoveDirVert = Vector3.Lerp(storedMoveDirVert, Vector3.zero, 0.1f);
            }


        }
        // Limit our speed in case we're over our max
        Vector3 relativeMoveDirHoriz = transform.InverseTransformDirection(moveDirHoriz);
        Vector3 relativeMoveDirVert = transform.InverseTransformDirection(moveDirVert);
        
        if (Mathf.Abs(relativeMoveDirHoriz.x) >= movementSettings.maxSpeed)
        {
            relativeMoveDirHoriz.x = speed * Mathf.Sign(relativeMoveDirHoriz.x);
        }
        if (Mathf.Abs(relativeMoveDirVert.z) >= movementSettings.maxSpeed)
        {
            relativeMoveDirVert.z = speed * Mathf.Sign(relativeMoveDirVert.z);
        }

        // Convert our relative speed back into our normal speed
        moveDirHoriz = transform.TransformDirection(relativeMoveDirHoriz);
        moveDirVert = transform.TransformDirection(relativeMoveDirVert);

        // Print our speed
        //uiMan.DisplaySpeed(Mathf.Round(relativeMoveDirHoriz.x * 100) / 100, Mathf.Round(relativeMoveDirVert.z * 100) / 100);




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

            // Reset some vars once we're grounded
            hasDoubleJumped = false;

            if(adjustedMidairMovement)
            {
                // Briefly set our speed to zero so we can land more easily
                moveDirHoriz = Vector3.zero;
                moveDirVert = Vector3.zero;
                adjustedMidairMovement = false;
            }
            

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

    bool CeilingRaycastCheck()
    {
        RaycastHit hit;
        Debug.DrawRay(cam.transform.position, cam.transform.TransformDirection(Vector3.forward * 2.5f), Color.red);

        if (Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.up), out hit, 2.5f))
        {
            return false;
        }
        return true;
    }

    void RaycastCheck()
    {
        RaycastHit hit;
        Debug.DrawRay(cam.transform.position, cam.transform.TransformDirection(Vector3.forward * 2.5f), Color.red);

        // Automatically disable crosshair - if something is hit, it will override this
        uiMan.DisableCrosshair();

        if (!isHoldingObject)
            uiMan.DisplayHintText("");

        if (Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out hit, 10))
        {
            if (hit.collider.tag == "Pickup" && !isHoldingObject)
            {
                uiMan.EnableCrosshair();
                pickup = hit.collider.GetComponent<PickupScript>();

                if (!pickup.isHeld && canPickupObjects)
                {
                    uiMan.EnableCrosshair();
                    uiMan.DisplayHintText("Press E to pick up " + pickup.pickupName);

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        uiMan.DisplayHintText("Press R to drop Press F to throw");
                        heldObject = pickup.gameObject;

                        PickupObject();

                        isHoldingObject = true;
                    }
                }
            }

            else if (hit.collider.tag == "NPC" && !uiMan.dialogBoxOpen)
            {
                uiMan.EnableCrosshair();
                InteractableNPC npc = hit.collider.GetComponent<InteractableNPC>();

                uiMan.DisplayHintText("Press E to talk to " + npc.npcName);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    uiMan.DisplayDialog(npc);
                }
            }/*
            else
            {
                uiMan.ResetDialogWindow();
                uiMan.StopAllCoroutines();
            }*/
        }
    }

    void PickupObject()
    {
        Debug.Log("Picked up object");
        pickup.isHeld = true;
        pickup.holder = this.gameObject;
        pickup.transform.parent = this.transform;
    }
    void DropObject()
    {
        pickup.isHeld = false;
        pickup.holder = null;
        pickup.transform.parent = null;
    }

    [Command]
    void CmdPlaySFX(int a)
    {
        RpcPlaySFX(a);
    }

    [Command]
    public void CmdLaunchProjectile(int proj)
    {
        RpcLaunchProjectile(proj);
    }

    [Command]
    public void CmdUpdatePlayerInfo(int materialIndex, string name)
    {
        RpcUpdatePlayerInfo(materialIndex, name);
    }

    [Command]
    public void CmdSetCrouchStatus(bool s)
    {
        RpcSetCrouchStatus(s);
    }

    [ClientRpc]
    void RpcPlaySFX(int a)
    {
        // Play different sfx based on int
        // 0 = Footsteps
        // 1 = Jump
        // 2 = Pickup Jump (To show you ain't fuckin around)
        // 3 = Crouch
        // 4 = Uncrouch

        if (a == 0)
        {
            pSounds.PlayFootstepSound();
        }
        else if (a == 1)
        {
            pSounds.PlayJumpSound();
        }
        else if (a == 2)
        {
            pSounds.PlayDoubleJumpSound();
        }
        else if (a == 3)
        {
            pSounds.PlayCrouchSound();
        }
        else if (a == 4)
        {
            pSounds.PlayUncrouchSound();
        }

    }

    [ClientRpc]
    public void RpcLaunchProjectile(int projNum)
    {
        GameObject proj = Instantiate(leftMouseProjectile, projectilePosition.position, projectilePosition.localRotation);
        proj.transform.parent = null;
        proj.GetComponent<Rigidbody>().AddRelativeForce(-cam.transform.right * 1000);
        NetworkServer.Spawn(proj);
    }

    [ClientRpc]
    void RpcUpdatePlayerInfo(int materialIndex, string name)
    {
        pns.playerName = name;
        pns.playerMatIndex = materialIndex;

        bodyRenderer.material = playerColors[materialIndex];
    }

    [ClientRpc]
    public void RpcSetCrouchStatus(bool s)
    {
        movementSettings.isCrouching = s;
        canStandUp = true;
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

        if (pns.playerName == "")
            pns.playerName = "Grinder " + nms.numConnectedPlayers;

        finishedSetup = true;
    }
}
