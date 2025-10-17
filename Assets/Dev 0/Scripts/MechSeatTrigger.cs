using UnityEngine;

public class MechSeatTrigger : MonoBehaviour
{
    public MechController mechController;
    public Transform playerSeatPosition;
    public Camera mechCamera;
    public Camera playerCamera;
    public GameObject playerObject;

    private bool inRange = false;
    private bool interactDelay = false;

    void Start() 
    {
        mechCamera.enabled = false;
        mechController.isControlled = false;
    }

    void Update()
    {
        if (!interactDelay && mechController.isControlled && Input.GetKeyDown(KeyCode.E))
        {
            ExitMech();
            interactDelay = true;
        }

        if (!interactDelay && inRange && Input.GetKeyDown(KeyCode.E))
        {
            EnterMech();
            interactDelay = true;
        }

        interactDelay = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
        }
    }

    void EnterMech()
    {
        Debug.Log("Mech Enter Seat");
        playerObject.SetActive(false); // Hide player model + disable controller
        playerCamera.enabled = false;

        mechCamera.enabled = true;
        mechController.isControlled = true;
    }

    void ExitMech()
    {
        Debug.Log("Mech Exit Seat");
        playerObject.SetActive(true);
        playerObject.transform.position = transform.position;
        playerObject.transform.rotation = transform.rotation;
        playerCamera.enabled = true;

        mechCamera.enabled = false;
        mechController.isControlled = false;
    }
}
