using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Car Settings")]
    [SerializeField] private float verticalSpeed = 12f;

    [Header("Lane Settings")]
    [SerializeField] private float[] lanePositions = { -2f, 0f, 2f };
    [SerializeField] private float laneChangeSpeed = 15f;

    [Header("Boundary Settings")]
    [SerializeField] private float topBoundaryOffset = 1f;
    [SerializeField] private float bottomBoundaryOffset = 1f;
    [SerializeField] private float boundaryBuffer = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private int currentLane = 1;
    private float targetX;
    private bool isChangingLane = false;
    private float topLimit;
    private float bottomLimit;

    private Rigidbody2D rb;
    private float normalSpeed;

    // PowerUp State
    private bool isSpeedBoosted = false;
    private bool isShieldActive = false;
    private bool isScoreBoosted = false;
    private float powerUpTimer = 0f;
    private float currentPowerUpDuration = 0f;
    private PowerUp.PowerUpType currentPowerUp;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PlayerController: Missing Rigidbody2D!");
            return;
        }

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (lanePositions.Length == 0)
        {
            Debug.LogError("No lane positions defined!");
            return;
        }

        normalSpeed = verticalSpeed;
        targetX = lanePositions[currentLane];
        transform.position = new Vector3(targetX, transform.position.y, 0f);
        CalculateBoundaries();

        if (showDebugInfo)
        {
            Debug.Log($"Initialized at lane {currentLane}, X: {targetX}");
            Debug.Log($"Boundaries - Top: {topLimit}, Bottom: {bottomLimit}");
        }
    }

    void Update()
    {
        HandleLaneInput();

        // PowerUp timer
        if (powerUpTimer > 0)
        {
            powerUpTimer -= Time.deltaTime;
            if (powerUpTimer <= 0) DeactivatePowerUp();
        }
    }

    void FixedUpdate()
    {
        HandleLaneChange();
        HandleVerticalMovement();
        HandleBoundaryConstraints();
    }

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

    void HandleLaneInput()
    {
        if (isChangingLane) return;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            ChangeLane(-1);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            ChangeLane(1);
    }

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

    void HandleLaneChange()
    {
        if (!isChangingLane) return;

        float newX = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.fixedDeltaTime);
        rb.MovePosition(new Vector2(newX, rb.position.y));

        if (Mathf.Abs(rb.position.x - targetX) < 0.05f)
        {
            rb.MovePosition(new Vector2(targetX, rb.position.y));
            isChangingLane = false;

            if (showDebugInfo)
                Debug.Log($"Lane change complete. Now at lane {currentLane}");
        }
    }

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

            if (proposedY < bottomLimit && currentY <= bottomLimit + boundaryBuffer)
                rb.velocity = new Vector2(rb.velocity.x, 2f);
            else if (proposedY > topLimit && currentY >= topLimit - boundaryBuffer)
                rb.velocity = new Vector2(rb.velocity.x, -2f);
        }
    }

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

            if (showDebugInfo)
                Debug.Log("Corrected player position within vertical boundaries");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (showDebugInfo)
            Debug.Log($"Collision: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("TrafficCar"))
        {
            if (isShieldActive)
            {
                Debug.Log("Shield absorbed the hit!");
                return;
            }

            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
            else
                Debug.LogError("GameManager.Instance is null!");
        }
    }

    public void ActivatePowerUp(PowerUp.PowerUpType type, float duration)
    {
        // Deactivate any existing powerup first
        if (powerUpTimer > 0)
        {
            Debug.Log($"Deactivating previous powerup: {currentPowerUp}");
            DeactivatePowerUp();
        }

        currentPowerUp = type;
        currentPowerUpDuration = duration;
        powerUpTimer = duration;

        switch (type)
        {
            case PowerUp.PowerUpType.SpeedBoost:
                isSpeedBoosted = true;
                float oldSpeed = verticalSpeed;
                verticalSpeed = normalSpeed * 2f;
                Debug.Log($"Speed Boost Activated! Speed: {oldSpeed} -> {verticalSpeed} for {duration}s");
                break;

            case PowerUp.PowerUpType.Shield:
                isShieldActive = true;
                Debug.Log($"Shield Activated for {duration}s!");
                break;

            case PowerUp.PowerUpType.ScoreMultiplier:
                isScoreBoosted = true;
                Debug.Log($"Score Multiplier Activated for {duration}s!");
                break;
        }
    }

    private void DeactivatePowerUp()
    {
        switch (currentPowerUp)
        {
            case PowerUp.PowerUpType.SpeedBoost:
                isSpeedBoosted = false;
                verticalSpeed = normalSpeed;
                Debug.Log("Speed Boost Ended");
                break;

            case PowerUp.PowerUpType.Shield:
                isShieldActive = false;
                Debug.Log("Shield Deactivated");
                break;

            case PowerUp.PowerUpType.ScoreMultiplier:
                isScoreBoosted = false;
                Debug.Log("Score Multiplier Deactivated");
                break;
        }
    }

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

/*
    /// <summary>
    /// Draws debug information on the screen if enabled.
    /// </summary>
    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"Current Speed: {verticalSpeed}");
        GUILayout.Label($"Normal Speed: {normalSpeed}");
        GUILayout.Label($"Speed Boosted: {isSpeedBoosted}");
        GUILayout.Label($"Shield Active: {isShieldActive}");
        GUILayout.Label($"Score Boosted: {isScoreBoosted}");
        GUILayout.Label($"PowerUp Timer: {powerUpTimer:F1}s");

        if (powerUpTimer > 0)
        {
            GUILayout.Label($"Active PowerUp: {currentPowerUp}");
        }
        GUILayout.EndArea();
    }
*/
    public bool IsScoreBoosted()
    {
        return isScoreBoosted;
    }
}
