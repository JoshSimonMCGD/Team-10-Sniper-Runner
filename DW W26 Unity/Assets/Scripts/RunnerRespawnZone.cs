using UnityEngine;

public class RunnerRespawnZone : MonoBehaviour
{
    [Header("Respawn points near this zone")]
    public Transform[] respawnPoints;

    [Header("Cooldown to prevent spam")]
    public float zoneCooldown = 1.0f;

    private float _nextAllowedTime = 0f;
    private int _nextPointIndex = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < _nextAllowedTime) return;

        PlayerController3D entering = other.GetComponentInParent<PlayerController3D>();
        if (entering == null) return;

        // Only living runners can trigger a team revive
        if (entering.PlayerNumber < 2) return; // ignore sniper
        if (entering.IsDead) return;

        if (respawnPoints == null || respawnPoints.Length == 0) return;

        // Find dead runners and revive them
        PlayerController3D[] players = FindObjectsByType<PlayerController3D>(FindObjectsSortMode.None);

        bool revivedAnyone = false;

        for (int i = 0; i < players.Length; i++)
        {
            PlayerController3D p = players[i];
            if (p == null) continue;

            if (p.PlayerNumber < 2) continue; // runners only
            if (!p.IsDead) continue;

            Transform point = respawnPoints[_nextPointIndex % respawnPoints.Length];
            _nextPointIndex++;

            p.ReviveAt(point);

            revivedAnyone = true;
        }

        if (revivedAnyone)
        {
            _nextAllowedTime = Time.time + zoneCooldown;
        }
    }
}
