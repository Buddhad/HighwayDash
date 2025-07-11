using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public GameObject[] powerUpPrefabs;
    public float spawnInterval = 10f;
    public float minX = -2f;
    public float maxX = 2f;
    public float spawnY = 6f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnPowerUp();
        }
    }

    void SpawnPowerUp()
    {
        int index = Random.Range(0, powerUpPrefabs.Length);
        Vector3 spawnPos = new Vector3(Random.Range(minX, maxX), spawnY, 0);
        Instantiate(powerUpPrefabs[index], spawnPos, Quaternion.identity);
    }
}
