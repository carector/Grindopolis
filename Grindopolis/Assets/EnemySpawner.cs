using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyToSpawn;
    public List<GameObject> currentEnemies;
    public int maxEnemies;
    public float spawnRadius;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < maxEnemies; i++)
        {
            InstantiateNewEnemy();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InstantiateNewEnemy()
    {
        float posX = Random.Range(-spawnRadius, spawnRadius);
        float posZ = Random.Range(-spawnRadius, spawnRadius);
        Vector3 spawnPosition = new Vector3(transform.position.x + posX, 0, transform.position.z + posZ);


        GameObject newEnemy = Instantiate(enemyToSpawn, spawnPosition, Quaternion.identity);
        currentEnemies.Add(newEnemy);
    }
}
