using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController3D : MonoBehaviour
{
    [field: SerializeField] public int PlayerNumber { get; private set; }
    [field: SerializeField] public Color PlayerColor { get; private set; }

    [Header("References")]
    [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
    [field: SerializeField] public Renderer ColorRenderer { get; private set; }

    [field: SerializeField] public CapsuleCollider Capsule { get; private set; }
    [SerializeField] private Transform moveReference; //camera transform for camera-relative movement

    [Header("Movement")]
    [field: SerializeField] public float MoveSpeed { get; private set; } = 6f;
    [field: SerializeField] public float JumpForce { get; private set; } = 5f;
    [SerializeField] private float airControlMultiplier = 0.5f;
    [field: SerializeField] public Animator Animator { get; private set; }
    


    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private LayerMask groundLayers = ~0; // everything by default

    public bool DoJump { get; private set; }

    public bool IsDead { get; private set; }
    [SerializeField] private GameObject VisualRoot; // assign your model root here (optional)
    [SerializeField] private Collider MainCollider; // assign CapsuleCollider (optional)

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

        // Use LOD0's material as a fallback for any LOD renderers that have missing material slots
        Material fallbackMat = ColorRenderer.sharedMaterial != null ? ColorRenderer.sharedMaterial : ColorRenderer.material;

        // Apply to ALL renderers under this player (includes Mascot_LOD1, Mascot_LOD2, etc.)
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();

        foreach (Renderer r in renderers)
        {
            if (r == null) continue;

            // If any material slots are missing, fill them with the fallback material
            // (prevents LOD meshes showing default/missing at distance)
            if (fallbackMat != null)
            {
                var shared = r.sharedMaterials;
                bool hasMissing = (shared == null || shared.Length == 0);

                if (!hasMissing)
                {
                    for (int i = 0; i < shared.Length; i++)
                    {
                        if (shared[i] == null) { hasMissing = true; break; }
                    }
                }

                if (hasMissing)
                {
                    int count = (shared != null && shared.Length > 0) ? shared.Length : 1;
                    Material[] filled = new Material[count];
                    for (int i = 0; i < count; i++) filled[i] = fallbackMat;
                    r.sharedMaterials = filled;
                }
            }

            // Per-player tint without duplicating materials
            r.GetPropertyBlock(mpb);
            mpb.SetColor("_BaseColor", color); // URP Lit
            mpb.SetColor("_Color", color);     // Built-in/legacy
            r.SetPropertyBlock(mpb);
        }
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

        

        #if UNITY_EDITOR
        // DEBUG: Press R to revive this player at world origin
        if (IsDead && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ReviveAt(Vector3.zero, Quaternion.identity);
        }
        #endif
    }

    void FixedUpdate()
    {
        if (IsDead) return;

        if (Rigidbody == null)
        {
            Debug.Log($"{name}'s {nameof(PlayerController3D)}.{nameof(Rigidbody)} is null.");
            return;
        }

        Vector2 move2 = InputActionMove != null ? InputActionMove.ReadValue<Vector2>() : Vector2.zero;

        // ANIMATION: moving / not moving
        if (Animator != null)
        {
            bool isMoving = move2.sqrMagnitude > 0.001f;
            Animator.SetBool("IsMoving", isMoving);
        }

        // Convert Vector2 -> Vector3 (XZ plane)
        Vector3 desired = new Vector3(move2.x, 0f, move2.y);

        // ROTATION: face movement direction
        if (desired.sqrMagnitude > 0.001f)
        {
            Vector3 lookDir = new Vector3(desired.x, 0f, desired.z);
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.fixedDeltaTime);
        }

        // camera-relative movement
        if (moveReference != null)
        {
            Vector3 fwd = moveReference.forward;
            Vector3 right = moveReference.right;
            fwd.y = 0f; right.y = 0f;
            fwd.Normalize(); right.Normalize();
            desired = (right * move2.x + fwd * move2.y);
        }

        bool grounded = IsGrounded();

        if (Animator != null)
        Animator.SetBool("IsGrounded", grounded);

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
                if (Animator != null)
                Animator.SetTrigger("Jump");
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
        if (Animator == null) Animator = GetComponentInChildren<Animator>();

        if (VisualRoot == null)
        VisualRoot = (Animator != null) ? Animator.gameObject : null;

        if (MainCollider == null)
        MainCollider = Capsule;
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;

        // Stop motion + stop physics interactions
        if (Rigidbody != null)
        {
            Rigidbody.linearVelocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
            Rigidbody.isKinematic = true;
        }

        if (MainCollider != null)
            MainCollider.enabled = false;

        // Hide model (optional but recommended)
        if (VisualRoot != null)
            VisualRoot.SetActive(false);

        // Animation flags (optional)
        if (Animator != null)
        {
            Animator.SetBool("IsMoving", false);
        }
    }

    public void ReviveAt(Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("ReviveAt called with null spawnPoint.");
            return;
        }

        ReviveAt(spawnPoint.position, spawnPoint.rotation);
    }

    public void ReviveAt(Vector3 position, Quaternion rotation)
    {
        // Mark alive first (so movement can resume after everything is enabled)
        IsDead = false;

        // Move the player safely
        if (Rigidbody != null)
        {
            // While kinematic, we can reposition without physics fighting us
            Rigidbody.isKinematic = true;

            Rigidbody.linearVelocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;

            Rigidbody.position = position;
            Rigidbody.rotation = rotation;

            // Re-enable physics
            Rigidbody.isKinematic = false;
            Rigidbody.WakeUp();
        }
        else
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        // Re-enable collisions
        if (MainCollider != null)
            MainCollider.enabled = true;

        // Show model again
        if (VisualRoot != null)
            VisualRoot.SetActive(true);

        // Reset animation flags (optional)
        if (Animator != null)
        {
            Animator.SetBool("IsMoving", false);
        }
    }
}