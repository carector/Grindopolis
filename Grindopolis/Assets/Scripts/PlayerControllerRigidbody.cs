using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]

public class PlayerControllerRigidbody : NetworkBehaviour
{

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false;
    }

    // Public vars
    [System.Serializable]
    public class PlayerMovementSettings
    {
        public bool isGrounded;
        public bool isCrouching = false;
        public bool canMove = true;
        public bool canDoubleJump;
        public float gravity = 1;
        public float jumpHeight;
        public float runSpeed;
        public float walkSpeed;
        public float maxVelocityChange;
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
    public float groundCheckOffset = 1.01f;

    Rigidbody rb;
    Rigidbody pickupToJumpOn;

    Camera cam;

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

        if (movementSettings.isGrounded)
        {
            // Play footstep sounds
            if ((Vector3.Distance(footstepPos, transform.position) >= 3f))
            {
                footstepPos = transform.position;
                CmdPlaySFX(0);
            }

            Vector3 velocity = rb.velocity;

            // Jump
            if (Input.GetButtonDown("Jump"))
            {
                rb.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
                CmdPlaySFX(1);
            }
        }
        else if (!hasDoubleJumped)
        {
            Vector3 velocity = rb.velocity;

            // Jump
            if (Input.GetButtonDown("Jump"))
            {
                rb.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
                CmdPlaySFX(2);
                hasDoubleJumped = true;
            }
        }
        movementSettings.isGrounded = GroundCheck();

        // Attack check
        if(Input.GetMouseButtonUp(0))
        {
            GameObject proj = Instantiate(leftMouseProjectile, projectilePosition.position, projectilePosition.rotation);
            proj.GetComponent<FireballScript>().playerRb = rb;
            pns.SpawnProjectile(proj);
        }
        /*
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
        */

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
        // Make sure this is our client's player
        if (!hasAuthority || !finishedSetup)
        {
            return;
        }

        if (movementSettings.isGrounded)
        {
            // Calculate how fast we should be moving
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            targetVelocity = transform.TransformDirection(targetVelocity);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                targetVelocity *= movementSettings.runSpeed;
            }
            else
            {
                targetVelocity *= movementSettings.walkSpeed;
            }

            // Apply a force that attempts to reach our target velocity
            Vector3 velocity = rb.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -movementSettings.maxVelocityChange, movementSettings.maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -movementSettings.maxVelocityChange, movementSettings.maxVelocityChange);
            velocityChange.y = 0;

            if (!uiMan.menuOpen)
                rb.AddForce(velocityChange, ForceMode.Impulse);

            hasDoubleJumped = false;
        }

        else
        {
            // Calculate how fast we should be moving
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            targetVelocity = transform.TransformDirection(targetVelocity);
            targetVelocity *= movementSettings.walkSpeed * 0.75f;

            // Apply a force that attempts to reach our target velocity
            Vector3 velocity = rb.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -movementSettings.maxVelocityChange, movementSettings.maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -movementSettings.maxVelocityChange, movementSettings.maxVelocityChange);
            velocityChange.y = 0;
            rb.AddForce(velocityChange);

            rb.AddForce(velocityChange);
        }


        // We apply gravity manually for more tuning control
        rb.AddForce(new Vector3(0, -movementSettings.gravity * rb.mass, 0));

        movementSettings.isGrounded = GroundCheck();

        CapsuleCollider cc = GetComponent<CapsuleCollider>();

        // If we're crouching, adjust our player's height and camera position to give the illusion of crouching
        if (movementSettings.isCrouching)
        {
            
            cc.center = new Vector3(0, -0.5f, 0);
            cc.height = 1;
            bodyRenderer.transform.localPosition = Vector3.Lerp(bodyRenderer.transform.localPosition, new Vector3(0, -0.5307f, 0), 0.25f);
            bodyRenderer.transform.localScale = Vector3.Lerp(bodyRenderer.transform.localScale, new Vector3(0.34343f, 0.7482614f, 0.34343f), 0.25f);

            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, Vector3.zero, 0.25f);

            groundCheckOffset = Mathf.Lerp(groundCheckOffset, 0.75f, 0.25f);
        }
        else
        {
            cc.center = Vector3.zero;
            cc.height = 2.4f;

            bodyRenderer.transform.localPosition = Vector3.Lerp(bodyRenderer.transform.localPosition, new Vector3(0, -0.0621f, 0), 0.25f);
            bodyRenderer.transform.localScale = Vector3.Lerp(bodyRenderer.transform.localScale, new Vector3(0.34343f, 1.12325f, 0.34343f), 0.25f);

            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, new Vector3(0, 1, 0), 0.25f);

            groundCheckOffset = Mathf.Lerp(groundCheckOffset, 1.01f, 0.25f);
        }
        /*
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
        */

    }

    // Boosts the player in a direction in midair
    void DoubleJumpBoost()
    {

    }

    float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * movementSettings.jumpHeight * movementSettings.gravity);
    }

    bool GroundCheck()
    {
        RaycastHit hit;

        // rayStart begins at 1.01 units lower than the player to make sure we don’t detect the player in our raycast
        Vector3 rayStart = new Vector3(transform.position.x, transform.position.y - groundCheckOffset, transform.position.z);

        Debug.DrawRay(rayStart, -transform.up * 0.3f, Color.red);
        // Raycast should extend to the maximum angle the player can walk up
        if (Physics.Raycast(rayStart, -transform.up, out hit, 0.3f))
        {
            return true;
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
        pSounds = GetComponentInChildren<PlayerSounds>();

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

            cam.GetComponent<AudioSource>().spatialBlend = 0;

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
/*
public float walkSpeed = 10.0f;
    public float runSpeed = 10.0f;
    public float gravity = 10.0f;
    public float maxVelocityChange = 10.0f;
    public bool canJump = true;
    public float jumpHeight = 2.0f;

    private bool hasDoubleJumped;
    private bool grounded = false;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false;
    }

    private void Update()
    {
        if (grounded)
        {
            Vector3 velocity = rb.velocity;

            // Jump
            if (canJump && Input.GetButtonDown("Jump"))
            {
                rb.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
            }
        }
        else if(!hasDoubleJumped)
        {
            Vector3 velocity = rb.velocity;

            // Jump
            if (canJump && Input.GetButtonDown("Jump"))
            {
                rb.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
                hasDoubleJumped = true;
            }
        }

    }
    void FixedUpdate()
    {
        if (grounded)
        {
            // Calculate how fast we should be moving
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            targetVelocity = transform.TransformDirection(targetVelocity);
            targetVelocity *= walkSpeed;

            // Apply a force that attempts to reach our target velocity
            Vector3 velocity = rb.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;
            rb.AddForce(velocityChange, ForceMode.Impulse);

            hasDoubleJumped = false;
        }
        
        else
        {
            // Calculate how fast we should be moving
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            targetVelocity = transform.TransformDirection(targetVelocity);
            targetVelocity *= walkSpeed * 0.75f;

            // Apply a force that attempts to reach our target velocity
            Vector3 velocity = rb.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;
            rb.AddForce(velocityChange);

            rb.AddForce(velocityChange);
        }
        

        // We apply gravity manually for more tuning control
        rb.AddForce(new Vector3(0, -gravity * rb.mass, 0));

        grounded = GroundCheck();
    }

    

    float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * jumpHeight * gravity);
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
            }
        }
    }
}
*/
