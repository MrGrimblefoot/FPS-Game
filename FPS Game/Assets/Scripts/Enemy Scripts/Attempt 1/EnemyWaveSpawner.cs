using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyWaveSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [Range(1,5)] [SerializeField] private int minSpawnAmount;
    [Range(1,9)] [SerializeField] private int maxSpawnAmount;
    public string enemyToSpawn;

    private int waveCount;
    [SerializeField] private int maxWaves;

    [SerializeField] private float spawnTime;
    [SerializeField] private float startSpawnTime;

    void Start()
    {
        spawnTime = startSpawnTime;
    }

    void Update()
    {
        if (spawnTime > 0) { spawnTime -= Time.deltaTime; }
        else { if (waveCount < maxWaves) { SpawnEnemies(); spawnTime = startSpawnTime; } }
    }

    private void SpawnEnemies()
    {
        waveCount++;

        int tempSpawnAmount = Random.Range(minSpawnAmount, maxSpawnAmount);

        for (int i = 0; i < tempSpawnAmount; i++)
        {
            Transform tempSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemy = PhotonNetwork.Instantiate(enemyToSpawn, tempSpawnPoint.position, tempSpawnPoint.rotation);
        }
    }
}
