using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawn : MonoBehaviour
{
    [field: SerializeField] public Transform[] SpawnPoints { get; private set; }
    [field: SerializeField] public Color[] PlayerColors { get; private set; }

    [SerializeField] private CameraFollow RunnerCamera; // assign DisplayCamera2 here in Inspector
    public int PlayerCount { get; private set; }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        int maxPlayerCount = Mathf.Min(SpawnPoints.Length, PlayerColors.Length);
        if (maxPlayerCount < 1)
        {
            string msg =
                $"You forgot to assign {name}'s {nameof(PlayerSpawn)}.{nameof(SpawnPoints)}" +
                $"and {nameof(PlayerSpawn)}.{nameof(PlayerColors)}!";
            Debug.Log(msg);
            return;
        }

        // Attach Runner Camera (DisplayCamera2) to Player 1
        if (PlayerCount == 0 && RunnerCamera != null)
        {
            RunnerCamera.Target = playerInput.transform;
        }

        // Prevent adding in more than max number of players
        if (PlayerCount >= maxPlayerCount)
        {
            // Delete new object
            string msg =
                $"Max player count {maxPlayerCount} reached. " +
                $"Destroying newly spawned object {playerInput.gameObject.name}.";
            Debug.Log(msg);
            Destroy(playerInput.gameObject);
            return;
        }

        // --- 1) Pick spawn + color using CURRENT PlayerCount (before increment) ---
        Transform spawn = SpawnPoints[PlayerCount];
        Color color = PlayerColors[PlayerCount];

        // --- 2) Place player at spawn (physics-safe if Rigidbody exists) ---
        Rigidbody rb = playerInput.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.position = spawn.position;
            rb.rotation = spawn.rotation;

            // Prevent drift / weird first-frame motion
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            playerInput.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
        }

        // --- 3) Set up the *3D* player controller (this was the big bug) ---
        PlayerController3D controller = playerInput.GetComponent<PlayerController3D>();
        if (controller == null)
        {
            Debug.LogError($"{playerInput.name} spawned but has no PlayerController3D component.");
        }
        else
        {
            controller.AssignPlayerInputDevice(playerInput);

            // PlayerNumber should be 1-based (Player 1, Player 2...)
            controller.AssignPlayerNumber(PlayerCount + 1);

            // Optional: player identity/readability
            controller.AssignColor(color);
        }

        // --- 4) Increment count LAST (avoids off-by-one mistakes) ---
        PlayerCount++;
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        // Not handling anything right now.
        Debug.Log("Player left...");
    }
}
