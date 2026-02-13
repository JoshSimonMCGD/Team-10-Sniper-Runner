using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RunnerVictoryZone : MonoBehaviour
{
    [SerializeField] private GameFlowManager flow;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var pc = other.GetComponentInParent<PlayerController3D>();
        if (pc == null) return;

        // runners only (Player 1 is sniper)
        if (pc.PlayerNumber < 2) return;

        if (flow == null)
            flow = FindFirstObjectByType<GameFlowManager>();

        if (flow != null)
            flow.ShowRunnersWin();
    }
}
