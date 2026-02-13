using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class SniperShoot : MonoBehaviour
{
    [Header("References")]
    public Camera sniperCamera;

    [Header("Settings")]
    public float maxDistance = 10000f;
    public LayerMask hitMask = ~0; // everything by default
    public float fireCooldown = 0.6f;
    private float _nextFireTime = 0f;

    [Header("Input (Action name in Player map)")]
    public string fireActionName = "Attack";

    private InputAction _fire;

    private void Awake()
    {
        var playerInput = GetComponent<PlayerInput>();
        _fire = playerInput.actions[fireActionName];

        if (_fire == null)
            Debug.LogError($"SniperShoot: Could not find action '{fireActionName}' on {name}.");
    }

    private void OnEnable()
    {
        _fire?.Enable();
    }

    private void OnDisable()
    {
        _fire?.Disable();
    }

    private void Update()
    {
        if (sniperCamera == null || _fire == null) return;

        // fire once per press
        if (!_fire.IsPressed()) return;

        if (Time.time < _nextFireTime) return;
        _nextFireTime = Time.time + fireCooldown;

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlaySniperShot();
        }

        Ray ray = new Ray(sniperCamera.transform.position, sniperCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            // Look for a runner controller on the hit object or its parents
            PlayerController3D runner = hit.collider.GetComponentInParent<PlayerController3D>();
            if (runner != null)
            {
                runner.Die();
            }
        }
        else
        {
            Debug.Log("[SniperShoot] Miss");
        }
    }
}
