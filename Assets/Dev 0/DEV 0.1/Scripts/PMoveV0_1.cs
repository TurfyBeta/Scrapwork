using UnityEngine;
using System.Collections;

public enum PlayerState
{
    Grounded,
    InAir,
    Jumping,
    Sliding,
    Hanging,
    Ragdoll,
    Aiming
}

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PMoveV0_1 : MonoBehaviour
{
    [Header("Camera")]
    public Transform CamTransform;
    public float mouseSensitivity = 2f;
    private float CamY = 0f;

    [Header("Movement")]
    public PlayerState playerState = PlayerState.Grounded;
    public float MoveSpeed = 5.5f;
    public float slidePower = 14f;
    public float jumpPower = 6.5f;
    public LayerMask climbableMask;

    private float pVelY = 0f;
    private Vector3 pVelX = Vector3.zero;
    public bool IsGrounded;
    private float RunTime = 0f;

    [Header("References")]
    private CharacterController CC;
    private Rigidbody rb;
    private CapsuleCollider CapCol;

    private Coroutine LastSlide;
    private Coroutine LastRagdoll;

    private Vector3 LedgeGrabPos;
    private Vector3 ledgeNormal;

    // Input buffer
    private bool jumpPressed = false;
    private bool lControlPressed = false;
    private bool lShiftPressed = false;

    void Start()
    {
        CC = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        CapCol = GetComponent<CapsuleCollider>();

        CapCol.enabled = false;
        rb.isKinematic = true; 


        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) jumpPressed = true;
        if (Input.GetKeyDown(KeyCode.LeftControl)) lControlPressed = true;
        if (Input.GetKey(KeyCode.LeftShift)) lShiftPressed = true;
        if (Input.GetKeyDown(KeyCode.F)) StartRagdoll();

        HandleLook();
    }

    void FixedUpdate()
    {
        if (playerState != PlayerState.Ragdoll)
        {
            Quaternion targetRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 0.07f);

            HandleMovement();
            if (playerState != PlayerState.Hanging)
            {
                GroundedHandler();
                GravityHandler();
            }
        }


        jumpPressed = false;
        lControlPressed = false;
        lShiftPressed = false;
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity; 
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        CamY -= mouseY;
        CamY = Mathf.Clamp(CamY, -80f, 80f);
        CamTransform.localRotation = Quaternion.Euler(CamY, 0, 0);
    }

    void GroundedHandler()
    {
        CC.Move(Vector3.up * -0.1f * Time.deltaTime);
        IsGrounded = CC.isGrounded;
        if (IsGrounded && playerState != PlayerState.Sliding) playerState = PlayerState.Grounded;
    }

    void HandleMovement()
    {
        float speedMult = 1f;

        
        Vector3 UpVec = new Vector3(0f, pVelY, 0f);
        Vector3 MoveVec = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
        MoveVec = Vector3.ClampMagnitude(MoveVec * MoveSpeed, MoveSpeed);

        if (IsGrounded)
        {
            if (playerState != PlayerState.Sliding) {
                pVelX = Vector3.Lerp(pVelX, Vector3.zero, 0.15f);
            } else {
                pVelX = Vector3.Lerp(pVelX, Vector3.zero, 0.05f);
            }

            if (jumpPressed)
            {
                if (LastSlide != null) {
                    StopCoroutine(LastSlide);
                    LastSlide = StartCoroutine(StopSlide(0.1f));
                }
                pVelY = jumpPower;
                UpVec = new Vector3(0f, pVelY, 0f);
                pVelX += MoveVec * 0.4f;
                playerState = PlayerState.Jumping;
            }

            if (lControlPressed && playerState != PlayerState.Sliding) 
            {
                StartSlide();
            }

            if (lShiftPressed && Input.GetAxis("Vertical") > 0f && playerState == PlayerState.Grounded) {
                RunTime += Time.deltaTime;
                RunTime = Mathf.Clamp(RunTime, 1f, 4f);
                MoveVec += transform.forward * RunTime;
            } else {
                RunTime = 0f;
            }
        }
        else
        {
            if (MoveVec.magnitude > 1f && playerState != PlayerState.Hanging && playerState != PlayerState.Sliding) playerState = PlayerState.InAir;

            speedMult = 0.4f;

            if (playerState != PlayerState.Hanging)
            {
                if (jumpPressed) StartLedgeGrab();
            }
            else
            {
                if (jumpPressed) DismountLedgeGrab();
                if (!jumpPressed) HandleLedgeGrab();
            }
        }

        if (playerState == PlayerState.Hanging || playerState == PlayerState.Sliding)
        {
            speedMult = 0f;
        }

        MoveVec *= speedMult;

        MoveVec += pVelX;
        MoveVec = Vector3.ClampMagnitude(MoveVec, 12f);

        MoveVec += UpVec;
        if (MoveVec != Vector3.zero) CC.Move(MoveVec * Time.deltaTime);

    }

    void GravityHandler()
    {
        if (IsGrounded)
            pVelY = 0f;
        else
            pVelY = Mathf.Clamp(pVelY - 0.35f, -18f, 18f);
    }

    void StartLedgeGrab()
    {
        if (playerState == PlayerState.Hanging) return;

        RaycastHit hit;
        Vector3 origin = CamTransform.position;
        Vector3 direction = CamTransform.forward;

        if (Physics.Raycast(origin, direction, out hit, 0.75f, climbableMask))
        {
            Vector3 topCheckStart = hit.point + Vector3.up * 1.2f;

            if (Physics.Raycast(topCheckStart, Vector3.down, out RaycastHit downHit, 1.7f, climbableMask))
            {
                float obstacleHeight = downHit.point.y - transform.position.y;

                if (obstacleHeight > 0.4f && obstacleHeight < 3f)
                {
                    ledgeNormal = hit.normal;
                    LedgeGrabPos = downHit.point + Vector3.up * -0.65f + transform.forward * -0.65f;
                    playerState = PlayerState.Hanging;
                    pVelX = Vector3.zero;
                    pVelY = 0f;
                }
            }
        }
    }

    void DismountLedgeGrab()
    {
        playerState = PlayerState.Jumping;

        Vector3 jumpDir = CamTransform.forward.normalized;
        if (jumpDir.y < 0.2f) jumpDir.y = 0.2f;

        pVelY = jumpDir.y * jumpPower * 1.2f;
        jumpDir.Normalize();
        pVelX = new Vector3(jumpDir.x, 0f, jumpDir.z) * jumpPower;
    }

    void HandleLedgeGrab()
    {
        float moveInput = Input.GetAxis("Horizontal");

        // Compute the direction along the wall
        Vector3 wallRight = Vector3.Cross(Vector3.up, ledgeNormal).normalized;

        // Flip input direction depending on facing
        float facingDot = Vector3.Dot(transform.forward, ledgeNormal);
        if (facingDot < 0f) wallRight *= -1f;


        // Check if still on wall
        bool stillOnWall = Physics.SphereCast(
            transform.position, 0.3f, -ledgeNormal, out RaycastHit wallHit, 0.6f, climbableMask);

        if (!stillOnWall)
        {
            // Lost wall contact → fall
            playerState = PlayerState.Jumping;
            return;
        }

        // Check if you can move sideways (shimmy)
        if (Mathf.Abs(moveInput) > 0.05f)
        {
            Vector3 shimmyDir = wallRight * Mathf.Sign(moveInput);
            Vector3 angledDir = (shimmyDir - ledgeNormal * 0.2f).normalized; // angled slightly into wall

            bool canShimmy = !Physics.SphereCast(
                transform.position, 0.25f, angledDir, out _, 0.5f, climbableMask);

            if (canShimmy)
            {
                LedgeGrabPos += wallRight * moveInput * 0.04f;
            }
        }

        // Smoothly move toward target ledge grab position
        if (Vector3.Distance(LedgeGrabPos, transform.position) > 0.05f)
        {
            transform.position = Vector3.Lerp(
                transform.position, LedgeGrabPos, 10f * Time.deltaTime);
        }
    }

    void StartSlide() 
    {
        // pVelX *= 0.5f; 
        pVelX += transform.forward * slidePower;
        playerState = PlayerState.Sliding;
        
        CC.height = 0.5f;
        CC.radius = 0.3f;
        CamTransform.localPosition = new Vector3(0f, 0f, 0f);
        transform.position += Vector3.up * -0.4f;
        LastSlide = StartCoroutine(StopSlide(1.7f));
    }

    IEnumerator StopSlide(float delay) {
        yield return new WaitForSeconds(delay);

        transform.position += Vector3.up * 0.4f;
        playerState = PlayerState.Grounded;
        CC.height = 2f;
        CC.radius = 0.5f;
        CamTransform.localPosition = new Vector3(0f, 0.5f, 0f);
        LastSlide = null;
    }

    void StartRagdoll()
    {
        CC.enabled = false;
        CapCol.enabled = true;
        rb.isKinematic = false; 

        rb.AddForce(Vector3.forward * pVelX[2] + Vector3.right * pVelX[0] + Vector3.up * pVelY, ForceMode.Impulse);

        playerState = PlayerState.Ragdoll;
        LastRagdoll = StartCoroutine(StopRagdoll(3f));
    }

    IEnumerator StopRagdoll(float delay) {
        yield return new WaitForSeconds(delay);

        pVelX[0] = rb.linearVelocity.x;
        pVelX[2] = rb.linearVelocity.z;
        pVelY = rb.linearVelocity.y;

        CC.enabled = true;
        CapCol.enabled = false;
        rb.isKinematic = true; 

        playerState = PlayerState.Grounded;
    }
}
