using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    //References
    public Transform cameraTransform;
    public CharacterController characterController;

    //Player Settings
    public float cameraSensitivity;
    public float moveSpeed;
    public float moveInputDeadZonePercent = 0.1f;


    //Player movement
    Vector2 moveTouchStartPos;
    Vector2 moveInput;
    private float verticalVelocity;

    //Touch detection
    int leftFingerID, rightFingerID;
    float halfScreenWidth;

    //Camera control
    Vector2 lookInput;
    float cameraPitch;

    //Gravity
    [Header("Gravity & Jumping")]
    public float gravityOnGround;
    public float gravity;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayers;
    public float groundCheckRadius;
    private bool grounded;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leftFingerID = -1;
        rightFingerID = -1;
        halfScreenWidth = Screen.width / 2;

        // Calculate the dead zone radius in pixels
        float deadZoneInPixels = Screen.height * moveInputDeadZonePercent;
        // Then square it for the efficient .sqrMagnitude check
        moveInputDeadZonePercent = deadZoneInPixels * deadZoneInPixels;
    }

    // Update is called once per frame
    void Update()
    {
        GetTouchInput();
        HandleLook(); 
        HandleMovementAndGravity();
    }
    void FixedUpdate()
    {
        grounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayers);
    }
    private void GetTouchInput()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            switch (t.phase)
            {
                case TouchPhase.Began:

                    if (t.position.x < halfScreenWidth && leftFingerID == -1)
                    {
                        leftFingerID = t.fingerId;
                        Debug.Log("Tracking Left Finger");
                        moveTouchStartPos = t.position;
                        Debug.Log("touchPosition initialized");
                    }
                    else if (t.position.x > halfScreenWidth && rightFingerID == -1)
                    {
                        rightFingerID = t.fingerId;
                        Debug.Log("Tracking Right Finger");
                    }
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (t.fingerId == leftFingerID)
                    {
                        leftFingerID = -1;
                        moveInput = Vector2.zero;
                        Debug.Log("Stop tracking Left Finger");
                    }
                    else if (t.fingerId == rightFingerID)
                    {
                        rightFingerID = -1;
                        Debug.Log("Stop tracking Right Finger");
                    }
                    break;
                case TouchPhase.Moved:
                    if (t.fingerId == rightFingerID)
                    {
                        lookInput = t.deltaPosition * cameraSensitivity * Time.deltaTime;
                    }
                    if (t.fingerId == leftFingerID)
                    {
                        moveInput = t.position - moveTouchStartPos;
                    }
                    break;
                case TouchPhase.Stationary:
                    if (t.fingerId == rightFingerID)
                    {
                        lookInput = Vector2.zero;
                    }
                    break;
            }
        }
    }

    void HandleLook()
    {
        // Only look around if right finger is being tracked
        if (rightFingerID == -1) return;

        cameraPitch = Mathf.Clamp(cameraPitch - lookInput.y, -90f, 90f);
        transform.rotation *= Quaternion.Euler(0f, lookInput.x, 0f);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    void HandleMovementAndGravity()
    {
        // --- Step 1: Handle Horizontal Movement (from player input) ---
        Vector3 horizontalMove = Vector3.zero; // Default to no movement

        // Only calculate horizontal movement if the left finger is tracked
        if (leftFingerID != -1 && moveInput.sqrMagnitude > moveInputDeadZonePercent)
        {
            // Get the direction 
            Vector2 movementDir = moveInput.normalized * moveSpeed;
            horizontalMove = transform.forward * movementDir.y + transform.right * movementDir.x;
        }

        // --- Step 2: Handle Vertical Movement (Gravity) ---

        // If we are on the ground and falling, reset velocity to a small downward force
        // This helps the character stick to slopes.
        if (grounded && verticalVelocity < 0)
        {
            verticalVelocity = -gravityOnGround;
        }

        // Apply gravity over time
        verticalVelocity -= gravity * Time.deltaTime;

        // Create the vertical movement vector
        Vector3 verticalMove = Vector3.up * verticalVelocity;

        // --- Step 3: Combine and Move ---
        // Combine horizontal and vertical motion, and THEN make it frame-rate independent
        Vector3 finalMove = (horizontalMove + verticalMove) * Time.deltaTime;

        // Apply the final, combined movement in a SINGLE call
        characterController.Move(finalMove);
    }
}
