using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenGate : MonoBehaviour
{
    public InteractableNPC npc;
    public float stoppingPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(npc.lines.Length == 1)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.025f);

            if(transform.position.z >= stoppingPoint)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, stoppingPoint);
                this.enabled = false;
            }
        }
    }
}
