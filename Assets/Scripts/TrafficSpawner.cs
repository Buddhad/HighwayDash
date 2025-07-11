using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficSpawner : MonoBehaviour
{
    public static TrafficSpawner Instance;

    [Header("Spawning Settings")]
    [SerializeField] private GameObject[] trafficCarPrefabs;
    [SerializeField] private float initialSpawnInterval = 5f; // Start very slow
    [SerializeField] private float spawnY = 12f;
    [SerializeField] private float[] lanePositions = { -2f, 0f, 2f };
    [SerializeField] private float minDistanceBetweenCars = 3f; // Minimum distance between cars

    [Header("Difficulty")]
    //[SerializeField] private float difficultyIncreaseRate = 0.05f; // Gradual increase
    [SerializeField] private float minSpawnInterval = 0.8f; // Minimum interval (fastest spawning)
    [SerializeField] private float difficultyRampUpTime = 30f; // Time to reach maximum difficulty

    private List<GameObject> carPool = new List<GameObject>();
    private float spawnTimer;
    private float gameTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializePool()
    {
        for (int i = 0; i < 15; i++) // Increased pool size
        {
            GameObject car = Instantiate(trafficCarPrefabs[Random.Range(0, trafficCarPrefabs.Length)]);
            car.SetActive(false);
            carPool.Add(car);
        }
    }

    void Update()
    {
        if (GameManager.Instance.IsGameActive())
        {
            gameTime += Time.deltaTime;
            spawnTimer += Time.deltaTime;

            // Progressive difficulty increase with smooth curve
            float difficultyProgress = Mathf.Min(1f, gameTime / difficultyRampUpTime);
            float currentSpawnInterval = Mathf.Lerp(initialSpawnInterval, minSpawnInterval, difficultyProgress * difficultyProgress);

            if (spawnTimer >= currentSpawnInterval)
            {
                SpawnTrafficCar();
                spawnTimer = 0f;
            }
        }
    }

    void SpawnTrafficCar()
    {
        // Try to find a lane that's safe to spawn in
        List<int> availableLanes = new List<int>();
        
        for (int i = 0; i < lanePositions.Length; i++)
        {
            if (IsLaneSafeToSpawn(lanePositions[i]))
            {
                availableLanes.Add(i);
            }
        }

        // If no lanes are safe, skip this spawn
        if (availableLanes.Count == 0)
        {
            return;
        }

        GameObject car = GetPooledCar();
        if (car != null)
        {
            int randomLaneIndex = Random.Range(0, availableLanes.Count);
            int selectedLane = availableLanes[randomLaneIndex];
            Vector3 spawnPosition = new Vector3(lanePositions[selectedLane], spawnY, 0);

            car.transform.position = spawnPosition;
            car.GetComponent<TrafficCar>().ResetCar();
            car.SetActive(true);
        }
    }

    bool IsLaneSafeToSpawn(float laneX)
    {
        // Check if there's any car too close to the spawn position
        foreach (GameObject car in carPool)
        {
            if (car.activeInHierarchy)
            {
                float carX = car.transform.position.x;
                float carY = car.transform.position.y;
                
                // Check if car is in the same lane and too close to spawn position
                if (Mathf.Abs(carX - laneX) < 0.5f && // Same lane (with tolerance)
                    Mathf.Abs(carY - spawnY) < minDistanceBetweenCars) // Too close vertically
                {
                    return false;
                }
            }
        }
        return true;
    }

    GameObject GetPooledCar()
    {
        foreach (GameObject car in carPool)
        {
            if (!car.activeInHierarchy)
            {
                return car;
            }
        }

        // If no car available, create a new one
        GameObject newCar = Instantiate(trafficCarPrefabs[Random.Range(0, trafficCarPrefabs.Length)]);
        carPool.Add(newCar);
        return newCar;
    }

    public void ReturnToPool(GameObject car)
    {
        car.SetActive(false);
        // Removed score addition from here - it should be handled when player passes cars
    }

    public void ResetSpawner()
    {
        gameTime = 0f;
        spawnTimer = 0f;

        foreach (GameObject car in carPool)
        {
            car.SetActive(false);
        }
    }
}