using UnityEngine;

public class SpriteDirCont : MonoBehaviour
{

    [SerializeField] float backAngle = 65f;
    [SerializeField] float sideAngle = 135f;
    [SerializeField] Transform mainTrans;
    [SerializeField] Animator anim;
    [SerializeField] SpriteRenderer spriteRend;

    void LateUpdate()
    {
        Vector3 CamForward = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z);
        float singedAngle = Vector3.SignedAngle(mainTrans.forward, CamForward, Vector3.up);

        Vector2 animDir = new Vector2(0f, -1f);

        float angle = Mathf.Abs(singedAngle);

        if (angle < backAngle)
        {
            animDir = new Vector2(0f, -1f);
        }
        else if (angle < sideAngle)
        {
            animDir = new Vector2(1f, 0f);
        }
        else
        {
            animDir = new Vector2(0f, 1f);
        }

        anim.SetFloat("moveX",animDir.x);
        anim.SetFloat("moveY",animDir.y);

    }
}
