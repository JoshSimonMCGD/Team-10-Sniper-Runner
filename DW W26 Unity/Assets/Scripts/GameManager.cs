using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    [Header("Display 1 UI (Sniper)")]
    [SerializeField] private GameObject d1_SniperWins;
    [SerializeField] private GameObject d1_RunnersWin;

    [Header("Display 2 UI (Runners)")]
    [SerializeField] private GameObject d2_SniperWins;
    [SerializeField] private GameObject d2_RunnersWin;

    [Header("Restart")]
    [SerializeField] private int restartSceneIndex = 0; // Scene_Title

    private bool gameOver;

    private void Start()
    {
        SetAllOff();
    }

    private void Update()
    {
        if (!gameOver) return;

        if (Keyboard.current.anyKey.wasPressedThisFrame ||
            (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame))
        {
            SceneManager.LoadScene(restartSceneIndex);
        }
    }

    private void SetAllOff()
    {
        SetUI(d1_SniperWins, false);
        SetUI(d1_RunnersWin, false);
        SetUI(d2_SniperWins, false);
        SetUI(d2_RunnersWin, false);
    }

    private void SetUI(GameObject obj, bool state)
    {
        if (obj != null) obj.SetActive(state);
    }

    // If all runners die: Sniper wins (Display1 shows SniperWins, Display2 shows SniperWins as defeat for runners if that art matches)
    public void ShowSniperWins()
    {
        if (gameOver) return;
        gameOver = true;

        SetAllOff();
        SetUI(d1_SniperWins, true);
        SetUI(d2_SniperWins, true);
    }

    // If runner reaches end: Runners win
    public void ShowRunnersWin()
    {
        if (gameOver) return;
        gameOver = true;

        SetAllOff();
        SetUI(d1_RunnersWin, true);
        SetUI(d2_RunnersWin, true);
    }
}