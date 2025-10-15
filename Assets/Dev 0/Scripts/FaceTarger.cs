using UnityEngine;

public class FaceTarget : MonoBehaviour
{
    public Transform target;

    // Update is called once per frame
    void Update()
    {
        Vector3 relPos = transform.InverseTransformPoint(target.position);

        if (relPos.x < 0) {
            transform.Rotate(Vector3.up, -24f * Time.deltaTime);
        } else {
            transform.Rotate(Vector3.up, 24f * Time.deltaTime);
        }
    }
}
