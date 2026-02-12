using UnityEngine;

public class SniperController : MonoBehaviour
{
    [Header("Temp - just to prove it works")]
    public float lookSpeed = 120f;

    private void OnEnable()
    {
        Debug.Log($"[SniperController] ENABLED on {name}");
    }

    private void OnDisable()
    {
        Debug.Log($"[SniperController] DISABLED on {name}");
    }

    private void Update()
    {
        // Placeholder so we can see it’s active.
        // replace this with Input System aim + raycast later.
        float yaw = Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
        transform.Rotate(0f, yaw, 0f);
    }
}