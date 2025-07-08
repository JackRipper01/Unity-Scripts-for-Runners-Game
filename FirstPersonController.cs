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
    Vector3 finalMove;
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
        // Gather all input here.
        GetTouchInput();

        // Camera look is not physics-based, it's fine in Update.
        // It will feel more responsive here.
        HandleLook();

        // Calculate the desired horizontal movement based on input.
        // We store it in a class variable for FixedUpdate to use.
        Vector3 horizontalMove = Vector3.zero;
        if (leftFingerID != -1 && moveInput.sqrMagnitude > moveInputDeadZonePercent)
        {
            Vector2 movementDir = moveInput.normalized * moveSpeed;
            horizontalMove = transform.forward * movementDir.y + transform.right * movementDir.x;
        }

        // Combine with the vertical velocity to create the final move direction.
        // Note: verticalVelocity is calculated in FixedUpdate.
        finalMove = horizontalMove + (Vector3.up * verticalVelocity);
    }

    // --- FixedUpdate is for ALL physics and CharacterController movement ---
    void FixedUpdate()
    {
        // 1. First, perform the physics check.
        grounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayers);
        if (grounded)
            Debug.Log("Grounded is True");
        // 2. Then, calculate gravity's effect based on the ground check result.
        if (grounded && verticalVelocity < 0)
        {
            verticalVelocity = -gravityOnGround;
        }
        else if (!grounded)
        {
            // We are in the air, so apply gravity.
            verticalVelocity -= gravity * Time.deltaTime;
        }

        // 3. Finally, apply the movement using the CharacterController.
        // We use the `finalMove` vector calculated in Update.
        // We multiply by Time.deltaTime to make the final movement frame-rate independent.
        characterController.Move(finalMove * Time.deltaTime);
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


    //DEBUG POSPUSE
    // Add this entire function to your script
    void OnDrawGizmosSelected()
    {
        // Make sure you have a groundCheck object assigned in the inspector
        if (groundCheck == null) return;

        // Set the color of the gizmo
        Gizmos.color = Color.red;

        // Draw the sphere in the Scene view
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
