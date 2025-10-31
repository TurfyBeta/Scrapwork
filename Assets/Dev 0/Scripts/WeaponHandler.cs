using UnityEngine;
using UnityEditor.Animations;

interface Weapon {
    AnimatorController GetAnimCont();
}

public class WeaponHandler : MonoBehaviour
{
    public Animator playerAnim;
    public SpriteRenderer spriteRend;
    public GameObject[] weapons;
    private int SelectedWeapon = 0;
    private bool SwappedWeapons = false;

    void Start()
    {
        weapons[0] = null;
    }

    void Update()
    {
        PInputHandler();

        // Only care about the selected weapon
        GameObject currentWeapon = weapons[SelectedWeapon];

        if (currentWeapon != null)
        {
            // Enable only the selected weapon
            for (int i = 0; i < weapons.Length; i++)
                weapons[i]?.SetActive(i == SelectedWeapon);

            // Swap animator controller when needed
            if (SwappedWeapons)
            {
                spriteRend.sprite = null;
                var weaponComp = currentWeapon.GetComponent<Weapon>();
                if (weaponComp != null)
                {
                    var newController = weaponComp.GetAnimCont();
                    if (newController != null)
                    {
                        playerAnim.runtimeAnimatorController = null; // Force reload
                        playerAnim.runtimeAnimatorController = newController;
                        Debug.Log("Swapped to controller: " + newController.name);
                    }
                }
            }
        }
        else
        {
            playerAnim.runtimeAnimatorController = null;
        }

        // Handle aiming animation
        if (playerAnim.runtimeAnimatorController != null)
            playerAnim.SetBool("Aiming", Input.GetMouseButton(1));

        SwappedWeapons = false;
    }

    void PInputHandler()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && SelectedWeapon != 0)
        {
            SwappedWeapons = true;
            SelectedWeapon = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && SelectedWeapon != 1)
        {
            SwappedWeapons = true;
            SelectedWeapon = 1;
        }
    }
}
