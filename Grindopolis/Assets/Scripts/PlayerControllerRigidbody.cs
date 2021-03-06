﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]

public class PlayerControllerRigidbody : MonoBehaviourPunCallbacks, IPunObservable
{
    // ========== VERY IMPORTANT ==========
    // OnPhotonSerializeView sends information out to other client players
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Send data
        if (stream.IsWriting)
        {
            stream.SendNext(mVar.isCrouching);
            stream.SendNext(mVar.playerName);
            stream.SendNext(mVar.playerMatIndex);
            stream.SendNext(vortexPos.GetComponent<ParticleSystem>().startColor.a);
            stream.SendNext(mVar.isPeeing);

            stream.SendNext(combatSettings.health);
        }
        // Recieve data
        else
        {
            mVar.isCrouching = (bool)stream.ReceiveNext();
            mVar.playerName = (string)stream.ReceiveNext();
            mVar.playerMatIndex = (int)stream.ReceiveNext();
            vortexPos.GetComponent<ParticleSystem>().startColor = new Color(255, 255, 255, (float)stream.ReceiveNext());
            mVar.isPeeing = (bool)stream.ReceiveNext();

            combatSettings.health = (int)stream.ReceiveNext();
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false;

        // Make sure we set this as the local player instance if we are the local player
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
        }
        else
        {
            LocalPlayerInstance = null;
        }

        //DontDestroyOnLoad(gameObject);
    }

    // Public vars
    [System.Serializable]
    public class PlayerMovementSettings
    {
        public bool isGrounded;
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
        public int health;
        public int mana;
        public float cash;
        public bool canAttack = true;

    }

    [System.Serializable]
    public class MultiplayerVariables
    {
        public bool isCrouching;
        public bool jumping;
        public bool doubleJumping;
        public string playerName;
        public int playerMatIndex;
        public bool isPeeing;
    }

    public static GameObject LocalPlayerInstance;

    public bool canPickupObjects;
    public PlayerMovementSettings movementSettings;
    public PlayerCombatSettings combatSettings;
    public MultiplayerVariables mVar;
    public GameObject HUDPrefab;
    public GameObject nameDisplayerPrefab;
    public GameObject heldObject;
    public Material[] playerColors;
    public AudioSource loopedAudio;
    public Renderer bodyRenderer;
    public Transform heldObjectFocus;
    public GameObject fireballProjectile;
    public GameObject healthBubbleProjectile;
    public Transform projectilePosition;
    public Light staffLight;
    public Transform vortexPos;
    public AudioClip pissSound;
    public AudioClip levitateSound;
    public ParticleSystem pissStream;
    public GameObject hitMarker;
    public Transform hitMarkerPosition;
    public Collider headCollider;
    public Rigidbody camRigidbody;
    public Collider camCollider;
    public FootColliderScript foot;
    public bool outOfBounds;
    // Private vars
    private GameObject storedPlatform;


    // Vars
    bool canStandUp;
    bool leftAttackCharged;
    bool rightAttackCharged;
    bool finishedSetup;
    bool isHoldingObject;
    bool adjustedMidairMovement;
    bool crouchSfx;
    bool jumpSfx;
    bool doubleJumpSfx;
    bool inWater;
    int storedColor = -1;
    bool isDeductingMana;
    bool isAddingMana;
    public bool[] canUseSpell;
    public bool isDead;

    public float groundCheckOffset = 1.01f;

    Rigidbody rb;
    Rigidbody pickupToJumpOn;

    Camera cam;
    public Camera renderTextureCam;
    public Image renderTextureImage;

    PlayerSounds pSounds;
    PlayerUIManager uiMan;
    StaffAnimate staff;

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

    GameObject nameDisplayer;

    // Use this for initialization
    void Start()
    {
        cam = GetComponentInChildren<Camera>();

        // Initialize positions
        currentPos = transform.position;
        previousPos = transform.position;

        // Start routine
        pSounds = GetComponentInChildren<PlayerSounds>();

        // Assign stored player name to our player's current name
        if (PlayerPrefs.GetString("PlayerName") != null)
        {
            mVar.playerName = PlayerPrefs.GetString("PlayerName");
        }
        else
        {
            mVar.playerName = "Brand New Grinder";
        }

        // Also assign stored color index
        if (PlayerPrefs.HasKey("PlayerColor"))
        {
            mVar.playerMatIndex = PlayerPrefs.GetInt("PlayerColor");
        }

        // Add random number to our vortexPos - this is so it can be found by pickups
        string rand = vortexPos.name + Random.Range(0, 1000);
        photonView.RPC("RandomizeVortexPos", RpcTarget.All, rand);

        AnnouncementsScript an = GameObject.Find("GameAnnouncementsAudio").GetComponent<AnnouncementsScript>();

        // Disable our camera if this is not our client player, as well as our audiolistener
        // if the player is the server, for whatever reason the camera won't disable due to the player not having authority
        // to bypass this, the server also checks to make sure there's at least one player connected before disabling the camera, in this case the server (i know this is fucking ASS but just roll with it)
        if ((photonView.IsMine == false && PhotonNetwork.IsConnected))
        {
            // Change our name so other spawned players know we're a specific client's player
            this.gameObject.name = "NonclientPlayer";

            Debug.Log("Non-client player joined");
            GetComponentInChildren<AudioListener>().enabled = false;
            renderTextureImage.enabled = false;
            renderTextureCam.enabled = false;

            // Update our material based on what our playerNetworkScript has stored
            //bodyRenderer.material = playerColors[pns.playerMatIndex];

            // Instantiate name displayer on any players that aren't the client's player
            nameDisplayer = Instantiate(nameDisplayerPrefab, this.transform);
            nameDisplayer.GetComponent<PlayerNameDisplayer>().thisPlayer = this;
            nameDisplayer.transform.localPosition = new Vector3(0, 1.75f, 0);
        }
        // This code runs if the current playercontroller is the one actually being controlled by a client
        else if (photonView.IsMine)
        {
            cam.enabled = true;

            // Change our name so other spawned players know we're a specific client's player
            this.gameObject.name = "ClientPlayer";

            GameObject hud = Instantiate(HUDPrefab, this.transform);
            uiMan = hud.GetComponent<PlayerUIManager>();
            uiMan.player = this.gameObject;

            staff = GetComponentInChildren<StaffAnimate>();

            cam.GetComponent<AudioSource>().spatialBlend = 0;
            loopedAudio.spatialBlend = 0;
            an.RpcWelcomeSound();
        }

        //StartCoroutine(OnStartCoroutine());
    }
    void OnEnable()
    {
        // Reset our rotation each time the player is enabled. This can fix problems that can occur when the player is above or below an NPC when they begin talking to it
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
    }

    // Input and jumping controlled via Update so nothing is missed
    void Update()
    {
        // All content that doesn't require this to be the client's player goes here

        // Make sure this is our client's player
        if (photonView.IsMine == false && PhotonNetwork.IsConnected)
        {
            //print("Player is not master client!");
            return;
        }

        // Check if our health is 0
        if (!isDead)
        {
            if (movementSettings.isGrounded)
            {
                Vector3 velocity = rb.velocity;

                mVar.jumping = false;
                mVar.doubleJumping = false;


                // Jump
                if (Input.GetButtonDown("Jump") && !uiMan.menuOpen)
                {
                    rb.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
                    photonView.RPC("PlayJumpSound", RpcTarget.All);
                    mVar.jumping = true;
                }
            }
            else if (!mVar.doubleJumping && !uiMan.menuOpen)
            {
                Vector3 velocity = rb.velocity;

                // Jump
                if (Input.GetButtonDown("Jump"))
                {
                    rb.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
                    photonView.RPC("PlayJumpSound", RpcTarget.All);
                    mVar.doubleJumping = true;
                    DoubleJumpBoost();
                }
            }
            movementSettings.isGrounded = GroundCheck();

            // Spellcast check
            if (!uiMan.menuOpen)
            {
                // Execute a different spell depending on what we currently have selected

                // Magic missile
                if (uiMan.currentSidebarIndex == 0 && canUseSpell[0])
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        GameObject proj = PhotonNetwork.Instantiate(this.fireballProjectile.name, projectilePosition.position, projectilePosition.rotation);
                        proj.GetComponent<FireballScript>().playerRb = rb;

                        StartCoroutine(SpellDelay(0.75f, 0));
                        uiMan.magicMissleCountdown = 0.75f;
                        canUseSpell[0] = false;
                    }
                }

                // Illuminate
                else if (uiMan.currentSidebarIndex == 1 && canUseSpell[1])
                {
                    if (Input.GetMouseButton(0))
                    {
                        staff.Illuminate(true);
                    }
                    else
                    {
                        staff.Illuminate(false);
                    }
                }

                // Levitate
                else if (uiMan.currentSidebarIndex == 2 && canUseSpell[2])
                {
                    if (heldObject != null && loopedAudio.clip != levitateSound)
                    {
                        loopedAudio.clip = levitateSound;
                        loopedAudio.Play();
                    }

                    vortexPos.transform.rotation = Quaternion.identity;

                    if (heldObject != null && Input.GetMouseButtonDown(1))
                    {
                        vortexPos.GetComponent<ParticleSystem>().startColor = Color.clear;

                        staff.StaffEmissions(false);
                        staff.VortexEmissions(false);

                        //heldObject.GetComponent<PhotonView>().RPC("RpcDropObject", RpcTarget.All, heldObject.transform.position, -transform.forward * 25);
                        heldObject.GetComponent<PickupControlNoNet>().LaunchObject(heldObject.transform.position, -transform.forward * 20);
                        heldObject = null;

                        isHoldingObject = false;
                    }
                    else if(heldObject != null && (Input.GetMouseButtonUp(0) || heldObject.GetComponent<PickupControlNoNet>().focusedTransform == null))
                    {
                        vortexPos.GetComponent<ParticleSystem>().startColor = Color.clear;

                        staff.StaffEmissions(false);
                        staff.VortexEmissions(false);

                        heldObject.GetComponent<PickupControlNoNet>().DropObject(heldObject.transform.position);
                        heldObject = null;

                        isHoldingObject = false;
                    }
                    else if (Input.GetMouseButton(0) && combatSettings.mana > 0)
                    {
                        //if (!isDeductingMana)
                        //StartCoroutine(DeductMana(0.2f, 1));

                        staff.StaffEmissions(true);

                        if (!isHoldingObject)
                        {
                            print("Raycasting");
                            RaycastCheck();
                        }
                        else
                        {
                            //staff.VortexEmissions(true);
                            loopedAudio.volume = Mathf.Lerp(loopedAudio.volume, 0.8f, 0.25f);
                            float newPitch = 1 + (Vector3.Distance(vortexPos.position, heldObject.transform.position) / 5f);

                            if(newPitch < 1.5f)
                            {
                                loopedAudio.pitch = newPitch;
                            }
                        }
                    }
                    else if (Input.GetMouseButtonUp(0))
                    {
                        staff.StaffEmissions(false);
                    }

                    if (!isHoldingObject)
                    {
                        loopedAudio.volume = Mathf.Lerp(loopedAudio.volume, 0, 0.25f);

                        if (loopedAudio.volume <= 0.05f)
                        {
                            loopedAudio.Stop();
                            loopedAudio.volume = 0;
                            loopedAudio.clip = null;
                        }
                    }
                }

                // Health Bubble
                else if (uiMan.currentSidebarIndex == 3 && canUseSpell[3])
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        GameObject proj = PhotonNetwork.Instantiate(this.healthBubbleProjectile.name, projectilePosition.position, projectilePosition.rotation);
                        StartCoroutine(SpellDelay(30, 3));
                        uiMan.healthBubbleCountdown = 30;
                        canUseSpell[3] = false;
                    }
                }

                // Piss
                else if (uiMan.currentSidebarIndex == 4 && canUseSpell[4])
                {
                    loopedAudio.pitch = 1;

                    if (Input.GetMouseButton(0))
                    {
                        mVar.isPeeing = true;

                    }
                    else
                    {
                        mVar.isPeeing = false;
                    }
                }


                // Reset light
                if (uiMan.currentSidebarIndex != 1)
                    staffLight.intensity = Mathf.Lerp(staffLight.intensity, 0, 0.25f);
            }

            // Recharge mana if we aren't holding the mouse
            if (!Input.GetMouseButton(0))
            {

            }

            canStandUp = CeilingRaycastCheck();

            // If we're crouching, set that to be true and play our sound effect
            if (Input.GetKeyDown(KeyCode.LeftControl) && !mVar.isCrouching && !uiMan.menuOpen)
            {
                mVar.isCrouching = true;
            }
            else if (canStandUp && Input.GetKeyUp(KeyCode.LeftControl))
            {
                mVar.isCrouching = false;
            }
            else if (!Input.GetKey(KeyCode.LeftControl) && mVar.isCrouching)
            {
                if (canStandUp)
                {
                    mVar.isCrouching = false;
                }
            }

            // Constantly use raycast to check for grabbable objects and NPCs
            RaycastCheck();
        }
    }

    // Movement calculation and physics are controlled with FixedUpdate
    // Network-related code also is triggered from here
    private void FixedUpdate()
    {
        // All content that doesn't require this to be the client's player goes here
        if (!isDead)
        {
            // Set our color
            if (storedColor != mVar.playerMatIndex)
            {
                bodyRenderer.material = playerColors[mVar.playerMatIndex];
                storedColor = mVar.playerMatIndex;
            }

            // Enable piss.
            if (mVar.isPeeing)
            {
                pissStream.Play();
                loopedAudio.volume = Mathf.Lerp(loopedAudio.volume, 0.8f, 0.25f);

                if (loopedAudio.clip != pissSound)
                {
                    loopedAudio.clip = pissSound;
                    loopedAudio.Play();
                }
            }
            else
            {
                mVar.isPeeing = false;
                pissStream.Stop();

                loopedAudio.volume = Mathf.Lerp(loopedAudio.volume, 0, 0.5f);

                if (loopedAudio.volume <= 0.1f)
                {
                    loopedAudio.Stop();
                    loopedAudio.volume = 0;
                    loopedAudio.clip = null;
                }
            }

            // Play footstep sounds
            if ((Vector3.Distance(footstepPos, transform.position) >= 3f) && GroundCheck())
            {
                footstepPos = transform.position;
                PlaySFX(0);
            }

            // If we're crouching, adjust our player's height and camera position to give the illusion of crouching
            CapsuleCollider cc = GetComponent<CapsuleCollider>();

            if (mVar.isCrouching)
            {
                if (!crouchSfx)
                {
                    PlaySFX(3);
                    crouchSfx = true;
                }
                cc.center = new Vector3(0, -0.5f, 0);
                cc.height = 1;
                bodyRenderer.transform.localPosition = Vector3.Lerp(bodyRenderer.transform.localPosition, new Vector3(0, -0.5307f, 0), 0.25f);
                bodyRenderer.transform.localScale = Vector3.Lerp(bodyRenderer.transform.localScale, new Vector3(0.34343f, 0.7482614f, 0.34343f), 0.25f);

                cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, Vector3.zero, 0.25f);

                groundCheckOffset = Mathf.Lerp(groundCheckOffset, 0.75f, 0.25f);
            }
            else
            {
                if (crouchSfx)
                {
                    PlaySFX(4);
                    crouchSfx = false;
                }
                cc.center = Vector3.zero;
                cc.height = 2.4f;

                bodyRenderer.transform.localPosition = Vector3.Lerp(bodyRenderer.transform.localPosition, new Vector3(0, -0.0621f, 0), 0.25f);
                bodyRenderer.transform.localScale = Vector3.Lerp(bodyRenderer.transform.localScale, new Vector3(0.34343f, 1.12325f, 0.34343f), 0.25f);

                cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, new Vector3(0, 1, 0), 0.25f);

                groundCheckOffset = Mathf.Lerp(groundCheckOffset, 1.01f, 0.25f);
            }


            // Make sure this is our client's player
            if (photonView.IsMine == false && PhotonNetwork.IsConnected)
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

            }


            // We apply gravity manually for more tuning control
            rb.AddForce(new Vector3(0, -movementSettings.gravity * rb.mass, 0));

            movementSettings.isGrounded = GroundCheck();
        }
    }

    // Boosts the player in a direction in midair
    void DoubleJumpBoost()
    {
        // Calculate how fast we should be moving
        int horiz = 0;
        int vert = 0;

        if(Input.GetKey(KeyCode.D))
        {
            horiz = 1;
        }
        else if(Input.GetKey(KeyCode.A))
        {
            horiz = -1;
        }

        if (Input.GetKey(KeyCode.W))
        {
            vert = 1;
        }
        else if(Input.GetKey(KeyCode.S))
        {
            vert = -1;
        }

        if(vert != 0 || horiz != 0)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        Vector3 targetVelocity = new Vector3(horiz, 0, vert);

        targetVelocity = transform.TransformDirection(targetVelocity).normalized;

        if(Input.GetKey(KeyCode.LeftShift))
        {
            targetVelocity *= movementSettings.runSpeed;
        }
        else
        {
            targetVelocity *= movementSettings.walkSpeed;
        }

        targetVelocity.x = Mathf.Clamp(targetVelocity.x, -movementSettings.maxVelocityChange, movementSettings.maxVelocityChange);
        targetVelocity.z = Mathf.Clamp(targetVelocity.z, -movementSettings.maxVelocityChange, movementSettings.maxVelocityChange);

        rb.AddForce(targetVelocity, ForceMode.VelocityChange);
    }

    float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * movementSettings.jumpHeight * movementSettings.gravity);
    }

    bool GroundCheck()
    {
        return foot.isGrounded;

        /*List<Collider> col = foot.cols;

        foreach (Collider c in col)
        {
            if (c.gameObject != this.gameObject)
            {
                if (c.tag == "Water")
                {
                    inWater = true;
                    mVar.doubleJumping = true;
                    return false;
                }

                inWater = false;
                return true;
            }
        }
        */

        /*Debug.DrawRay(rayStart, -transform.up * 0.3f, Color.red);
        // Raycast should extend to the maximum angle the player can walk up
        if (Physics.Raycast(rayStart, -transform.up, out hit, 0.3f))
        {
            
        }
        else
        {
            // If we don't hit anything, fall normally
            return false;
        }
        */
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
        Debug.DrawRay(cam.transform.position, cam.transform.TransformDirection(Vector3.forward * 5), Color.red);

        // Automatically disable crosshair - if something is hit, it will override this
        //uiMan.DisableCrosshair();

        if (!isHoldingObject)
            uiMan.DisplayHintText("");

        if (Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out hit, 5f))
        {
            if (heldObject == null && uiMan.currentSidebarIndex == 2 && hit.transform != vortexPos && hit.collider.tag == "Pickup" && Input.GetMouseButton(0))
            {
                // Since we can't directly control the pickup physics from each client, we have the pickup itself
                // control the physics and just update the transform it's focusing on
                if (hit.collider.GetComponent<PickupControlNoNet>().focusedTransform != vortexPos)
                {
                    //hit.collider.GetComponent<PhotonView>().RPC("RpcUpdateOwnership", RpcTarget.All, photonView.ViewID, vortexPos.name);
                    hit.collider.GetComponent<PickupControlNoNet>().UpdateTransform(vortexPos);
                    heldObject = hit.collider.gameObject;
                    isHoldingObject = true;
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
    public void ReceiveDamage(int damage)
    {
        combatSettings.health -= damage;
        if (combatSettings.health <= 0 && !isDead)
        {
            StartCoroutine(DeathSequence(Vector3.zero));
        }
    }

    public IEnumerator SpellDelay(float time, int spellIndex)
    {
        yield return new WaitForSeconds(time);
        canUseSpell[spellIndex] = true;
    }

    // Death process
    public IEnumerator DeathSequence(Vector3 force)
    {
        isDead = true;
        PlaySFX(5);
        rb.isKinematic = true;

        // Send our camera flying
        headCollider.isTrigger = true;

        cam.GetComponent<PlayerLook>().enabled = false;
        camRigidbody.isKinematic = false;
        camCollider.isTrigger = false;
        yield return new WaitForEndOfFrame();
        camRigidbody.AddForce(force, ForceMode.VelocityChange);
        camRigidbody.angularVelocity = force;

        yield return new WaitForSeconds(4);
        print("Finished delay");

        while (uiMan.screenBlackout.color.a < 0.95f)
        {
            uiMan.screenBlackout.color = new Color(0, 0, 0, uiMan.screenBlackout.color.a + 0.03f);
            yield return null;
        }

        uiMan.screenBlackout.color = Color.black;
        yield return new WaitForSeconds(0.25f);

        // Reset camera and fade in screen
        transform.position = new Vector3(Random.Range(-10, 10), 1.2f, Random.Range(-5, -15));
        transform.rotation = Quaternion.identity;

        camRigidbody.isKinematic = true;
        camRigidbody.angularVelocity = Vector3.zero;
        camRigidbody.velocity = Vector3.zero;
        camCollider.isTrigger = true;
        headCollider.isTrigger = false;

        cam.transform.localPosition = new Vector3(0, 1, 0);
        cam.transform.localRotation = Quaternion.identity;

        while (uiMan.screenBlackout.color.a > 0.05f)
        {
            uiMan.screenBlackout.color = new Color(0, 0, 0, uiMan.screenBlackout.color.a - 0.03f);
            yield return null;
        }

        uiMan.screenBlackout.color = Color.clear;

        while (combatSettings.health < 100)
        {
            combatSettings.health += 1;
            yield return null;
        }

        rb.isKinematic = false;
        cam.GetComponent<PlayerLook>().enabled = true;

        isDead = false;
    }

    void PlaySFX(int ind)
    {
        // Play different sfx based on int
        // 0 = Footsteps
        // 1 = Jump
        // 2 = Pickup Jump (To show you ain't fuckin around)
        // 3 = Crouch
        // 4 = Uncrouch
        // 5 = Death

        if (ind == 0)
        {
            pSounds.PlayFootstepSound();
        }
        else if (ind == 1)
        {
            pSounds.PlayJumpSound();
        }
        else if (ind == 2)
        {
            pSounds.PlayDoubleJumpSound();
        }
        else if (ind == 3)
        {
            pSounds.PlayCrouchSound();
        }
        else if (ind == 4)
        {
            pSounds.PlayUncrouchSound();
        }
        else if (ind == 5)
        {
            pSounds.PlayDeathSound();
        }

        // Be sure to reset sfxIndex so we don't constantly play sfxIndex
    }

    [PunRPC]
    void PlayJumpSound()
    {
        PlaySFX(1);
        mVar.jumping = true;
    }

    [PunRPC]
    void RandomizeVortexPos(string rand)
    {
        vortexPos.name = rand;
    }

    public void UpdatePlayerInfo(int materialIndex, string name)
    {
        mVar.playerName = name;
        PlayerPrefs.SetString("PlayerName", name);
        mVar.playerMatIndex = materialIndex;
        PlayerPrefs.SetInt("PlayerColor", materialIndex);
    }

    IEnumerator DeductMana(float speed, int amount)
    {
        isDeductingMana = true;
        combatSettings.mana -= amount;
        yield return new WaitForSeconds(speed);
        isDeductingMana = false;

    }

    IEnumerator AddMana(float speed, int amount)
    {
        isDeductingMana = true;
        combatSettings.mana -= amount;
        yield return new WaitForSeconds(speed);
        isDeductingMana = false;

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
