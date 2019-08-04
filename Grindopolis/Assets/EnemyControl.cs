using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyControl : MonoBehaviour
{    
    [System.Serializable]
    public class EnemyStats
    {
        public int health;
        public int damage;
        public int attackForce;
        public float verticalForce = 1;
        public float speed;
        public float range;
    }

    [System.Serializable]
    public class EnemySounds
    {
        public AudioClip punchSound;
    }

    Animator anim;
    AudioSource audio;
    NavMeshAgent nav;
    bool canAttack = true;

    public GameObject currentTarget;
    public GameObject hitMarker;
    public Transform hitMarkerPosition;
    public EnemyStats enemyStats;
    public EnemySounds enemySounds;

    // Start is called before the first frame update
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        audio = GetComponent<AudioSource>();
        anim = GetComponentInChildren<Animator>();

        nav.speed = enemyStats.speed;
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, 10);

        foreach(Collider c in cols)
        {
            if(c.transform.tag == "Player")
            {
                currentTarget = c.gameObject;
                UpdateTarget();
                break;
            }
        }

        // If we still haven't found a target, stop following
        if (currentTarget == null)
        {
            RemoveTarget();
        }
        // Otherwise, attack when we're close to the player
        else if(Vector3.Distance(currentTarget.transform.position, transform.position) <= enemyStats.range)
        {
            if(canAttack)
            {
                StartCoroutine(DealDamage());
                canAttack = false;
            }
        }

        
    }

    void UpdateTarget()
    {
        nav.isStopped = false;
        nav.SetDestination(currentTarget.transform.position);
        //anim.SetBool("Moving", true);
    }

    void RemoveTarget()
    {
        nav.isStopped = true;
        anim.SetBool("Moving", false);
    }

    public void ReceiveDamage(int damage)
    {
        enemyStats.health -= damage;
        DamageTextScript d = Instantiate(hitMarker, new Vector3(hitMarkerPosition.transform.position.x + Random.Range(-0.25f, 0.25f), hitMarkerPosition.transform.position.y, hitMarkerPosition.transform.position.z + Random.Range(-0.25f, 0.25f)), Quaternion.LookRotation(transform.position - currentTarget.transform.position), transform).GetComponent<DamageTextScript>();
        d.RecieveDamage(damage, currentTarget.transform);

    }

    IEnumerator DealDamage()
    {
        print("Dealt damage");
        audio.PlayOneShot(enemySounds.punchSound);
        Vector3 damageForce = new Vector3(transform.forward.x, 1 * enemyStats.verticalForce, transform.forward.z).normalized * enemyStats.attackForce;

        currentTarget.GetComponent<Rigidbody>().AddForce(damageForce, ForceMode.VelocityChange);
        currentTarget.GetComponent<PlayerControllerRigidbody>().ReceiveDamage(enemyStats.damage);

        yield return new WaitForSeconds(Random.Range(0.25f, 1.25f));

        canAttack = true;
    }
}
