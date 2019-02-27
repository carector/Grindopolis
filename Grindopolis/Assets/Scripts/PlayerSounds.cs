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
    public AudioClip crouchSound;
    public AudioClip uncrouchSound;

    // Private vars
    private AudioSource audio;

    bool isCrouched;


    // Use this for initialization
    void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    // PlayJumpSound() is called from PlayerMove when the player jumps
    public void PlayJumpSound()
    {
        audio.PlayOneShot(jumpSound);
    }

    public void PlayFootstepSound()
    {
        audio.PlayOneShot(footsteps[Random.Range(0, footsteps.Length - 1)]);
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

    public void PlayCrouchSound()
    {
        // To prevent multiple clips from playing, isCrouched must be checked (resets each time this is called)
        if(!isCrouched)
            audio.PlayOneShot(crouchSound);
            isCrouched = true;
    }
    public void PlayUncrouchSound()
    {
        // To prevent multiple clips from playing, isCrouched must be unchecked checked (resets each time this is called)
        if (isCrouched)  
            audio.PlayOneShot(uncrouchSound);
            isCrouched = false;
    }
}
