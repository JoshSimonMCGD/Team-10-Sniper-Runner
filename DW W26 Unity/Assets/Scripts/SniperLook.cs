using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class SniperLook : MonoBehaviour
{
    [Header("References")]
    public Transform cameraAnchor;

    [Header("Settings")]
    public float sensitivity = 0.12f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Action name in Input Actions (Player map)")]
    public string lookActionName = "Look";

    private InputAction _look;
    private float _pitch;

    private void Awake()
    {
        var playerInput = GetComponent<PlayerInput>();
        _look = playerInput.actions[lookActionName];

        if (_look == null)
            Debug.LogError($"SniperLook: Could not find action '{lookActionName}' on {name}.");
    }

    private void OnEnable()
    {
        _look?.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        _look?.Disable();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (cameraAnchor == null || _look == null) return;

        Vector2 look = _look.ReadValue<Vector2>();

        // Mouse delta is already "per-frame", stick is more like "per-frame" too in practice with Input System.
        // Keep it simple: same sensitivity for both for now.
        float yaw = look.x * sensitivity;
        float pitchDelta = look.y * sensitivity;

        // Yaw rotates player body (left/right)
        transform.Rotate(0f, yaw, 0f);

        // Pitch rotates anchor (up/down)
        _pitch -= pitchDelta;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        cameraAnchor.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }
}