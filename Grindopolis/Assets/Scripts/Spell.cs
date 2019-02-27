using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used to store spell / attack data for when the player casts one
public class Spell : MonoBehaviour
{
    public int damage;
    public float duration;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        duration -= Time.deltaTime;

        if(duration <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
