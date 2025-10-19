using UnityEngine;
    
[RequireComponent(typeof(CharacterController))]
public class CamMechMove : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform cameraTransform; // Your cockpit camera
    [SerializeField] Transform mechBody;        // The mech's root or body

    [Header("Mouse Look Settings")]
    [SerializeField] float mouseSensitivityX = 120f;
    [SerializeField] float mouseSensitivityY = 80f;
    [SerializeField] float minPitch = -60f;
    [SerializeField] float maxPitch = 60f;
    [SerializeField] float lookSmoothSpeed = 10f;

    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float acceleration = 4f;     // smooth ramp up/down
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float mechWeightFactor = 0.5f; // slows down acceleration (for heavy feel)

    private CharacterController controller;
    private float yaw = 0f;
    private float pitch = 0f;
    private Vector3 velocity;
    private Vector3 currentMoveDir;


    void Start()
    {
        // Setup
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = mechBody.eulerAngles.y;
        pitch = cameraTransform.localEulerAngles.x;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();


    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Rotate mech horizontally
        Quaternion targetBodyRot = Quaternion.Euler(0f, yaw, 0f);
        mechBody.rotation = Quaternion.Slerp(mechBody.rotation, targetBodyRot, lookSmoothSpeed * Time.deltaTime);

        // Rotate camera vertically
        Quaternion targetCamRot = Quaternion.Euler(pitch, 0f, 0f);
        cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, targetCamRot, lookSmoothSpeed * Time.deltaTime);
    }

    void HandleMovement()
    {
        // WASD input
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S

        // Move relative to mech body
        Vector3 move = (mechBody.forward * v + mechBody.right * h).normalized;

        // Smooth acceleration to feel heavy
        currentMoveDir = Vector3.Lerp(currentMoveDir, move, Time.deltaTime * acceleration * mechWeightFactor);

        // Apply gravity (if needed)
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;

        // Move mech
        Vector3 finalMove = currentMoveDir * moveSpeed + new Vector3(0f, velocity.y, 0f);
        controller.Move(finalMove * Time.deltaTime);
    }


}
