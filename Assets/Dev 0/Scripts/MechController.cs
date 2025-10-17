using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class MechController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float rotationSpeed = 60f;

    [Header("Look Settings")]
    public float mouseSensitivity = 3f;
    public float cockpitFollowSpeed = 5f;   // How quickly the cockpit matches camera yaw
    public float mechFollowSpeed = 2f;      // How quickly the mech matches cockpit yaw
    public float returnLookSpeed = 4f;      // How fast camera re-centers after free look
    public float pitchLimit = 80f;

    [Header("References")]
    public Transform mechCamera;    // The cockpit camera
    public Transform cockpitTransform;
    public GameObject[] spriteRenderers;

    [Header("State")]
    public bool playerInside = false;
    public bool isControlled = false;

    private Rigidbody rb;
    private float pitch = 0f;
    private float yaw = 0f;
    private float cockpitYaw = 0f;
    private float mechYaw = 0f;
    private bool isFreeLooking = false;
    private bool isReturning = false;
    private Quaternion freeLookReturnRot;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        yaw = transform.eulerAngles.y;
        cockpitYaw = yaw;
        mechYaw = yaw;

        if (cockpitTransform != null)
            cockpitTransform.rotation = transform.rotation;
    }

    void Update()
    {
        if (!isControlled || isReturning)
            return;

        HandleLook();
        HandleMovement();
    }

    // === MOUSE LOOK HANDLING ===
    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        if (Input.GetKey(KeyCode.Space))
        {
            // === FREE LOOK MODE ===
            if (!isFreeLooking)
            {
                isFreeLooking = true;
                freeLookReturnRot = mechCamera.localRotation;
            }

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);

            // Camera moves independently
            mechCamera.localRotation = Quaternion.Euler(pitch, yaw - cockpitYaw, 0f);
        }
        else
        {
            // === NORMAL CONTROL MODE ===
            if (isFreeLooking)
            {
                // Released free look
                isFreeLooking = false;
                StartCoroutine(ReturnCameraForwardAndSnap());
                return;
            }

            // Normal movement: camera drives cockpit yaw, cockpit drives mech
            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);

            // Cockpit slowly follows player’s current look yaw
            cockpitYaw = Mathf.LerpAngle(cockpitYaw, yaw, Time.deltaTime * cockpitFollowSpeed);

            // Mech slowly follows the cockpit’s yaw
            mechYaw = Mathf.LerpAngle(mechYaw, cockpitYaw, Time.deltaTime * mechFollowSpeed);

            // Apply rotations
            // cockpitTransform.rotation = Quaternion.Euler(0f, cockpitYaw, 0f);
            transform.rotation = Quaternion.Euler(0f, mechYaw, 0f);

            cockpitTransform.rotation = transform.rotation;

            // Pitch affects camera only
            mechCamera.localRotation = Quaternion.Euler(pitch, cockpitYaw - mechYaw, 0f);
        }
    }

    // === CAMERA RETURN AFTER FREELOOK ===
    IEnumerator ReturnCameraForwardAndSnap()
    {
        isReturning = true;

        Quaternion startRot = mechCamera.localRotation;
        Quaternion targetRot = Quaternion.Euler(pitch, 0f, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * returnLookSpeed;
            mechCamera.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        mechCamera.localRotation = targetRot;

        // After the camera finishes returning, snap mech to match player view
        yaw = cockpitYaw = mechCamera.parent.eulerAngles.y;
        mechYaw = yaw;

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cockpitTransform.rotation = transform.rotation;

        isReturning = false;
    }

    // === MOVEMENT ===
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDir = transform.forward * v + transform.right * h;
        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.deltaTime);
    }

    // === ENTER / EXIT ===
    public void MechEnterExit()
    {
        playerInside = !playerInside;

        foreach (GameObject i in spriteRenderers)
            i.SetActive(!playerInside);
    }
}
