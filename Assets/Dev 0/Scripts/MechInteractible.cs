using UnityEngine;


public class MechInteractible : MonoBehaviour, MechInteractable
{
    public enum ButtonType
    {
        ComponentPower,
        AccessoryPower
    }
    public ButtonType BT;
    public string Key = "None";
    public float ButtonValue = 0f;

    public void MechInteract(GameObject MCGO) 
    {
        MechController MC = MCGO.GetComponent<MechController>();
        switch (BT)
        {
            case ButtonType.ComponentPower:
                MC.SetComponentPower(Key,ButtonValue);
                break;
            case ButtonType.AccessoryPower:
                MC.SetAccessoryPower(Key,ButtonValue);
                break;
        }
    }
}
