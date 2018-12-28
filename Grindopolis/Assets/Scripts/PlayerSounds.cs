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
    private AudioSource audio;


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
}
