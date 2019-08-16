using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCAnimations : MonoBehaviour
{
    public bool rotateHead;
    public bool blink;
    public bool shutEyes;
    public bool randomMovements;
    public float movementRange;

    public float minRotateDistance = 10;
    public GameObject head;
    public GameObject leftEye;
    public GameObject rightEye;

    EnemyControl e;
    NavMeshAgent n;

    public bool isMoving;
    GameObject player;
    Transform playerCam;
    Vector3 eyeSize;
    Vector3 shutEyeSize;
    Vector3 originalPos;

    // Start is called before the first frame update
    void Start()
    {
        originalPos = transform.position;
        if(GetComponent<NavMeshAgent>() != null)
            n = GetComponent<NavMeshAgent>();

        eyeSize = rightEye.transform.localScale;
        shutEyeSize = new Vector3(eyeSize.x, eyeSize.y, 0.02f);
        if(GetComponent<EnemyControl>() != null)
        {
            e = GetComponent<EnemyControl>();
            minRotateDistance = e.stats.detectionRange;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCam == null)
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

            else if (shutEyes)
            {
                leftEye.transform.localScale = Vector3.Lerp(leftEye.transform.localScale, shutEyeSize, 0.2f);
                rightEye.transform.localScale = Vector3.Lerp(rightEye.transform.localScale, shutEyeSize, 0.2f);
            }
        }

        else if (Vector3.Distance(transform.position, playerCam.position) <= minRotateDistance && rotateHead)
        {
            MoveHead();
        }
        /*
        if (randomMovements)
        {
            if (!isMoving)
            {
                StartCoroutine(RandomPos());
                isMoving = true;
            }
        }*/

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

    IEnumerator RandomPos()
    {
        
        Vector3 newPos = new Vector3(originalPos.x + Random.Range(-movementRange, movementRange), originalPos.z + Random.Range(-movementRange, movementRange));
        print("Moving to position " + newPos);
        n.SetDestination(newPos);

        while (!n.isStopped)
            yield return null;

        yield return new WaitForSeconds(Random.Range(2, 4));
        isMoving = false;
    }

    IEnumerator RandomBlinks()
    {
        yield return new WaitForSeconds(Random.Range(1, 6));

        leftEye.transform.localScale = new Vector3(eyeSize.x, eyeSize.y, 0.02f);
        rightEye.transform.localScale = new Vector3(eyeSize.x, eyeSize.y, 0.02f);

        StartCoroutine(RandomBlinks());

    }
    
}
