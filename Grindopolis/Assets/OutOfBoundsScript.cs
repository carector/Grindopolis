using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBoundsScript : MonoBehaviour
{
    public PlayerControllerRigidbody player;
    public bool currentlyInSequence;
    public AnnouncementsScript an;

    public flatman fm;
    public Color fogColor;
    public float fogDistance;
    public float fogStartDistance;
    public float lightIntensity;
    public float currentTime = 60;
    public Light dirLight;

    // Start is called before the first frame update
    void Start()
    {
        currentTime = 60;
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.outOfBounds)
        {
            currentlyInSequence = false;

        }

        if(currentlyInSequence)
        {
            if(currentTime > 1)
                currentTime -= Time.deltaTime;
            else
            {
                fm.appear = true;
            }

            lightIntensity = currentTime / 60f;
            fogDistance = (680 * (currentTime / 60f));
            fogStartDistance = (60 * (currentTime / 60f));
            fogColor = Color.Lerp(RenderSettings.fogColor, Color.black, 0.001f);

            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogDistance;
            RenderSettings.fogColor = fogColor;
            dirLight.intensity = lightIntensity;

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == player.gameObject && !player.outOfBounds)
        {
            player.outOfBounds = true;
            an.OutOfBoundsSound();
            currentlyInSequence = true;
        }
    }
}
