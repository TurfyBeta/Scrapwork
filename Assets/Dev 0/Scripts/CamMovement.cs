using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 5f;
    public float vaultForce = 10f;
    public float vaultUpForce = 6f;
    public float vaultRange = 2.5f;
    public float vaultHeightCheck = 2f;
    public LayerMask climbableMask;
    public float gravityScale = 1.2f;
    public bool useCustomGravity = true;

    [Header("Slide Settings")]
    public float slideForce = 10f;
    public float slideDuration = 0.8f;
    public float slideCooldown = 1.5f;
    public float slideHeightScale = 0.5f;

    [Header("Look Settings")]
    public float mouseSensitivity = 2f;
    public float maxLookX = 80f;
    public float minLookX = -80f;

    [Header("References")]
    public Transform cameraTransform;
    public Transform PlayerTransform;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private float rotX;
    private bool isGrounded;
    private bool isVaulting;
    private bool isSliding;
    private float slideTimer;
    private float lastSlideTime = -999f;
    private float originalHeight;
    private float originalCenterY;
    private Vector3 originalCameraPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        rb.linearDamping = 2f;
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            PhysicsMaterial frictionless = new PhysicsMaterial("Frictionless");
            frictionless.dynamicFriction = 0f;
            frictionless.staticFriction = 0f;
            frictionless.frictionCombine = PhysicsMaterialCombine.Minimum;
            frictionless.bounceCombine = PhysicsMaterialCombine.Minimum;
            frictionless.bounciness = 0f;
            col.material = frictionless;
        }

        capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            originalHeight = capsuleCollider.height;
            originalCenterY = capsuleCollider.center.y;
        }

        if (cameraTransform != null)
        {
            originalCameraPos = cameraTransform.localPosition;
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleSlideInput();

        if (!isVaulting && !isSliding && Time.time >= lastSlideTime + slideCooldown)
        {
            HandleJump();
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f)
            {
                EndSlide();
            }
        }
    }

    void FixedUpdate()
    {
        if (useCustomGravity)
            rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);

        if (!isVaulting && !isSliding)
        {
            HandleMovement();
        }
        else if (isSliding)
        {
            HandleSlideMovement();
        }
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        PlayerTransform.Rotate(Vector3.up * mouseX);

        rotX -= mouseY;
        rotX = Mathf.Clamp(rotX, minLookX, maxLookX);
        cameraTransform.localRotation = Quaternion.Euler(rotX, 0, 0);
    }

    void HandleSlideInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && Time.time >= lastSlideTime + slideCooldown 
            && !isSliding && !isVaulting && isGrounded)
        {
            StartSlide();
        }
    }

    void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        lastSlideTime = Time.time;

        if (capsuleCollider != null)
        {
            capsuleCollider.height = originalHeight * slideHeightScale;
            capsuleCollider.center = new Vector3(
                capsuleCollider.center.x,
                originalCenterY * slideHeightScale,
                capsuleCollider.center.z
            );
        }

        if (cameraTransform != null)
        {
            Vector3 camPos = cameraTransform.localPosition;
            camPos.y = originalCameraPos.y * slideHeightScale;
            cameraTransform.localPosition = camPos;
        }

        Vector3 slideDir = PlayerTransform.forward;
        slideDir.y = 0f;
        slideDir.Normalize();

        rb.linearDamping = 20f;
        rb.AddForce(slideDir * slideForce, ForceMode.VelocityChange);
    }

    void HandleSlideMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        Vector3 steerForce = PlayerTransform.right * moveX * 3f;
        steerForce.y = 0f;
        rb.AddForce(steerForce, ForceMode.Acceleration);
    }

    void EndSlide()
    {
        isSliding = false;
        rb.linearDamping = 2f;

        if (capsuleCollider != null)
        {
            capsuleCollider.height = originalHeight;
            capsuleCollider.center = new Vector3(
                capsuleCollider.center.x,
                originalCenterY,
                capsuleCollider.center.z
            );
        }

        if (cameraTransform != null)
        {
            cameraTransform.localPosition = originalCameraPos;
        }
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 flatForward = PlayerTransform.forward;
        flatForward.y = 0f;
        flatForward.Normalize();

        Vector3 flatRight = PlayerTransform.right;
        flatRight.y = 0f;
        flatRight.Normalize();

        Vector3 inputDir = (flatForward * moveZ + flatRight * moveX).normalized;

        float groundAcceleration = 18f;
        float airAcceleration = 10f;
        float groundDrag = 8f;
        float glideDrag = 0.1f;
        float airControl = 0.7f;
        float maxSpeed = moveSpeed;

        Vector3 vel = rb.linearVelocity;
        Vector3 horizVel = new Vector3(vel.x, 0f, vel.z);

        Vector3 adjustedInputDir = inputDir;
        if (inputDir.sqrMagnitude > 0.01f)
        {
            adjustedInputDir = GetWallAdjustedMovement(inputDir);
        }

        if (adjustedInputDir.sqrMagnitude > 0.01f)
        {
            Vector3 targetVel = adjustedInputDir * maxSpeed;
            if (!isGrounded)
                targetVel *= airControl;

            Vector3 velDiff = targetVel - horizVel;
            float accel = isGrounded ? groundAcceleration : airAcceleration;

            Vector3 accelForce = velDiff.normalized * accel;
            rb.AddForce(new Vector3(accelForce.x, 0, accelForce.z), ForceMode.Acceleration);
        }
        else
        {
            if (isGrounded)
            {
                Vector3 slowedVel = horizVel * (1f - Time.fixedDeltaTime * groundDrag);
                rb.linearVelocity = new Vector3(slowedVel.x, vel.y, slowedVel.z);
            }
            else
            {
                Vector3 slowedVel = horizVel * (1f - Time.fixedDeltaTime * glideDrag);
                rb.linearVelocity = new Vector3(slowedVel.x, vel.y, slowedVel.z);
            }
        }
    }

    Vector3 GetWallAdjustedMovement(Vector3 inputDir)
    {
        Vector3 origin = transform.position;
        float checkDistance = 0.46f;
        float capsuleRadius = 0.45f;

        RaycastHit hit;
        
        if (Physics.SphereCast(origin, capsuleRadius, inputDir, out hit, checkDistance, climbableMask))
        {
            float angle = Vector3.Angle(-hit.normal, inputDir);
            
            if (angle < 60f)
            {
                return Vector3.zero;
            }
            
            Vector3 slideDir = Vector3.ProjectOnPlane(inputDir, hit.normal);
            slideDir.y = 0f;
            return slideDir.normalized;
        }

        return inputDir;
    }

    void HandleJump()
    {
        if (Input.GetButton("Jump") && isGrounded)
        {
            float mult = 1f;
            // if (isSliding)
            // {
            //     EndSlide();
            //     mult = -1f;
            //     PlayerTransform.position += Vector3.up * 0.55f;
            // }

            rb.AddForce(Vector3.up * jumpForce * mult, ForceMode.Impulse);
            isGrounded = false;
        }
        else if (Input.GetButtonDown("Jump"))
        {
            TryVault();
        }
    }

void TryVault()
{
    if (isVaulting) return;

    RaycastHit hit;

    // Start from camera height so we vault toward where the player is looking
    Vector3 origin = cameraTransform.position;
    Vector3 direction = cameraTransform.forward;
    // direction.y = 0f; // Keep horizontal focus for vault detection
    direction.Normalize();

    // Short forward ray to detect walls in front of the camera
    if (Physics.Raycast(origin, direction, out hit, vaultRange, climbableMask))
    {
        // Check for a flat spot above the hit point
        Vector3 topCheckStart = hit.point + Vector3.up * vaultHeightCheck;

        if (Physics.Raycast(topCheckStart, Vector3.down, out RaycastHit downHit, vaultHeightCheck + 0.5f, climbableMask))
        {
            float obstacleHeight = downHit.point.y - transform.position.y;

            // Only vault if obstacle is between knee height and about 2.5x player height
            if (obstacleHeight > 0.4f && obstacleHeight < 3f)
            {
                StartCoroutine(VaultOverObstacle(downHit.point, hit.distance, obstacleHeight, direction));
            }
        }
    }
}

System.Collections.IEnumerator VaultOverObstacle(Vector3 targetPoint, float distanceToWall, float obstacleHeight, Vector3 direction)
{
    isVaulting = true;

    // Freeze current velocity and adjust capsule
    rb.linearVelocity = Vector3.zero;
    rb.useGravity = false;

    if (capsuleCollider != null)
    {
        capsuleCollider.height = originalHeight * 0.6f;
        capsuleCollider.center = new Vector3(
            capsuleCollider.center.x,
            originalCenterY * 0.6f,
            capsuleCollider.center.z
        );
    }

    // Lower camera slightly during vault
    if (cameraTransform != null)
    {
        Vector3 camPos = cameraTransform.localPosition;
        camPos.y = originalCameraPos.y * 0.75f;
        cameraTransform.localPosition = camPos;
    }

    // Calculate vault direction and force scaling
    Vector3 vaultDir = direction;
    vaultDir.y = 0f;
    vaultDir.Normalize();

    float heightMultiplier = Mathf.Clamp(obstacleHeight / 1.5f, 0.8f, 2.5f);
    float upwardBoost = vaultUpForce * heightMultiplier * 1.2f; // Slightly stronger vault
    float forwardBoost = vaultForce * (0.8f + heightMultiplier * 0.3f);

    // Apply vault forces (camera-based)
    rb.AddForce(Vector3.up * upwardBoost, ForceMode.VelocityChange);
    rb.AddForce(vaultDir * forwardBoost, ForceMode.VelocityChange);

    // Small delay to let the vault clear
    yield return new WaitForSeconds(0.45f);

    // Restore player physics
    rb.useGravity = true;

    if (capsuleCollider != null)
    {
        capsuleCollider.height = originalHeight;
        capsuleCollider.center = new Vector3(
            capsuleCollider.center.x,
            originalCenterY,
            capsuleCollider.center.z
        );
    }

    if (cameraTransform != null)
    {
        cameraTransform.localPosition = originalCameraPos;
    }

    isVaulting = false;
}


    void OnCollisionStay(Collision collision)
    {
        isGrounded = false;
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Angle(contact.normal, Vector3.up) < 45f)
            {
                isGrounded = true;
                rb.linearDamping = 2f;
                break;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
        rb.linearDamping = 0f;
    }
}