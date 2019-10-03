using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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
        public float detectionRange;
    }

    [System.Serializable]
    public class EnemySounds
    {
        public AudioClip punchSound;
        public AudioClip bonkSound;
        public AudioClip fireSound;
        public AudioClip noticeSound;
        public AudioClip deathSound;
    }

    Animator anim;
    AudioSource audio;
    NavMeshAgent nav;
    NPCAnimations anims;
    Rigidbody rb;

    bool canAttack = true;
    bool isDying;

    bool receivingFireAttack;
    bool receivingObjectAttack;

    public GameObject currentTarget;
    public GameObject hitMarker;
    public Transform hitMarkerPosition;
    public bool followNearbyEnemies;

    public EnemyStats stats;
    public EnemySounds enemySounds;

    public Transform body;
    public Transform head;

    Slider sl;
    int storedMaxHealth;


    // Start is called before the first frame update
    void Start()
    {
        body.localPosition = new Vector3(0, -3.53f, -0.763f);
        head.localPosition = new Vector3(0, -0.397f, 0);

        nav = GetComponent<NavMeshAgent>();
        audio = GetComponent<AudioSource>();
        //anim = GetComponentInChildren<Animator>();
        anims = GetComponent<NPCAnimations>();
        rb = GetComponent<Rigidbody>();
        sl = GetComponentInChildren<Slider>();

        storedMaxHealth = stats.health;

        nav.speed = stats.speed;
    }

    // Update is called once per frame
    void Update()
    {
        // Check to see if we're dead
        if (stats.health <= 0)
        {
            isDying = true;
        }

        else
        {
            sl.value = (float)stats.health / storedMaxHealth;
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.01f);
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, 0.05f);

            Collider[] cols = Physics.OverlapSphere(transform.position, stats.detectionRange);


            foreach (Collider c in cols)
            {
                // Check for players to target
                if (c.transform.tag == "Player" && c.GetComponent<PlayerControllerRigidbody>().combatSettings.health > 0)
                {
                    if (currentTarget == null)
                    {
                        audio.PlayOneShot(enemySounds.noticeSound, 0.8f);
                    }
                    currentTarget = c.gameObject;
                    UpdateTarget();
                    break;
                }

                // Also check nearby enemies to see if they're following a player
                if (c.GetComponent<EnemyControl>() != null && currentTarget == null && followNearbyEnemies)
                {
                    if (c.GetComponent<EnemyControl>().currentTarget != null)
                    {
                        currentTarget = c.GetComponent<EnemyControl>().currentTarget;
                        UpdateTarget();
                        break;
                    }
                }
            }

            if(currentTarget != null)
            {
                body.localPosition = Vector3.Lerp(body.localPosition, new Vector3(0, 0, -0.763f), 0.1f);
                head.localPosition = Vector3.Lerp(head.localPosition, new Vector3(0, 3.132f, 0), 0.1f);
            }

            // If we still haven't found a target, stop following
            if (currentTarget == null || currentTarget.GetComponent<PlayerControllerRigidbody>().combatSettings.health <= 0)
            {
                RemoveTarget();
            }

            // Otherwise, attack when we're close to the player
            else if (Vector3.Distance(currentTarget.transform.position, transform.position) <= stats.range)
            {
                if (canAttack)
                {
                    StartCoroutine(DealDamage());
                    canAttack = false;
                }
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
        //anim.SetBool("Moving", false);
    }

    public void ReceiveDamage(int damage, Transform tr)
    {
        stats.health -= damage;
        DamageTextScript d = Instantiate(hitMarker, new Vector3(hitMarkerPosition.transform.position.x + Random.Range(-0.5f, 0.5f), hitMarkerPosition.transform.position.y, hitMarkerPosition.transform.position.z + Random.Range(-0.5f, 0.5f)), Quaternion.LookRotation(transform.position - currentTarget.transform.position), transform).GetComponent<DamageTextScript>();
        d.RecieveDamage(damage, currentTarget.transform);

        Vector3 damageForce = new Vector3(-tr.right.x, 1, -tr.right.z).normalized * 20;

        rb.AddForce(damageForce, ForceMode.VelocityChange);

        if (stats.health <= 0)
        {
            StartCoroutine(DeathSequence());
        }
    }

    // Same as above, but with no knockback
    public void ReceiveDamage(int damage)
    {
        stats.health -= damage;
        DamageTextScript d = Instantiate(hitMarker, new Vector3(hitMarkerPosition.transform.position.x + Random.Range(-0.5f, 0.5f), hitMarkerPosition.transform.position.y, hitMarkerPosition.transform.position.z + Random.Range(-0.5f, 0.5f)), Quaternion.LookRotation(transform.position - currentTarget.transform.position), transform).GetComponent<DamageTextScript>();
        d.RecieveDamage(damage, currentTarget.transform);

        if (stats.health <= 0 && !isDying)
        {
            StartCoroutine(DeathSequence());
        }
    }

    // Fire damage (damage over time, several short bursts of low damage)
    public IEnumerator ReceiveFireDamage(int damage, Transform tr)
    {
        if (!receivingFireAttack && !isDying)
        {
            receivingFireAttack = true;

            for (int i = 0; i <= 3; i++)
            {
                if (i == 0)
                {
                    ReceiveDamage(Mathf.RoundToInt(Random.Range(damage * 0.5f, damage)), tr);
                }
                else
                {
                    ReceiveDamage(Mathf.RoundToInt(Random.Range(damage * 0.5f, damage)));
                }

                yield return new WaitForSeconds(0.5f);
            }

            receivingFireAttack = false;
        }


    }
    public IEnumerator ReceiveObjectDamage(int damage)
    {
        if (!receivingObjectAttack && !isDying)
        {
            receivingObjectAttack = true;

            ReceiveDamage(damage);
            audio.PlayOneShot(enemySounds.bonkSound);
            yield return new WaitForSeconds(0.25f);

            receivingObjectAttack = false;
        }


    }

    // What happens when we die
    IEnumerator DeathSequence()
    {
        audio.PlayOneShot(enemySounds.deathSound);
        nav.enabled = false;
        anims.rotateHead = false;
        anims.blink = false;
        anims.shutEyes = true;

        Destroy(sl.gameObject);

        if (currentTarget != null)
        {
            Vector3 damageForce = (currentTarget.transform.position - transform.position).normalized * rb.mass;

            rb.AddForce(damageForce, ForceMode.VelocityChange);
        }
        else
        {

        }


        yield return new WaitForSeconds(5);

        Destroy(this.gameObject);
    }

    IEnumerator DealDamage()
    {
        int dam = Mathf.RoundToInt(Random.Range(stats.damage * 0.75f, stats.damage * 1.25f));

        PlayerControllerRigidbody pcr = currentTarget.GetComponent<PlayerControllerRigidbody>();

        // If the attack will kill the player, don't play a sound effect and don't add knockback to the player
        Vector3 damageForce = new Vector3(transform.forward.x, 1 * stats.verticalForce, transform.forward.z).normalized * stats.attackForce;

        if (pcr.combatSettings.health - dam > 0)
        {
            audio.PlayOneShot(enemySounds.punchSound);
        }

        pcr.ReceiveDamage(dam);

        if(pcr.combatSettings.health <= 0)
        {
            pcr.StartCoroutine(pcr.DeathSequence(damageForce));
        }
        else
        {
            currentTarget.GetComponent<Rigidbody>().AddForce(damageForce, ForceMode.VelocityChange);
        }

        yield return new WaitForSeconds(Random.Range(0.25f, 1.25f));

        canAttack = true;
    }
}
