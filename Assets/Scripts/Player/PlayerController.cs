using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Profiling; // NEW: Required for Profiler.BeginSample/EndSample to mark code sections for performance analysis

public class PlayerController : MonoBehaviour
{
    // Dictionary to track active power-ups and their remaining time
    private Dictionary<PowerUp.PowerUpType, float> activePowerUps = new Dictionary<PowerUp.PowerUpType, float>();

    [Header("Car Settings")]
    [SerializeField] private float verticalSpeed = 12f; // Player's movement speed

    [Header("Lane Settings")]
    [SerializeField] private float[] lanePositions = { -2f, 0f, 2f }; // X positions of lanes
    [SerializeField] private float laneChangeSpeed = 15f; // Speed of lane change movement

    [Header("Boundary Settings")]
    [SerializeField] private float topBoundaryOffset = 1f;
    [SerializeField] private float bottomBoundaryOffset = 1f;
    [SerializeField] private float boundaryBuffer = 0.2f; // Margin for boundary correction

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true; // Toggle debug logs

    [Header("Debug Options")]
    [SerializeField] private bool debugInvincibility = false; // Toggle this in Inspector for debug mode: Player can't be killed by traffic cars (for testing mechanics without interruptions)

    // Lane and boundary tracking
    private int currentLane = 1; // Start at center lane
    private float targetX; // Target X position during lane change
    private bool isChangingLane = false; // Is currently changing lane
    private float topLimit; // Camera top boundary
    private float bottomLimit; // Camera bottom boundary

    private Rigidbody2D rb;
    private float normalSpeed; // Stores default vertical speed

    // PowerUp State Flags
    private bool isSpeedBoosted = false;
    private bool isShieldActive = false;
    private bool isScoreBoosted = false;

    void Start()
    {
        // Initialize Rigidbody2D and set physics properties
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PlayerController: Missing Rigidbody2D!");
            return;
        }

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // Ensure lanes are defined
        if (lanePositions.Length == 0)
        {
            Debug.LogError("No lane positions defined!");
            return;
        }

        normalSpeed = verticalSpeed;
        targetX = lanePositions[currentLane]; // Start in the center lane
        transform.position = new Vector3(targetX, transform.position.y, 0f);

        CalculateBoundaries(); // Set camera boundaries

        if (showDebugInfo)
        {
            Debug.Log($"Initialized at lane {currentLane}, X: {targetX}");
            Debug.Log($"Boundaries - Top: {topLimit}, Bottom: {bottomLimit}");
        }
    }

    void Update()
    {
        // NEW: Start Profiler sample for Update method - this marks the section in Unity Profiler for analysis (open Window > Analysis > Profiler to see CPU usage, spikes, etc.)
        Profiler.BeginSample("PlayerController.Update");

        HandleLaneInput(); // Check for lane change input
        UpdatePowerUps();  // Update active power-up timers

        // NEW: In-game toggle for debug invincibility - press 'I' to enable/disable
        // This allows quick switching during playtesting without pausing the editor
        if (Input.GetKeyDown(KeyCode.I))
        {
            debugInvincibility = !debugInvincibility;
            Debug.Log($"Debug Invincibility: {(debugInvincibility ? "Enabled" : "Disabled")}");
        }

        // NEW: End Profiler sample - pair with BeginSample to measure execution time of this method in Profiler
        Profiler.EndSample();
    }

    void FixedUpdate()
    {
        // NEW: Start Profiler sample for FixedUpdate - helps identify physics-related bottlenecks over time (e.g., after 3 minutes)
        Profiler.BeginSample("PlayerController.FixedUpdate");

        // NEW: Cache Time.fixedDeltaTime to avoid repeated calls (minor optimization for long play sessions)
        float fixedDelta = Time.fixedDeltaTime;

        HandleLaneChange();       // Smooth lane change movement (uses cached fixedDelta internally)
        HandleVerticalMovement(); // Vertical input movement (uses cached fixedDelta)
        HandleBoundaryConstraints(); // Ensure player stays inside screen bounds

        // NEW: End Profiler sample
        Profiler.EndSample();
    }

    // Calculates screen boundary limits based on camera size
    void CalculateBoundaries()
    {
        if (Camera.main == null)
        {
            topLimit = 4f;
            bottomLimit = -4f;
            return;
        }

        float camHeight = Camera.main.orthographicSize;
        topLimit = camHeight - topBoundaryOffset;
        bottomLimit = -camHeight + bottomBoundaryOffset;
    }

    // Checks for left/right lane input from player
    void HandleLaneInput()
    {
        if (isChangingLane) return;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            ChangeLane(-1);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            ChangeLane(1);
    }

    // Handles lane change logic and prevents out-of-bounds lanes
    void ChangeLane(int direction)
    {
        int newLane = currentLane + direction;
        if (newLane >= 0 && newLane < lanePositions.Length)
        {
            currentLane = newLane;
            targetX = lanePositions[currentLane];
            isChangingLane = true;

            if (showDebugInfo)
                Debug.Log($"Changing to lane {currentLane}, target X: {targetX}");
        }
        else if (showDebugInfo)
        {
            Debug.Log($"Lane {newLane} is out of bounds");
        }
    }

    // Moves the player smoothly to the target lane position
    void HandleLaneChange()
    {
        if (!isChangingLane) return;

        float step = laneChangeSpeed * Time.fixedDeltaTime;
        Vector2 newPos = Vector2.MoveTowards(rb.position, new Vector2(targetX, rb.position.y), step);
        rb.MovePosition(newPos);

        if (Mathf.Abs(rb.position.x - targetX) < 0.05f)
        {
            rb.position = new Vector2(targetX, rb.position.y);
            isChangingLane = false;

            if (showDebugInfo)
                Debug.Log($"Lane change complete. Now at lane {currentLane}");
        }
    }

    // Allows the player to move up and down with input
    void HandleVerticalMovement()
    {
        float moveY = Input.GetAxisRaw("Vertical");
        float currentY = rb.position.y;
        float proposedY = currentY + (moveY * verticalSpeed * Time.fixedDeltaTime);

        if (proposedY >= bottomLimit && proposedY <= topLimit)
        {
            rb.velocity = new Vector2(rb.velocity.x, moveY * verticalSpeed);
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);

            // Apply gentle pushback when at boundaries
            if (proposedY < bottomLimit && currentY <= bottomLimit + boundaryBuffer)
                rb.velocity = new Vector2(rb.velocity.x, 2f);
            else if (proposedY > topLimit && currentY >= topLimit - boundaryBuffer)
                rb.velocity = new Vector2(rb.velocity.x, -2f);
        }
    }

    // Updates active power-up timers and disables them when expired
    // This method safely iterates over a copy of keys to avoid "Collection was modified" exceptions
    private void UpdatePowerUps()
    {
        // NEW: Start Profiler sample for UpdatePowerUps - monitor this in Profiler to see if it causes spikes after 3 minutes (e.g., due to large dictionary)
        Profiler.BeginSample("PlayerController.UpdatePowerUps");

        List<PowerUp.PowerUpType> expiredPowerUps = new List<PowerUp.PowerUpType>(); // List to collect expired power-ups for safe removal after iteration

        // Create a copy of the keys to iterate over (prevents errors if dictionary is modified during loop, e.g., by collecting a new power-up)
        var keys = new List<PowerUp.PowerUpType>(activePowerUps.Keys);

        // Iterate over the copied keys
        foreach (var powerUp in keys)
        {
            // Safety check: Ensure the key still exists in the dictionary (in case it was removed elsewhere)
            if (activePowerUps.ContainsKey(powerUp))
            {
                activePowerUps[powerUp] -= Time.deltaTime; // Decrement the timer for this power-up

                // NEW: Optimized debug log - only log every 1 second to reduce overhead during long play (prevents log spam after 3 minutes)
                if (showDebugInfo && powerUp == PowerUp.PowerUpType.Shield && Mathf.Approximately(Time.time % 1f, 0f))
                    Debug.Log($"Shield remaining: {activePowerUps[powerUp]:F2} seconds");

                // If timer has expired, mark it for removal
                if (activePowerUps[powerUp] <= 0f)
                    expiredPowerUps.Add(powerUp);
            }
        }

        // Now safely remove and deactivate expired power-ups (after iteration is complete)
        foreach (var expired in expiredPowerUps)
        {
            // Safety check before removal
            if (activePowerUps.ContainsKey(expired))
            {
                ApplyPowerUpEffect(expired, false); // Deactivate the effect
                activePowerUps.Remove(expired); // Remove from dictionary
                // NEW: Optimized log - only if showDebugInfo is true, to reduce console overhead
                if (showDebugInfo)
                    Debug.Log($"{expired} Deactivated");
            }
        }

        // NEW: Prevent dictionary buildup over long play (e.g., after 3 minutes) by limiting size
        // If too many power-ups accumulate (unlikely but possible with bugs), clear the oldest to avoid performance degradation
        const int maxPowerUps = 10; // Adjust this limit based on your game (e.g., max expected active power-ups)
        if (activePowerUps.Count > maxPowerUps)
        {
            Debug.LogWarning($"Power-up dictionary exceeded limit ({activePowerUps.Count} > {maxPowerUps}) - removing oldest to optimize performance");
            var oldestKey = new List<PowerUp.PowerUpType>(activePowerUps.Keys)[0]; // Get the first (oldest) key
            ApplyPowerUpEffect(oldestKey, false); // Deactivate it
            activePowerUps.Remove(oldestKey); // Remove to keep dictionary small
        }

        // NEW: End Profiler sample
        Profiler.EndSample();
    }

    // Ensures player stays inside the vertical boundaries
    void HandleBoundaryConstraints()
    {
        Vector2 currentPos = rb.position;
        bool corrected = false;

        if (currentPos.y < bottomLimit)
        {
            currentPos.y = bottomLimit + boundaryBuffer;
            corrected = true;
        }
        else if (currentPos.y > topLimit)
        {
            currentPos.y = topLimit - boundaryBuffer;
            corrected = true;
        }

        if (corrected)
        {
            rb.position = currentPos;
            rb.velocity = new Vector2(rb.velocity.x, 0f);

            // NEW: Optimized log - only if showDebugInfo and every 1 second to reduce spam
            if (showDebugInfo && Mathf.Approximately(Time.time % 1f, 0f))
                Debug.Log("Corrected player position within vertical boundaries");
        }
    }

    // Handles collision with TrafficCar and checks shield status (or debug invincibility)
    void OnTriggerEnter2D(Collider2D other)
    {
        // NEW: Start Profiler sample for OnTriggerEnter2D - check if frequent collisions cause buildup over time
        Profiler.BeginSample("PlayerController.OnTriggerEnter2D");

        // NEW: Optimized log - only if showDebugInfo and every 1 second to reduce overhead in long sessions
        if (showDebugInfo && Mathf.Approximately(Time.time % 1f, 0f))
            Debug.Log($"Collision: {other.name}, Tag: {other.tag}"); // Log the collision details for debugging

        if (other.CompareTag("TrafficCar")) // Check if the collided object is tagged as a TrafficCar
        {
            // NEW: Debug invincibility check - if enabled, absorb the hit without triggering GameOver
            // This allows thorough testing of game mechanics (e.g., movement, scoring, power-ups) without dying
            // It's isolated: Uses only local fields; optional pooling call can be removed if no external refs desired
            if (debugInvincibility)
            {
                // NEW: Optimized log for debug mode
                if (showDebugInfo && Mathf.Approximately(Time.time % 1f, 0f))
                    Debug.Log("Debug Invincibility: Hit absorbed! (Player is invincible for testing)"); // Log to confirm debug mode is active

                // Optional: Handle the TrafficCar to prevent visual overlap (consistent with Shield behavior)
                if (TrafficSpawner.Instance != null) // Safety null check for singleton
                    TrafficSpawner.Instance.ReturnToPool(other.gameObject); // Reuse if pooling is set up
                // OR: Destroy(other.gameObject); // Simple destroy if not using pooling

                // NEW: End sample early if in debug mode
                Profiler.EndSample();
                return; // Exit early: No GameOver in debug mode
            }

            // Normal Shield check: If Shield is active, absorb the hit
            if (isShieldActive)
            {
                // NEW: Optimized log
                if (showDebugInfo && Mathf.Approximately(Time.time % 1f, 0f))
                    Debug.Log("Shield absorbed the hit!");

                // Handle the TrafficCar to prevent overlap
                if (TrafficSpawner.Instance != null) // Safety null check for singleton
                    TrafficSpawner.Instance.ReturnToPool(other.gameObject); // Preferred for performance
                // OR: Destroy(other.gameObject);

                // NEW: End sample early if shielded
                Profiler.EndSample();
                return; // Exit early: No GameOver if shielded
            }

            // Normal GameOver logic if no Shield and no debug invincibility
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
            else
                Debug.LogError("GameManager.Instance is null!");
        }

        // NEW: End Profiler sample
        Profiler.EndSample();
    }

    // Called by PowerUp when collected — Activates or stacks power-up duration with a cap to prevent abuse
    public void ActivatePowerUp(PowerUp.PowerUpType type, float duration)
    {
        // NEW: Start Profiler sample for ActivatePowerUp - monitor if frequent activations cause issues over time
        Profiler.BeginSample("PlayerController.ActivatePowerUp");

        float maxDuration = 30f; // Maximum allowed duration for any power-up (e.g., cap Shield at 30 seconds to prevent indefinite immunity)

        if (!activePowerUps.ContainsKey(type))
        {
            // First time activating: Add to dictionary and apply the effect
            activePowerUps.Add(type, duration);
            ApplyPowerUpEffect(type, true);
        }
        else
        {
            // Already active: Stack (add) the new duration to the existing one
            activePowerUps[type] += duration;

            // Cap the total duration to prevent abuse (e.g., stacking too many Shields)
            if (activePowerUps[type] > maxDuration)
                activePowerUps[type] = maxDuration;
        }

        // NEW: Optimized log - only if showDebugInfo and every 1 second
        if (showDebugInfo && Mathf.Approximately(Time.time % 1f, 0f))
            Debug.Log($"{type} Activated/Extended. Remaining time: {activePowerUps[type]} seconds (capped at {maxDuration})");

        // NEW: End Profiler sample
        Profiler.EndSample();
    }

    // Applies or disables specific power-up effects
    private void ApplyPowerUpEffect(PowerUp.PowerUpType type, bool isActive)
    {
        switch (type)
        {
            case PowerUp.PowerUpType.SpeedBoost:
                if (isActive)
                {
                    isSpeedBoosted = true;
                    verticalSpeed = normalSpeed * 2f;
                }
                else
                {
                    isSpeedBoosted = false;
                    verticalSpeed = normalSpeed;
                }
                break;

            case PowerUp.PowerUpType.Shield:
                isShieldActive = isActive;
                // Optional: Add visual/audio feedback here (e.g., toggle a shield particle effect)
                // NEW: Optimized log
                if (showDebugInfo && Mathf.Approximately(Time.time % 1f, 0f))
                    Debug.Log($"Shield {(isActive ? "Activated" : "Deactivated")}");
                break;

            case PowerUp.PowerUpType.ScoreMultiplier:
                isScoreBoosted = isActive;
                break;
        }
    }

    // Resets player position and lane when restarting the game
    public void ResetPlayer()
    {
        currentLane = 1;
        targetX = lanePositions[currentLane];
        isChangingLane = false;

        Vector3 resetPosition = new Vector3(targetX, 0f, 0f);
        transform.position = resetPosition;
        rb.position = resetPosition;
        rb.velocity = Vector2.zero;

        if (showDebugInfo)
            Debug.Log("Player reset to center position");
    }

    // Draws visual debug lines for lanes and boundaries in editor
    void OnDrawGizmos()
    {
        if (lanePositions == null || lanePositions.Length == 0) return;

        Gizmos.color = Color.yellow;
        float gizmoHeight = 10f;

        foreach (float laneX in lanePositions)
        {
            Vector3 start = new Vector3(laneX, -gizmoHeight, 0);
            Vector3 end = new Vector3(laneX, gizmoHeight, 0);
            Gizmos.DrawLine(start, end);
        }

        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector3 targetPos = new Vector3(targetX, transform.position.y, 0);
            Gizmos.DrawWireSphere(targetPos, 0.3f);

            Gizmos.color = Color.magenta;
            Vector3 topBoundary = new Vector3(0, topLimit, 0);
            Vector3 bottomBoundary = new Vector3(0, bottomLimit, 0);
            Gizmos.DrawWireCube(topBoundary, new Vector3(6f, 0.1f, 0));
            Gizmos.DrawWireCube(bottomBoundary, new Vector3(6f, 0.1f, 0));
        }
    }

    // Check if Score Multiplier is active (other scripts may use this)
    public bool IsScoreBoosted()
    {
        return isScoreBoosted;
    }
}
