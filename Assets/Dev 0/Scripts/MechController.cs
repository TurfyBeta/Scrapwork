using UnityEngine;
using System.Collections;
using System.Collections.Generic;   

interface MechInteractable {
    public void MechInteract(GameObject MCGO);
}

[RequireComponent(typeof(Rigidbody))]
public class MechController : MonoBehaviour
{
    [Header("Movement")]
    public float[] moveSpeed;
    public float rotationSpeed = 60f;

    [Header("Look Settings")]
    public float mouseSensitivity = 3f;
    public float[] mechFollowSpeed;      // How quickly the mech matches cockpit yaw
    public float pitchLimit = 80f;

    [Header("References")]
    public Transform mechCamera;    // The cockpit camera
    public Transform cockpitTransform;
    public Camera[] RTCams;
    public GameObject[] spriteRenderers;
    public LayerMask InteractableObjects;
    public Animator DSAnimator;
    public Animator BDAnimator;
    public LayerMask climbableMask;
    public DSHandler dsHandler;

    [Header("State")]
    public bool playerInside = false;
    public bool isControlled = false;

    private Rigidbody rb;
    private float pitch = 0f;
    private float yaw = 0f;
    private float yawCockpit = 0f;
    private float cockpitYaw = 0f;
    private float mechYaw = 0f;

    private Dictionary<string, float> ComponentPower = new Dictionary<string, float>();
    private Dictionary<string, float> AccessoryPower = new Dictionary<string, float>();


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        moveSpeed = new float[]{0f, 7.5f, 9f, 12f};
        mechFollowSpeed = new float[]{0f, 1.5f, 2.5f, 4f};
        yaw = transform.eulerAngles.y;
        cockpitYaw = yaw;
        mechYaw = yaw;

        if (cockpitTransform != null)
            cockpitTransform.rotation = transform.rotation;
        
        
        // Variable Setup
        ZeroComponentPower();
        ZeroAccessoryPower();
    }

    void Update()
    {
        HandleLook();

        if (ComponentPower["chassis"] >= 1 && isControlled) {

            if (!Input.GetKey(KeyCode.Space)) yawCockpit = yaw;

            HandleMechTurn();
            HandleMovement();
        }

        if (Input.GetMouseButtonDown(0) && isControlled)
        {
            Interact();
        }
        
        if (AccessoryPower["blastDoor"] == 0f)
        {
            DSAnimator.SetBool("ScreenOpen", true);
            BDAnimator.SetBool("BDOpen", true);
            dsHandler.CamsState(false);
        } else
        {
            DSAnimator.SetBool("ScreenOpen", false);
            BDAnimator.SetBool("BDOpen", false);
            dsHandler.CamsState(true);
        }
    }

    // === MOUSE LOOK HANDLING ===
    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);
        
        mechCamera.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMechTurn() 
    {
        mechYaw = Mathf.LerpAngle(mechYaw, yawCockpit, Time.deltaTime * mechFollowSpeed[(int)ComponentPower["chassis"]]);

        transform.rotation = Quaternion.Euler(0f, mechYaw, 0f);
        cockpitTransform.rotation = transform.rotation;
    }

    // === MOVEMENT ===
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDir = transform.forward * v + transform.right * h;
        rb.MovePosition(rb.position + WallMult(moveDir) * moveDir * moveSpeed[(int)ComponentPower["chassis"]] * Time.deltaTime);
    }

private float WallMult(Vector3 desiredMove)
{
    if (desiredMove.sqrMagnitude < 0.001f)
        return 0f;

    // Define mech size (3×3×8)
    Vector3 halfExtents = new Vector3(1.5f, 4f, 1.5f);
    Vector3 moveDir = desiredMove.normalized;
    float moveDistance = desiredMove.magnitude * moveSpeed[(int)ComponentPower["chassis"]] * Time.deltaTime;

    // BoxCast to check for obstacles
    bool hitWall = Physics.BoxCast(
        transform.position + transform.forward * -1f,        // start at mech center
        halfExtents,               // half extents
        moveDir,                   // move direction
        out RaycastHit hit,
        transform.rotation,
        moveDistance + 1.2f        // small buffer
    );

    // If we detect a wall, reduce movement speed by half
    if (hitWall)
    {
        Debug.Log("Adj");
        return 0.2f;
    }
    else
    {
        return 1f;
    }
}

    void Interact() 
    {
        Ray ray = new Ray(mechCamera.position, mechCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f, InteractableObjects, QueryTriggerInteraction.Collide)) {
            if (hit.collider.gameObject.TryGetComponent(out MechInteractable interactObj)) {
                interactObj.MechInteract(this.gameObject);
            }
        }
    }

    // === ENTER / EXIT ===
    public void MechEnterExit()
    {
        // Debug.Log(this.gameObject.name);
        playerInside = !playerInside;

        foreach (Camera i in RTCams)
        {
            i.enabled = playerInside;
        }

        foreach (GameObject i in spriteRenderers)
            i.SetActive(!playerInside);
    }
    
    private void ZeroComponentPower()
    {
        ComponentPower = new Dictionary<string, float>() {
            { "chassis", 0f },
            { "melee", 0f },
            { "weapon1", 0f },
            { "weapon2", 0f },
            { "shield", 0f },
            { "flack", 0f },
            { "radar", 0f }
        };
    }

    private void ZeroAccessoryPower()
    {
        AccessoryPower = new Dictionary<string, float>() {
            { "blastDoor", 1f },
            { "cockpitLights", 0f },
            { "floodLights", 0f }
        };
    }

    public void SetComponentPower(string key, float value)
    {
        ComponentPower[key] = ComponentPower[key] == value ? 0 : value;
    }
    
    public void SetAccessoryPower(string key, float value)
    {
        AccessoryPower[key] = AccessoryPower[key] == value ? 0 : value;
    }
    
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector3 halfExtents = new Vector3(1.5f, 2f, 1.5f);
        Vector3 moveDir = transform.forward; // visualize forward direction
        float distance = 5f;

        if (Physics.BoxCast(
            transform.position,
            halfExtents,
            moveDir,
            out RaycastHit hit,
            transform.rotation,
            distance))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(hit.point, halfExtents * 2f);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, moveDir * distance);
        }
    }
}
