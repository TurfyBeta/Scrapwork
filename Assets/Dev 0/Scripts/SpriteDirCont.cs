using UnityEngine;

public class SpriteDirCont : MonoBehaviour
{
    [Header("Angle Thresholds")]
    [SerializeField] float backAngle = 45f;      // 0° to this = back
    [SerializeField] float backSideAngle = 90f;  // 45°–90° = back-side (45°)
    [SerializeField] float sideAngle = 135f;     // 90°–135° = side
    [SerializeField] float frontSideAngle = 160f;// 135°–160° = front-side (45°)
    [SerializeField] float frontAngle = 180f;    // beyond = front

    [Header("References")]
    [SerializeField] Transform mainTrans;
    [SerializeField] Animator anim;
    [SerializeField] SpriteRenderer spriteRend;

    void LateUpdate()
    {
        Vector3 camForward = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z);
        float signedAngle = Vector3.SignedAngle(mainTrans.forward, camForward, Vector3.up);
        float angle = Mathf.Abs(signedAngle);

        // Flip sprite depending on camera side
        spriteRend.flipX = signedAngle >= 0f;

        // Determine animDir based on angle
        Vector2 animDir;

        if (angle < backAngle)
        {
            // Back
            animDir = new Vector2(0f, -1f);
        }
        else if (angle < backSideAngle)
        {
            // Back-side diagonal (e.g., back-right or back-left)
            animDir = new Vector2(0.707f, -0.707f);
        }
        else if (angle < sideAngle)
        {
            // Side
            animDir = new Vector2(1f, 0f);
        }
        else if (angle < frontSideAngle)
        {
            // Front-side diagonal (e.g., front-right or front-left)
            animDir = new Vector2(0.707f, 0.707f);
        }
        else
        {
            // Front
            animDir = new Vector2(0f, 1f);
        }

        anim.SetFloat("moveX", animDir.x);
        anim.SetFloat("moveY", animDir.y);
    }
}
