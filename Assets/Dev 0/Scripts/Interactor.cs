using UnityEngine;


interface IInteractable {
    public void Interact(GameObject interactor = null);
}

public class Interactor : MonoBehaviour
{
    public Transform InteractorSource;
    public float InteractRange;
    public LayerMask InteractableObjects;

    private bool InputBuffer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) 
        {
            InputBuffer = true;
        }
    }

    void FixedUpdate()
    {
        if (InputBuffer) {
            Debug.Log("Player Interact");
            Ray ray = new Ray(InteractorSource.position, InteractorSource.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, InteractRange, InteractableObjects, QueryTriggerInteraction.Collide)) {
                if (hit.collider.gameObject.TryGetComponent(out IInteractable interactObj)) {
                    interactObj.Interact(gameObject);
                }
            }
        }
        InputBuffer = false;
    }
}
