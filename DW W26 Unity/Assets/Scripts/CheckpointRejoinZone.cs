using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class CheckpointRejoinZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerSpawn playerSpawn;           // on the PlayerInputManager object
    [SerializeField] private PlayerInputManager inputManager;   // Unity component

    [Header("New spawn positions (0 = Sniper, 1–5 = Runners)")]
    [SerializeField] private Transform spawn0_Sniper;
    [SerializeField] private Transform spawn1_Runner;
    [SerializeField] private Transform spawn2_Runner;
    [SerializeField] private Transform spawn3_Runner;
    [SerializeField] private Transform spawn4_Runner;
    [SerializeField] private Transform spawn5_Runner;

    [Header("Rejoin window")]
    [SerializeField] private float rejoinSeconds = 5f;

    private bool _rejoinRunning;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log($"[CheckpointRejoinZone] Triggered by: {other.name}", this);
        // Only runners trigger (PlayerNumber 2+)
        var pc = other.GetComponentInParent<PlayerController3D>();
        if (pc == null) return;
        if (pc.PlayerNumber < 2) return;

        MoveSpawnPointsHere();

        if (!_rejoinRunning)
            StartCoroutine(RejoinWindow());
    }

    private void MoveSpawnPointsHere()
    {
        if (playerSpawn == null || playerSpawn.SpawnPoints == null) return;
        if (playerSpawn.SpawnPoints.Length < 6) return;

        if (spawn0_Sniper != null)
            playerSpawn.SpawnPoints[0].SetPositionAndRotation(spawn0_Sniper.position, spawn0_Sniper.rotation);

        if (spawn1_Runner != null)
            playerSpawn.SpawnPoints[1].SetPositionAndRotation(spawn1_Runner.position, spawn1_Runner.rotation);

        if (spawn2_Runner != null)
            playerSpawn.SpawnPoints[2].SetPositionAndRotation(spawn2_Runner.position, spawn2_Runner.rotation);

        if (spawn3_Runner != null)
            playerSpawn.SpawnPoints[3].SetPositionAndRotation(spawn3_Runner.position, spawn3_Runner.rotation);

        if (spawn4_Runner != null)
            playerSpawn.SpawnPoints[4].SetPositionAndRotation(spawn4_Runner.position, spawn4_Runner.rotation);

        if (spawn5_Runner != null)
            playerSpawn.SpawnPoints[5].SetPositionAndRotation(spawn5_Runner.position, spawn5_Runner.rotation);
    }

    private IEnumerator RejoinWindow()
    {
        _rejoinRunning = true;

        if (inputManager != null)
            inputManager.EnableJoining();

        yield return new WaitForSeconds(rejoinSeconds);

        if (inputManager != null)
            inputManager.DisableJoining();

        _rejoinRunning = false;
    }
}