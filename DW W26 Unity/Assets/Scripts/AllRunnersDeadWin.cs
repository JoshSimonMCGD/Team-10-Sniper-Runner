using UnityEngine;
using UnityEngine.InputSystem;

public class AllRunnersDeadWin : MonoBehaviour
{
    [SerializeField] private GameFlowManager flow;
    [SerializeField] private PlayerInputManager inputManager; // assign from Player Input Manager object

    private void Start()
    {
        if (flow == null)
            flow = FindFirstObjectByType<GameFlowManager>();

        if (inputManager == null)
            inputManager = FindFirstObjectByType<PlayerInputManager>();
    }

    private void Update()
    {
        if (flow == null) return;
        if (inputManager == null) return;

        // IMPORTANT: only evaluate win AFTER joining is locked
        if (inputManager.joiningEnabled) return;

        var players = FindObjectsByType<PlayerController3D>(FindObjectsSortMode.None);

        bool anyRunnerExists = false;
        bool anyRunnerAlive = false;

        for (int i = 0; i < players.Length; i++)
        {
            var p = players[i];
            if (p == null) continue;

            if (p.PlayerNumber < 2) continue; // runners only

            anyRunnerExists = true;

            if (!p.IsDead)
            {
                anyRunnerAlive = true;
                break;
            }
        }

        if (anyRunnerExists && !anyRunnerAlive)
            flow.ShowSniperWins();
    }
}
