using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{

    // Public vars
    public float distanceBeforeStep;
    public AudioClip[] footsteps;
    public AudioClip jumpSound;
    public AudioClip boostJumpSound;
    public AudioClip hitGroundSound;
    public AudioClip fallDamageSound;

    // Private vars
    private PlayerMove pMove;
    private AudioSource audio;
    private  Vector3 storedPos;
    private bool hitGround = false;


    // Use this for initialization
    void Start()
    {
        audio = GetComponent<AudioSource>();
        pMove = GetComponentInParent<PlayerMove>();
        storedPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // If the player is grounded and we aren't on a train...
        if (pMove.isGrounded)
        {
            if (Vector3.Distance(storedPos, transform.position) >= distanceBeforeStep)
            {
                audio.PlayOneShot(footsteps[Random.Range(0, footsteps.Length - 1)]);

                storedPos = transform.position;
            }
            // If we've hit the ground but haven't played our hitGround sound yet, play it and set hitGround to true so we don't infinitely play it
            if (!hitGround)
            {
                HitGroundSound(0);
                hitGround = true;
            }
        }
        else
        {
            hitGround = false;
        }
    }
    // PlayJumpSound() is called from PlayerMove when the player jumps
    public void PlayJumpSound()
    {
        audio.PlayOneShot(jumpSound);
    }

    // Called when player double jumps
    public void PlayDoubleJumpSound()
    {
        audio.PlayOneShot(boostJumpSound);
    }

    public void HitGroundSound(int power)
    {
        if(power == 0)
            audio.PlayOneShot(hitGroundSound, 0.6f);

        else
            audio.PlayOneShot(fallDamageSound, 1);
    }
}
