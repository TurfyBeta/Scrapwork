using UnityEngine;
using UnityEditor.Animations;

public class Pistol : MonoBehaviour, Weapon
{

    public AnimatorController AnimCont;

    public AnimatorController GetAnimCont()
    {
        return AnimCont;
    }

}
