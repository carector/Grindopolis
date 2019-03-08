using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used to store spell / attack data for when the player casts one
public class Spell : MonoBehaviour
{
    public int cost; // Mana cost

    public int healthModifier;


    public enum SpellTypes
    {
        projectile,
        grounded,
        healing,

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
