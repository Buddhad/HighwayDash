using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { SpeedBoost, Shield, ScoreMultiplier }
    
    [Header("PowerUp Settings")]
    [SerializeField] private PowerUpType powerUpType;
    [SerializeField] private float duration = 5f;
    [SerializeField] private float moveSpeed = 5f;

    void Update()
    {
        // Move down with traffic
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);

        // Despawn when off screen
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyPowerUp();
            Destroy(gameObject);
        }
    }

    void ApplyPowerUp()
    {
        switch (powerUpType)
        {
            case PowerUpType.SpeedBoost:
                // Implement speed boost logic
                break;
            case PowerUpType.Shield:
                // Implement shield logic
                break;
            case PowerUpType.ScoreMultiplier:
                // Implement score multiplier logic
                break;
        }
    }
}
