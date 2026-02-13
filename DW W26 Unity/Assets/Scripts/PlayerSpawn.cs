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

        // ===== ROLE ASSIGNMENT =====
        bool isSniper = (PlayerCount == 0);

        SniperLook sniperLook = playerInput.GetComponent<SniperLook>();

        // Disable runner movement if sniper
        if (controller != null)
        {
            controller.enabled = !isSniper;
        }

        // Enable sniper look only for Player 1
        if (sniperLook != null)
        {
            sniperLook.enabled = isSniper;
        }

        SniperShoot sniperShoot = playerInput.GetComponent<SniperShoot>();
        if (sniperShoot != null)
        {
            sniperShoot.enabled = isSniper;
        }

        if (isSniper)
        {
            SniperCameraFollow sniperCam = FindFirstObjectByType<SniperCameraFollow>();

            var shoot = playerInput.GetComponent<SniperShoot>();
            
            if (shoot != null)
            {
                Camera display1Cam = GameObject.Find("Display1 Camera")?.GetComponent<Camera>();
                if (display1Cam != null)
                    shoot.sniperCamera = display1Cam;
                else
                    Debug.LogWarning("Could not find 'Display1 Camera' to assign to SniperShoot.");
            }

            if (sniperCam != null)
            {
            Transform anchor = playerInput.transform.Find("SniperCameraAnchor");
            sniperCam.target = anchor;
            }
        }

        Debug.Log($"Player {PlayerCount + 1} is {(isSniper ? "SNIPER" : "RUNNER")}");

        // --- 4) Increment count LAST (avoids off-by-one mistakes) ---
        PlayerCount++;
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        // Not handling anything right now.
        Debug.Log("Player left...");
    }
}
