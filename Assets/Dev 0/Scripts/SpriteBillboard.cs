using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    [SerializeField] bool freezeXY = true;
    void LateUpdate()
    {
        if (freezeXY == true)
        {
            transform.rotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        } else
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}
