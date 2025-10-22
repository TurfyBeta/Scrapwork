using UnityEngine;
using UnityEngine.AI;
// using UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class StewwieAI : MonoBehaviour, IInteractable
{
    private Transform playerTransform;
    private NavMeshAgent aiAgent;
    private Rigidbody rb;

    [Header("Settings")]
    public float lookSmoothness = 1f;
    public float stopDistance = 3f;

    void Start()
    {
        // Find player root (adjust as needed)
        playerTransform = Camera.main.transform.root;

        rb = GetComponent<Rigidbody>();
        aiAgent = GetComponent<NavMeshAgent>();
        
    }

    void FixedUpdate()
    {
        float dist = Vector3.Distance(playerTransform.position, transform.position);

        if (dist > stopDistance)
        {
            aiAgent.destination = playerTransform.position;
        }
        else
        {
            aiAgent.destination = transform.position;

            // Get direction to player (flatten Y to avoid tilt)
            Vector3 dir = playerTransform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    Time.deltaTime * lookSmoothness
                );
            }
        }
    }

    public void Interact(GameObject interactor = null) {
        if (interactor != null) {
            RigidbodyPlayerController pCode = interactor.GetComponent<RigidbodyPlayerController>();
            pCode.RagDoll();
            pCode.GetRB().AddForceAtPosition(transform.forward * 14f, interactor.transform.position + Vector3.up * 0.002f + Vector3.right * Random.Range(-0.005f, 0.005f), ForceMode.Impulse); 
        }
    }
}
