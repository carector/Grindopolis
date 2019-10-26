using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempleExplode : MonoBehaviour
{
    public bool activate;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(activate)
        {
            Collider[] col = Physics.OverlapSphere(transform.position, 50);

            foreach(Collider c in col)
            {
                if(c.name == "TempleExplosionCube")
                {
                    c.GetComponent<Rigidbody>().isKinematic = false;
                    c.GetComponent<Rigidbody>().AddExplosionForce(2000, transform.position, 50);
                }
                else if(c.GetComponent<PlayerControllerRigidbody>() != null && c.GetComponent<PlayerControllerRigidbody>().combatSettings.isDead == false)
                {
                    c.GetComponent<PlayerControllerRigidbody>().ReceiveDamage(250);
                }
            }
        }
    }
}
