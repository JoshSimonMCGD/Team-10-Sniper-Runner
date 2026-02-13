using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Single Target Mode (old behavior)")]
    public Transform Target;
    public bool LookAtTarget = true;

    [Header("Offset / Follow")]
    public Vector3 Offset = new Vector3(0f, 10f, -15f);
    public float PositionSmoothTime = 0.20f;
    public float RotationLerpSpeed = 10f;

    [Header("Group Mode (new behavior)")]
    public bool UseGroupMode = true;
    public int FirstRunnerPlayerNumber = 2; // P2+ are runners

    [Header("Auto Zoom (FOV)")]
    public Camera Cam; // assign this to the same camera component
    public float MinFov = 55f;
    public float MaxFov = 80f;
    public float SpreadMin = 6f;
    public float SpreadMax = 30f;
    public float FovSmoothTime = 0.15f;

    Vector3 _posVel;
    float _fovVel;

    void Reset()
    {
        Cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (UseGroupMode)
        {
            FollowRunnerGroup();
        }
        else
        {
            FollowSingleTarget();
        }
    }

    void FollowSingleTarget()
    {
        if (Target == null) return;

        Vector3 desiredPos = Target.position + Offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _posVel, PositionSmoothTime);

        if (LookAtTarget)
        {
            Quaternion desiredRot = Quaternion.LookRotation(Target.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, RotationLerpSpeed * Time.deltaTime);
        }
    }

    void FollowRunnerGroup()
    {
        if (Cam == null) Cam = GetComponent<Camera>();

        // Find all players (jam-scale OK)
        PlayerController3D[] players = FindObjectsByType<PlayerController3D>(FindObjectsSortMode.None);

        bool hasRunner = false;
        Bounds b = new Bounds(Vector3.zero, Vector3.zero);

        for (int i = 0; i < players.Length; i++)
        {
            var p = players[i];
            if (p == null) continue;

            // Only runners
            if (p.PlayerNumber < FirstRunnerPlayerNumber) continue;

            // Only alive
            if (p.IsDead) continue;

            Vector3 pos = p.transform.position;

            if (!hasRunner)
            {
                b = new Bounds(pos, Vector3.zero);
                hasRunner = true;
            }
            else
            {
                b.Encapsulate(pos);
            }
        }

        if (!hasRunner) return;

        Vector3 center = b.center;
        Vector3 desiredPos = center + Offset;

        // Smooth position
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _posVel, PositionSmoothTime);

        // Smooth rotation
        if (LookAtTarget)
        {
            Quaternion desiredRot = Quaternion.LookRotation(center - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, RotationLerpSpeed * Time.deltaTime);
        }

        // Auto zoom based on spread
        if (Cam != null)
        {
            float spread = Mathf.Max(b.size.x, b.size.z);
            float t = Mathf.InverseLerp(SpreadMin, SpreadMax, spread);
            float desiredFov = Mathf.Lerp(MinFov, MaxFov, t);

            Cam.fieldOfView = Mathf.SmoothDamp(Cam.fieldOfView, desiredFov, ref _fovVel, FovSmoothTime);
        }
    }
}