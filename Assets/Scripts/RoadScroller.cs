using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RoadScroller : MonoBehaviour
{
    [Header("Scrolling Settings")]
    [SerializeField] private float scrollSpeed = 5f;

    [Header("Road Dimensions")]
    [SerializeField] private float roadHeight = 10f; // Height of your road sprite

    [Header("Reset Positions")]
    [SerializeField] private float resetPositionY = -10f; // When to reset (usually -roadHeight)
    [SerializeField] private float startPositionY = 10f;  // Where to reset to (usually +roadHeight)

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private bool fitToScreen = true;

    private SpriteRenderer spriteRenderer;
    private float actualRoadHeight;

    void Start()
    {
        // Get sprite renderer to calculate actual road height
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("RoadScroller: SpriteRenderer component is required!");
            return;
        }

        // Fit sprite to screen if enabled
        if (fitToScreen)
        {
            FitSpriteToScreen();
        }

        if (spriteRenderer.sprite != null)
        {
            // Calculate actual height based on sprite bounds (after scaling)
            actualRoadHeight = spriteRenderer.bounds.size.y;

            // Auto-adjust reset positions based on actual road height
            if (resetPositionY == -10f && startPositionY == 10f) // Default values
            {
                resetPositionY = -actualRoadHeight;
                startPositionY = actualRoadHeight;
            }

            if (showDebugInfo)
            {
                Debug.Log($"Road Height: {actualRoadHeight}, Reset Y: {resetPositionY}, Start Y: {startPositionY}");
            }
        }
        else
        {
            Debug.LogWarning("RoadScroller: No Sprite found! Using manual roadHeight value.");
            actualRoadHeight = roadHeight;
        }
    }

    void FitSpriteToScreen()
    {
        if (spriteRenderer == null || Camera.main == null)
        {
            Debug.LogError("RoadScroller: Cannot fit to screen - missing SpriteRenderer or Main Camera!");
            return;
        }

        float worldHeight = Camera.main.orthographicSize * 2f;
        float worldWidth = worldHeight * Screen.width / Screen.height;

        // Calculate scale based on original sprite bounds (before any scaling)
        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.one; // Reset to get original bounds

        float originalWidth = spriteRenderer.bounds.size.x;
        float originalHeight = spriteRenderer.bounds.size.y;

        // Restore original scale before applying new scale
        transform.localScale = originalScale;

        // Calculate new scale
        Vector3 newScale = new Vector3(
            (worldWidth / originalWidth) * originalScale.x,
            (worldHeight / originalHeight) * originalScale.y,
            originalScale.z
        );

        transform.localScale = newScale;

        if (showDebugInfo)
        {
            Debug.Log($"Fitted to screen. New scale: {newScale}, World size: {worldWidth}x{worldHeight}");
        }
    }

    void Update()
    {
        // Check if game is active (with null safety)
        if (GameManager.Instance != null && GameManager.Instance.IsGameActive())
        {
            // Move road downward
            transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);

            // Reset position when off screen
            if (transform.position.y <= resetPositionY)
            {
                Vector3 newPosition = new Vector3(
                    transform.position.x,
                    startPositionY,
                    transform.position.z
                );
                transform.position = newPosition;

                if (showDebugInfo)
                {
                    Debug.Log($"Road reset to position: {newPosition}");
                }
            }
        }
    }

    // Helper method to set up road positions automatically
    [ContextMenu("Auto Setup Road Positions")]
    void AutoSetupPositions()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            actualRoadHeight = spriteRenderer.bounds.size.y;
            resetPositionY = -actualRoadHeight;
            startPositionY = actualRoadHeight;

            Debug.Log($"Auto-setup complete. Road Height: {actualRoadHeight}");
        }
        else
        {
            Debug.LogError("Cannot auto-setup: No SpriteRenderer or Sprite found!");
        }
    }

    // Helper method to manually fit sprite to screen
    [ContextMenu("Fit Sprite to Screen")]
    void ManualFitToScreen()
    {
        FitSpriteToScreen();
    }

    // Visualize the reset boundaries in Scene view
    void OnDrawGizmos()
    {
        if (!showDebugInfo) return;

        Gizmos.color = Color.red;
        // Draw reset line
        Vector3 resetLineStart = new Vector3(transform.position.x - 2f, resetPositionY, transform.position.z);
        Vector3 resetLineEnd = new Vector3(transform.position.x + 2f, resetPositionY, transform.position.z);
        Gizmos.DrawLine(resetLineStart, resetLineEnd);

        Gizmos.color = Color.green;
        // Draw start line
        Vector3 startLineStart = new Vector3(transform.position.x - 2f, startPositionY, transform.position.z);
        Vector3 startLineEnd = new Vector3(transform.position.x + 2f, startPositionY, transform.position.z);
        Gizmos.DrawLine(startLineStart, startLineEnd);
    }
}