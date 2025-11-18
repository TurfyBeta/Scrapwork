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
        playerAnim.runtimeAnimatorController = null;
    }

    void Update()
    {
        PInputHandler();

        if (SwappedWeapons)
        {
            UpdateRefs();
        }

        // Handle aiming animation
        if (playerAnim.runtimeAnimatorController != null)
            playerAnim.SetBool("Aiming", Input.GetMouseButton(1));
            playerAnim.SetBool("Reloading", Input.GetKeyDown(KeyCode.R));

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

    void UpdateRefs()
    {
        GameObject currentWeapon = weapons[SelectedWeapon];

        if (currentWeapon != null)
        {
            // Enable only the selected weapon
            for (int i = 0; i < weapons.Length; i++)
                weapons[i]?.SetActive(i == SelectedWeapon);

            // Swap animator controller when needed
            
            spriteRend.sprite = null;
                var weaponComp = currentWeapon.GetComponent<Weapon>();
                if (weaponComp != null)
                {
                    var newController = weaponComp.GetAnimCont();
                    if (newController != null)
                    {
                        playerAnim.runtimeAnimatorController = null;
                        playerAnim.runtimeAnimatorController = newController;
                    }
                }
        }
        else
        {
            playerAnim.runtimeAnimatorController = null;
        }
    }
}
