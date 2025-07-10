using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Car Settings")]
    [SerializeField] private float verticalSpeed = 12f;

    [Header("Lane Settings")]
    [SerializeField] private float[] lanePositions = { -2f, 0f, 2f }; // Fixed X positions
    [SerializeField] private float laneChangeSpeed = 15f;

    [Header("Boundary Settings")]
    [SerializeField] private float topBoundaryOffset = 1f; // Distance from camera edge
    [SerializeField] private float bottomBoundaryOffset = 1f;
    [SerializeField] private float boundaryBuffer = 0.2f; // Extra buffer to prevent sticking

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private int currentLane = 1;
    private float targetX;
    private bool isChangingLane = false;
    private float topLimit;
    private float bottomLimit;

    private Rigidbody2D rb;

    void Start()
    {
        // Component validation
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PlayerController: Missing Rigidbody2D component!");
            return;
        }

        // Rigidbody setup
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // Initial position setup
        if (lanePositions.Length == 0)
        {
            Debug.LogError("PlayerController: No lane positions defined!");
            return;
        }

        targetX = lanePositions[currentLane];
        transform.position = new Vector3(targetX, transform.position.y, 0f);

        // Calculate boundaries once
        CalculateBoundaries();

        if (showDebugInfo)
        {
            Debug.Log($"PlayerController initialized at lane {currentLane}, position X: {targetX}");
            Debug.Log($"Boundaries - Top: {topLimit}, Bottom: {bottomLimit}");
        }
    }

    void Update()
    {
        HandleLaneInput();
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
            Debug.LogWarning("PlayerController: No main camera found for boundary calculation");
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

        bool leftInput = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
        bool rightInput = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);

        if (leftInput)
        {
            if (showDebugInfo) Debug.Log("Left input detected");
            ChangeLane(-1);
        }
        else if (rightInput)
        {
            if (showDebugInfo) Debug.Log("Right input detected");
            ChangeLane(1);
        }
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
            {
                Debug.Log($"Changing to lane {currentLane}, target X: {targetX}");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"Cannot change to lane {newLane} - out of bounds");
            }
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
            {
                Debug.Log($"Lane change complete. Now at lane {currentLane}");
            }
        }
    }

    void HandleVerticalMovement()
    {
        float moveY = Input.GetAxisRaw("Vertical"); // W/S or ↑/↓
        
        // Check if player is trying to move beyond boundaries
        float currentY = rb.position.y;
        float proposedY = currentY + (moveY * verticalSpeed * Time.fixedDeltaTime);
        
        // Allow movement only if it doesn't exceed boundaries
        if (proposedY >= bottomLimit && proposedY <= topLimit)
        {
            Vector2 newVelocity = new Vector2(rb.velocity.x, moveY * verticalSpeed);
            rb.velocity = newVelocity;
        }
        else
        {
            // Stop vertical movement when at boundary
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            
            // Add slight push back from boundary to prevent sticking
            if (proposedY < bottomLimit && currentY <= bottomLimit + boundaryBuffer)
            {
                rb.velocity = new Vector2(rb.velocity.x, 2f); // Small upward push
            }
            else if (proposedY > topLimit && currentY >= topLimit - boundaryBuffer)
            {
                rb.velocity = new Vector2(rb.velocity.x, -2f); // Small downward push
            }
        }
    }

    void HandleBoundaryConstraints()
    {
        Vector2 currentPos = rb.position;
        bool needsCorrection = false;
        
        // Check and correct vertical position if somehow outside bounds
        if (currentPos.y < bottomLimit)
        {
            currentPos.y = bottomLimit + boundaryBuffer;
            needsCorrection = true;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            
            if (showDebugInfo)
            {
                Debug.Log("Corrected position from bottom boundary");
            }
        }
        else if (currentPos.y > topLimit)
        {
            currentPos.y = topLimit - boundaryBuffer;
            needsCorrection = true;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            
            if (showDebugInfo)
            {
                Debug.Log("Corrected position from top boundary");
            }
        }
        
        if (needsCorrection)
        {
            rb.position = currentPos;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (showDebugInfo)
        {
            Debug.Log($"Collision detected with: {other.name}, Tag: {other.tag}");
        }

        if (other.CompareTag("TrafficCar"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
            else
            {
                Debug.LogError("GameManager.Instance is null!");
            }
        }
    }

    // Public method to reset player position
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
        {
            Debug.Log("Player reset to center position");
        }
    }

    // Debug visualization in Scene view
    void OnDrawGizmos()
    {
        if (lanePositions == null || lanePositions.Length == 0) return;

        Gizmos.color = Color.yellow;
        float gizmoHeight = 10f;
        
        for (int i = 0; i < lanePositions.Length; i++)
        {
            Vector3 start = new Vector3(lanePositions[i], -gizmoHeight, 0);
            Vector3 end = new Vector3(lanePositions[i], gizmoHeight, 0);
            Gizmos.DrawLine(start, end);
        }

        // Show current target
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector3 targetPos = new Vector3(targetX, transform.position.y, 0);
            Gizmos.DrawWireSphere(targetPos, 0.3f);
            
            // Show boundaries
            Gizmos.color = Color.magenta;
            Vector3 topBoundary = new Vector3(0, topLimit, 0);
            Vector3 bottomBoundary = new Vector3(0, bottomLimit, 0);
            Gizmos.DrawWireCube(topBoundary, new Vector3(6f, 0.1f, 0));
            Gizmos.DrawWireCube(bottomBoundary, new Vector3(6f, 0.1f, 0));
        }
    }
}