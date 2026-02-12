using UnityEngine;

public class SniperCameraFollow : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
