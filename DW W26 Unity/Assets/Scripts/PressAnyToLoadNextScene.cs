using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PressAnyToLoadNextScene : MonoBehaviour
{
    [SerializeField] private int nextSceneIndex = 1; // Controls scene

    private void Update()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame ||
            (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame))
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}