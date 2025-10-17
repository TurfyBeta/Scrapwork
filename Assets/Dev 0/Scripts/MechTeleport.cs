using UnityEngine;

public class MechTeleport : MonoBehaviour, IInteractable
{
    public Transform destination;
    public MechController mechCon;


    public void Interact(GameObject interactor = null) {
        interactor.transform.position = destination.transform.position;
        mechCon.MechEnterExit();
    }
}
