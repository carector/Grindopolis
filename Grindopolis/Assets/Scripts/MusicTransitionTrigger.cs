using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTransitionTrigger : MonoBehaviour
{
    // Provide two AudioSources and this script will fade one out while fading in the other.

    public AudioSource audioToFadeOut;
    public AudioSource audioToFadeIn;
    public float fadeSpeed;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "ClientPlayer")
        {
            // Begin by stopping all running coroutines, so we don't have two transition triggers competing for superiority
            StartCoroutine(TransitionMusic());
        }
    }

    IEnumerator TransitionMusic()
    {
        while(audioToFadeIn.volume < 1)
        {
            audioToFadeOut.volume -= Time.deltaTime * fadeSpeed * 0.1f;
            audioToFadeIn.volume += Time.deltaTime * fadeSpeed * 0.1f;
            yield return null;
        }

        audioToFadeOut.volume = 0;
        audioToFadeIn.volume = 1;
    }
}
