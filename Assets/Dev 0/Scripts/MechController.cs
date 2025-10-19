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
    public float moveSpeed = 10f;
    public float rotationSpeed = 60f;

    [Header("Look Settings")]
    public float mouseSensitivity = 3f;
    public float cockpitFollowSpeed = 5f;   // How quickly the cockpit matches camera yaw
    public float mechFollowSpeed = 2f;      // How quickly the mech matches cockpit yaw
    public float pitchLimit = 80f;

    [Header("References")]
    public Transform mechCamera;    // The cockpit camera
    public Transform cockpitTransform;
    public GameObject[] spriteRenderers;
    public LayerMask InteractableObjects;

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

        
        if (Input.GetKeyDown(KeyCode.P))
        {
            SetComponentPower("chassis", 1);
        }
        
        if (Input.GetKeyDown(KeyCode.E)) {
            Interact();   
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
        cockpitYaw = Mathf.LerpAngle(cockpitYaw, yawCockpit, Time.deltaTime * cockpitFollowSpeed);
        mechYaw = Mathf.LerpAngle(mechYaw, yawCockpit, Time.deltaTime * mechFollowSpeed);

        transform.rotation = Quaternion.Euler(0f, mechYaw, 0f);
        cockpitTransform.rotation = transform.rotation;
    }

    // === MOVEMENT ===
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDir = transform.forward * v + transform.right * h;
        rb.MovePosition(rb.position + moveDir * moveSpeed * ComponentPower["chassis"] / 1.5f * Time.deltaTime);
    }

    void Interact() 
    {
        Debug.Log("Mech Interact");
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
        Debug.Log("run");
        playerInside = !playerInside;

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
            { "blastDoor", 0f },
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
    
}
