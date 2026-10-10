using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class SpawnGuns : NetworkBehaviour
{
    private SpawnGunManager spawnGunManager;
    [SerializeField] private List<Transform> possibleSpawnPositions = new List<Transform>();
    [SerializeField] private int NumberOfObjectsToSpawn;
    //[SerializeField] private float RandomBias;

    private void Start()
    {
        if (IsServer)
        {
            spawnGunManager = FindFirstObjectByType<SpawnGunManager>();
            NumberOfObjectsToSpawn += Random.Range(-1, 1);
            NumberOfObjectsToSpawn = Mathf.Clamp(NumberOfObjectsToSpawn, 0, 3);
            List<Transform> spawnPositions = new List<Transform>();
            for (int i = 0; i < NumberOfObjectsToSpawn; i++)
            {
                int index = Random.Range(0, possibleSpawnPositions.Count - 1);
                spawnPositions.Add(possibleSpawnPositions[index]);
                possibleSpawnPositions.RemoveAt(index);
            }

            for (int i = 0; i < spawnPositions.Count; i++)
            {
                Instantiate(spawnGunManager.spawnPrefabs[Random.Range(0, spawnGunManager.spawnPrefabs.Count)], spawnPositions[i].position, Quaternion.identity).GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
