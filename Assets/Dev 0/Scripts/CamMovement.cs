using UnityEngine;


public class CamMovement : MonoBehaviour
{
    public float MoveSpeed = 7f;
    public float TurnSpeed = 100f;

    void FixedUpdate()
    {
        float adMove = Input.GetAxis("Horizontal");
        float wsMove = Input.GetAxis("Vertical");

        // Movement relative to camera direction
        Vector3 moveDir = (transform.forward * wsMove + transform.right * adMove).normalized;
        transform.position += moveDir * MoveSpeed * Time.deltaTime;

        // Rotation (yaw)
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.up, -TurnSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.up, TurnSpeed * Time.deltaTime);
        }
    }
}
