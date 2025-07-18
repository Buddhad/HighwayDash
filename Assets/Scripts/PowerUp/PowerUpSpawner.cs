using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("PowerUp Settings")]
    public GameObject[] powerUpPrefabs;   // Array of power-up prefabs to spawn
    public float spawnInterval = 4f;      // Time interval between spawns
    public float spawnY = 6f;             // Base Y position where power-ups will spawn
    [SerializeField] private float spawnYOffset = 3f; // Offset added to spawnY for safe spawning (adjust to avoid traffic)
    public float checkDistance = 5f;      // Height of the area to check for traffic/power-ups (increase to cover more space)

    [Header("Detection Settings")]
    [SerializeField] private LayerMask trafficLayer; // Layer for detecting traffic cars
    [SerializeField] private LayerMask powerUpLayer; // Layer for detecting existing power-ups
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
        List<float> safeLanes = new List<float>();

        // The Y position at which power-ups will actually spawn
        float spawnPosY = spawnY + spawnYOffset;

        // How far up/down to look for traffic cars when deciding if it's safe
        float safetyBuffer = 4f; // Increase this value to create a larger exclusion zone (helps prevent spawns too close to moving traffic)

        foreach (float laneX in lanePositions)
        {
            // The center of the box we're checking for traffic cars
            Vector2 checkCenter = new Vector2(laneX, spawnPosY);
            // Height of the exclusion zone: covers above and below where the power-up would spawn
            float exclusionZoneHeight = checkDistance + safetyBuffer * 2f;

            Vector2 checkSize = new Vector2(overlapBoxWidth, exclusionZoneHeight);

            // Check for traffic in the wider, taller exclusion area
            Collider2D hitTraffic = Physics2D.OverlapBox(checkCenter, checkSize, 0f, trafficLayer);
            // Optionally, also check for existing power-ups (to avoid double-spawning)
            Collider2D hitPowerUp = Physics2D.OverlapBox(checkCenter, checkSize, 0f, powerUpLayer);

            if (hitTraffic == null && hitPowerUp == null)
            {
                safeLanes.Add(laneX);
            }
            else
            {
                if (showDebugInfo)
                {
                    if (hitTraffic != null) Debug.Log($"Lane {laneX} blocked by NEARBY Traffic: {hitTraffic.name}");
                    if (hitPowerUp != null) Debug.Log($"Lane {laneX} already has a PowerUp nearby: {hitPowerUp.name}");
                }
            }
        }

        // Only spawn a powerup if there is a truly safe lane
        if (safeLanes.Count > 0)
        {
            StartCoroutine(SpawnWithDelay(safeLanes, spawnPosY));
        }
        else
        {
            if (showDebugInfo)
                Debug.Log("No safe lane for PowerUp spawn.");
        }
    }

    private IEnumerator SpawnWithDelay(List<float> safeLanes, float spawnPosY)
    {
        // Wait a small random delay to improve "accuracy" of safety checks (gives time for traffic to move past if close)
        yield return new WaitForSeconds(Random.Range(0.08f, 0.15f));

        // Pick a lane from still-safe options (repeat check for ultimate safety)
        float chosenLane = safeLanes[Random.Range(0, safeLanes.Count)];
        Vector2 checkCenter = new Vector2(chosenLane, spawnPosY);
        float safetyBuffer = 3.0f; // Reuse the buffer value for consistency
        float exclusionZoneHeight = checkDistance + safetyBuffer * 2f;
        Vector2 checkSize = new Vector2(overlapBoxWidth, exclusionZoneHeight);

        Collider2D hitTraffic = Physics2D.OverlapBox(checkCenter, checkSize, 0f, trafficLayer);
        Collider2D hitPowerUp = Physics2D.OverlapBox(checkCenter, checkSize, 0f, powerUpLayer);

        if (hitTraffic == null && hitPowerUp == null)
        {
            // Safe! Spawn the powerup prefab here
            int index = Random.Range(0, powerUpPrefabs.Length);
            Vector3 spawnPos = new Vector3(chosenLane, spawnPosY, 0);

            Instantiate(powerUpPrefabs[index], spawnPos, Quaternion.identity);

            if (showDebugInfo)
                Debug.Log($"Spawned PowerUp at Lane: {chosenLane}, Position: {spawnPos}");
        }
        else
        {
            if (showDebugInfo)
                Debug.Log("Spawn cancelled: Traffic or PowerUp too close at spawn time (last check).");
        }
    }

    // Visualize OverlapBox areas in Scene view (Editor only)
    void OnDrawGizmosSelected()
    {
        if (lanePositions == null) return;

        float safeSpawnY = spawnY + spawnYOffset; // Use the actual spawn Y for Gizmos

        Gizmos.color = Color.cyan;
        foreach (float laneX in lanePositions)
        {
            Vector3 center = new Vector3(laneX, safeSpawnY, 0); // Center Gizmo at spawn position
            Vector3 size = new Vector3(overlapBoxWidth, checkDistance, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
