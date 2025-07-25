using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficCar : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float destroyY = -10f;
    
    [Header("Scoring")]
    [SerializeField] private float scoreY = 0f; // Y position where score is awarded (player car position)
    
    private bool hasBeenPassed = false; // Track if this car has already been passed
    
    void Update()
    {
        // Move the car downward
        transform.Translate(Vector3.down * speed * Time.deltaTime);
        
        // Check if player has passed this car
        if (!hasBeenPassed && transform.position.y < scoreY)
        {
            hasBeenPassed = true;
            GameManager.Instance.AddScore(10); // Award points for passing
        }
        
        // Return to pool when off screen
        if (transform.position.y < destroyY)
        {
            TrafficSpawner.Instance.ReturnToPool(gameObject);
        }
    }
    
    public void ResetCar()
    {
        hasBeenPassed = false; // Reset the passed flag
    }
    
    // REMOVED: OnTriggerEnter2D method
    // The collision detection is now handled entirely by the PlayerController
    // This prevents the traffic car from immediately triggering game over
    // before the player's shield can be checked
}