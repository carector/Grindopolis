using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAnimations : MonoBehaviour
{
    public bool rotateHead;
    public bool blink;
    public float minRotateDistance = 10;
    public GameObject head;
    public GameObject leftEye;
    public GameObject rightEye;

    GameObject player;
    Transform playerCam;
    Vector3 eyeSize;

    // Start is called before the first frame update
    void Start()
    {
        eyeSize = rightEye.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if(playerCam == null)
        {
            player = GameObject.Find("ClientPlayer");

            // If we haven't found our player, return so we don't cause an error looking for our camera's transform
            if (player == null)
                return;

            playerCam = player.GetComponentInChildren<Camera>().transform;

            if (blink)
            {
                StartCoroutine(RandomBlinks());
            }
        }

        else if(Vector3.Distance(transform.position, playerCam.position) <= minRotateDistance && rotateHead)
        {
            MoveHead();
        }

        if (blink)
        {
            // Constantly set our eyes to their default sizes
            leftEye.transform.localScale = Vector3.Lerp(leftEye.transform.localScale, eyeSize, 0.2f);
            rightEye.transform.localScale = Vector3.Lerp(rightEye.transform.localScale, eyeSize, 0.2f);
        }
    }

    void MoveHead()
    {
        Vector3 target = playerCam.transform.position - head.transform.position;
        Quaternion lookAt = Quaternion.LookRotation(target);

        head.transform.rotation = Quaternion.Lerp(head.transform.rotation, lookAt, 0.08f);
    }

    IEnumerator RandomBlinks()
    {
        yield return new WaitForSeconds(Random.Range(1, 6));

        leftEye.transform.localScale = new Vector3(eyeSize.x, eyeSize.y, 0.02f);
        rightEye.transform.localScale = new Vector3(eyeSize.x, eyeSize.y, 0.02f);

        StartCoroutine(RandomBlinks());

    }
    
}
