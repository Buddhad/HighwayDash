using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("PowerUp Settings")]
    public GameObject[] powerUpPrefabs;   // Array of power-up prefabs to spawn
    public float spawnInterval = 4f;      // Time interval between spawns
    public float spawnY = 6f;             // Y position where power-ups will spawn
    public float checkDistance = 3f;      // Distance to check for traffic cars in a lane before spawning


    [Header("Detection Settings")]
    [SerializeField] private LayerMask trafficLayer;
    [SerializeField] private LayerMask powerUpLayer;      // LayerMask to detect power-ups
    [SerializeField] private float overlapBoxWidth = 2f; // Width of the OverlapBox check area

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;   // Toggle for showing debug info in console

    private float timer;   // Timer to track spawn intervals
    private float[] lanePositions = { -2f, 0f, 2f };  // X positions of player lanes (Left, Middle, Right)

    void Update()
    {
        // Increase timer every frame
        timer += Time.deltaTime;

        // If timer reaches interval, try spawning power-up
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnPowerUp();
        }
    }

    void TrySpawnPowerUp()
    {
        List<float> freeLanes = new List<float>();

        // Check each lane for traffic cars using OverlapBox
        foreach (float laneX in lanePositions)
        {
            Vector2 checkCenter = new Vector2(laneX, spawnY - checkDistance / 2f);
            Vector2 checkSize = new Vector2(overlapBoxWidth, checkDistance);

            // Only check against traffic
            Collider2D hitTraffic = Physics2D.OverlapBox(checkCenter, checkSize, 0f, trafficLayer);
            // Optional: Check if powerup already spawned there
            Collider2D hitPowerUp = Physics2D.OverlapBox(checkCenter, checkSize, 0f, powerUpLayer);

            if (hitTraffic == null && hitPowerUp == null)
            {
                freeLanes.Add(laneX);
            }
            else
            {
                if (showDebugInfo)
                {
                    if (hitTraffic != null) Debug.Log("Lane blocked by Traffic at " + laneX);
                    if (hitPowerUp != null) Debug.Log("Lane already has a PowerUp at " + laneX);
                }
            }
        }
        // Spawn power-up if at least one lane is free
        if (freeLanes.Count > 0)
        {
            float selectedLane = freeLanes[Random.Range(0, freeLanes.Count)];
            int index = Random.Range(0, powerUpPrefabs.Length);
            // Spawn position at the selected lane
            float safeSpawnY = spawnY + 3f; // ✅ Adjust spawn Y to avoid overlap with traffic
            Vector3 spawnPos = new Vector3(selectedLane, safeSpawnY, 0); // ✅ Use selected lane X position

            Instantiate(powerUpPrefabs[index], spawnPos, Quaternion.identity);

            if (showDebugInfo)
                Debug.Log("Spawned PowerUp at Lane: " + selectedLane);
        }
        else
        {
            if (showDebugInfo)
                Debug.Log("No free lane found. PowerUp not spawned.");
        }
    }

    // Visualize OverlapBox areas in Scene view (Editor only)
    void OnDrawGizmosSelected()
    {
        if (lanePositions == null) return;

        Gizmos.color = Color.cyan;
        foreach (float laneX in lanePositions)
        {
            Vector3 center = new Vector3(laneX, spawnY - checkDistance / 2f, 0);
            Vector3 size = new Vector3(overlapBoxWidth, checkDistance, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
