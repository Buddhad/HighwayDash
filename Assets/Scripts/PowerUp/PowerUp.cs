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
            ApplyPowerUp(other);
            Destroy(gameObject);
        }
    }

    void ApplyPowerUp(Collider2D player)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        
        if (playerController != null)
        {
            playerController.ActivatePowerUp(powerUpType, duration);
            Debug.Log($"PowerUp {powerUpType} applied to player for {duration} seconds");
        }
        else
        {
            Debug.LogError("PlayerController component not found on player!");
        }
    }
}