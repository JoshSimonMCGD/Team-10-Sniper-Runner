using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;
    public Vector3 Offset = new Vector3(0f, 10f, -15f);
    public bool LookAtTarget = true;

    void LateUpdate()
    {
        if (Target == null) return;

        transform.position = Target.position + Offset;

        if (LookAtTarget)
            transform.LookAt(Target);
    }
}