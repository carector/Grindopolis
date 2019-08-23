using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTransitionTrigger : MonoBehaviour
{
    // Provide two AudioSources and this script will fade one out while fading in the other.

    public MusicTransitionTrigger[] triggers;
    public AudioSource[] audioToFadeOut;
    public AudioSource audioToFadeIn;
    public float fadeSpeed;
    public float targetVolume = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "ClientPlayer")
        {
            // Begin by stopping all running coroutines, so we don't have two transition triggers competing for superiority
            //Debug.Log("Player entered transition zone");
            StartCoroutine(TransitionMusic());
        }
    }

    IEnumerator TransitionMusic()
    {
        if (audioToFadeIn.volume <= targetVolume * 0.75f)
        {
            audioToFadeIn.Stop();
            audioToFadeIn.Play();
        }

        foreach(MusicTransitionTrigger trigger in triggers)
        {
            if(trigger != this)
                trigger.StopAllCoroutines();
        }

        while(audioToFadeIn.volume < targetVolume)
        {
            audioToFadeIn.volume += Time.deltaTime * fadeSpeed * 0.1f;

            foreach (AudioSource audio in audioToFadeOut)
            {
                // Select all audio sources except the one we're fading in - this way we can add all audio sources to our list
                if(audio != audioToFadeIn)
                    audio.volume -= Time.deltaTime * fadeSpeed * 0.2f;
            }

            yield return null;
        }

        foreach(AudioSource audio in audioToFadeOut)
            audio.volume = 0;

        audioToFadeIn.volume = targetVolume;
        //Debug.Log("Completed transition");
    }
}
