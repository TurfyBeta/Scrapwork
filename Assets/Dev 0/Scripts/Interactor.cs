using UnityEngine;


interface IInteractable {
    public void Interact(GameObject interactor = null);
}

public class Interactor : MonoBehaviour
{
    public Transform InteractorSource;
    public float InteractRange;
    public LayerMask InteractableObjects;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) {
            Debug.Log("Player Interact");
            Ray ray = new Ray(InteractorSource.position, InteractorSource.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, InteractRange, InteractableObjects, QueryTriggerInteraction.Collide)) {
                if (hit.collider.gameObject.TryGetComponent(out IInteractable interactObj)) {
                    interactObj.Interact(this.gameObject);
                }
            }
        }
    }
}
