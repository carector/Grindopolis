using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flatman : MonoBehaviour
{
    public bool appear;
    public Transform vortex;
    public PlayerControllerRigidbody player;

    float countdown = 5;
    SpriteRenderer spr;
    // Start is called before the first frame update
    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(appear)
        {
            player.ReceiveDamage(7843);
            player.outOfBounds = false;
            appear = false;
            /*
            player.outOfBounds = false;

            transform.parent = vortex;
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.zero;
            spr.color = Color.Lerp(spr.color, Color.white, 0.05f);

            countdown -= Time.deltaTime;

            if(countdown <= 0)
            {
                
                spr.color = Color.clear;
            }
            */
        }
    }
}
