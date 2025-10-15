using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 2f;
    public float maxLookX = 80f;
    public float minLookX = -80f;

    [Header("References")]
    public Transform cameraTransform;

    private Rigidbody rb;
    private float rotX;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevent physics from rotating the player
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleJump();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    // Mouse look (camera rotation)
    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player horizontally
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera vertically
        rotX -= mouseY;
        rotX = Mathf.Clamp(rotX, minLookX, maxLookX);
        cameraTransform.localRotation = Quaternion.Euler(rotX, 0, 0);
    }

    // WASD movement (relative to camera/player)
    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDir = (transform.forward * moveZ + transform.right * moveX).normalized;
        Vector3 moveVelocity = moveDir * moveSpeed;

        // Keep existing Y velocity (for gravity / jump)
        Vector3 newVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        rb.linearVelocity = newVelocity;
    }

    // Optional jump
    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // Ground check
    void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
