using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController3D : MonoBehaviour
{
    [field: SerializeField] public int PlayerNumber { get; private set; }
    [field: SerializeField] public Color PlayerColor { get; private set; }

    [Header("References")]
    [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
    [field: SerializeField] public Renderer ColorRenderer { get; private set; } // MeshRenderer or SkinnedMeshRenderer

    [field: SerializeField] public CapsuleCollider Capsule { get; private set; }
    [SerializeField] private Transform moveReference; //camera transform for camera-relative movement

    [Header("Movement")]
    [field: SerializeField] public float MoveSpeed { get; private set; } = 6f;
    [field: SerializeField] public float JumpForce { get; private set; } = 5f;
    [SerializeField] private float airControlMultiplier = 0.5f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private LayerMask groundLayers = ~0; // everything by default

    public bool DoJump { get; private set; }

    // Player input information
    private PlayerInput PlayerInput;
    private InputAction InputActionMove;
    private InputAction InputActionJump;

    public void AssignColor(Color color)
    {
        PlayerColor = color;

        if (ColorRenderer == null)
        {
            Debug.Log($"Failed to set color to {name} {nameof(PlayerController3D)}.");
            return;
        }

        // Most materials use "_BaseColor" (URP Lit) or "color" (legacy).
        // Try both safely.
        var mat = ColorRenderer.material;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        else if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
    }

    public void AssignPlayerInputDevice(PlayerInput playerInput)
    {
        PlayerInput = playerInput;

        // Same action names as your 2D controller
        InputActionMove = playerInput.actions.FindAction("Player/Move");
        InputActionJump = playerInput.actions.FindAction("Player/Jump");

        if (InputActionMove == null || InputActionJump == null)
        {
            Debug.LogError($"{name}: Could not find Move/Jump actions. Check action map names.");
        }
    }

    public void AssignPlayerNumber(int playerNumber) => PlayerNumber = playerNumber;

    void Update()
    {
        if (InputActionJump != null && InputActionJump.WasPressedThisFrame())
            DoJump = true;
    }

    void FixedUpdate()
    {
        if (Rigidbody == null)
        {
            Debug.Log($"{name}'s {nameof(PlayerController3D)}.{nameof(Rigidbody)} is null.");
            return;
        }

        Vector2 move2 = InputActionMove != null ? InputActionMove.ReadValue<Vector2>() : Vector2.zero;

        // Convert Vector2 -> Vector3 (XZ plane)
        Vector3 desired = new Vector3(move2.x, 0f, move2.y);

        // Optional: camera-relative movement (recommended for 3D)
        if (moveReference != null)
        {
            Vector3 fwd = moveReference.forward;
            Vector3 right = moveReference.right;
            fwd.y = 0f; right.y = 0f;
            fwd.Normalize(); right.Normalize();
            desired = (right * move2.x + fwd * move2.y);
        }

        bool grounded = IsGrounded();

        // Movement via velocity set (keeps gravity + collisions, avoids force-sliding)
        Vector3 vel = Rigidbody.linearVelocity; // Unity 6: linearVelocity (vs velocity in older versions)
        float control = grounded ? 1f : airControlMultiplier;

        Vector3 targetHorizontal = desired * MoveSpeed * control;
        vel.x = targetHorizontal.x;
        vel.z = targetHorizontal.z;
        Rigidbody.linearVelocity = vel;

        // Jump (buffered)
        if (DoJump)
        {
            DoJump = false;

            if (grounded)
            {
                // Clear downward velocity so jump is consistent
                Vector3 v = Rigidbody.linearVelocity;
                if (v.y < 0f) v.y = 0f;
                Rigidbody.linearVelocity = v;

                Rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
            }
        }
    }

    private bool IsGrounded()
    {
        if (Capsule == null) return false;

        // World-space center of the capsule
        Vector3 center = Capsule.transform.TransformPoint(Capsule.center);

        // Compute the bottom sphere center of the capsule in world space
        float radius = Mathf.Max(0.01f, Capsule.radius * Mathf.Max(Capsule.transform.lossyScale.x, Capsule.transform.lossyScale.z));
        float halfHeight = Mathf.Max(Capsule.height * 0.5f * Capsule.transform.lossyScale.y, radius);

        Vector3 bottom = center + Vector3.down * (halfHeight - radius);

        // Cast a tiny distance below the capsule to check for ground contact
        float castDistance = groundCheckDistance;
        return Physics.SphereCast(bottom + Vector3.up * 0.01f, radius * 0.95f, Vector3.down,
            out _, castDistance + 0.01f, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void OnValidate() => Reset();

    private void Reset()
    {
        if (Rigidbody == null) Rigidbody = GetComponent<Rigidbody>();
        if (Capsule == null) Capsule = GetComponent<CapsuleCollider>();
        if (ColorRenderer == null) ColorRenderer = GetComponentInChildren<Renderer>();
    }
}