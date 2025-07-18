using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target; // The target (e.g., player) for the camera to follow
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Offset from the target position
    public float smoothSpeed = 0.125f; // Speed of smoothing when following the target
    public float roadScrollSpeed = 5f; // Speed to sync with road scrolling (match your RoadScroller's scrollSpeed)

    [Header("Tilt Settings")]
    public float tiltAmount = 1f; // Amount of horizontal shift for tilt effect (adjust for subtlety; e.g., 0.5f-2f)
    public float tiltSpeed = 5f; // Speed of the tilt animation

    [Header("Shake Settings")]
    public float shakeDuration = 0.2f; // Duration of the shake effect
    public float shakeMagnitude = 0.1f; // Strength of the shake

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f; // Speed of the zoom animation (controls lerp in coroutine)

    private Camera cam; // Reference to the main camera

    void Start()
    {
        cam = Camera.main; // Get the main camera reference
    }

    void LateUpdate()
    {
        if (target == null) return; // Skip if no target is set

        // Calculate the desired position based on target and offset
        Vector3 desiredPosition = target.position + offset;
        // Smoothly move the camera to the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Sync camera with road scrolling to keep player centered relative to moving road
        transform.position += Vector3.down * roadScrollSpeed * Time.deltaTime;
    }

    // Call this when switching lanes (e.g., -1 for left, 1 for right)
    // This triggers a smooth horizontal shift (tilt simulation) without rotating/distorting the view
    public void TriggerTilt(float direction)
    {
        StopAllCoroutines(); // Stop any ongoing tilt coroutines
        StartCoroutine(TiltCoroutine(direction)); // Start the new tilt effect
    }

    IEnumerator TiltCoroutine(float dir)
    {
        // Store the current camera position before tilting
        Vector3 startPos = transform.position;
        // Calculate target position: Shift horizontally based on direction (e.g., left/right lean)
        Vector3 targetPos = startPos + new Vector3(tiltAmount * dir, 0f, 0f);
        float t = 0; // Animation timer

        // Smoothly move to tilted position
        while (t < 1f)
        {
            t += Time.deltaTime * tiltSpeed; // Increment timer based on tilt speed
            transform.position = Vector3.Lerp(startPos, targetPos, t); // Interpolate position for smooth movement
            yield return null; // Wait for next frame
        }

        // Brief pause at full tilt for effect
        yield return new WaitForSeconds(0.1f);

        t = 0; // Reset timer
        // Smoothly return to original position
        while (t < 1f)
        {
            t += Time.deltaTime * tiltSpeed; // Increment timer
            transform.position = Vector3.Lerp(targetPos, startPos, t); // Interpolate back to original
            yield return null; // Wait for next frame
        }

        // Ensure exact reset to avoid floating-point drift
        transform.position = startPos;
    }

    // Call this when a collision or power-up happens
    // This shakes the camera briefly for feedback
    public void TriggerShake()
    {
        StopCoroutine(nameof(ShakeCoroutine)); // Stop any ongoing shake
        StartCoroutine(ShakeCoroutine()); // Start new shake
    }

    IEnumerator ShakeCoroutine()
    {
        Vector3 originalPos = transform.position; // Store current position
        float elapsed = 0f; // Timer for shake duration

        // Apply random offsets for shake effect
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude; // Random X offset
            float y = Random.Range(-1f, 1f) * shakeMagnitude; // Random Y offset
            transform.position = originalPos + new Vector3(x, y, 0); // Apply shake
            elapsed += Time.deltaTime; // Increment timer
            yield return null; // Wait for next frame
        }

        // Reset to original position
        transform.position = originalPos;
    }

    // Call this to zoom the camera (e.g., for boost or shield)
    // This smoothly changes the orthographic size
    public void TriggerZoom(float targetSize, float duration)
    {
        StopCoroutine(nameof(ZoomCoroutine)); // Stop any ongoing zoom
        StartCoroutine(ZoomCoroutine(targetSize, duration)); // Start new zoom
    }

    IEnumerator ZoomCoroutine(float targetSize, float duration)
    {
        float startSize = cam.orthographicSize; // Current zoom size
        float t = 0; // Timer for lerp animation

        // Smoothly lerp to target zoom size
        while (t < 1f)
        {
            t += Time.deltaTime / duration; // Increment timer
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, t); // Apply zoom
            yield return null; // Wait for next frame
        }
    }
}
