using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MouseCapture))]
[RequireComponent(typeof(PlayerInput))]
public class TogglePlayMode : MonoBehaviour
{
  [SerializeField][RequiredComponent] MouseCapture reqMouseCapture;
  [SerializeField][RequiredComponent] PlayerInput reqPlayerInput;

  bool lastPlayingState = true;

  bool Playing
  {
    get => Time.timeScale != 0f;
    set
    {
      Time.timeScale = value ? 1f : 0f;
      AudioListener.pause = !value;

      reqPlayerInput.SwitchCurrentActionMap(value ? "Game" : "UI");
      reqMouseCapture.IsCaptured = value;
    }
  }

  // Check just in case scene was reloaded while paused
  void Start()
  {
    Playing = true;
  }

  public void OnPlayPause(InputAction.CallbackContext context)
  {
    if (context.phase != InputActionPhase.Performed) return;

    Playing = !Playing;
  }

  public void OnReload(InputAction.CallbackContext context)
  {
    if (context.phase != InputActionPhase.Performed) return;

    Playing = true;
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
  }

  private void OnApplicationFocus(bool focusStatus)
  {
    // If focusing in, restore play status
    if (focusStatus)
    {
      Playing = lastPlayingState;
    }
    // If focusing out, store state and pause
    else
    {
      lastPlayingState = Playing;
      Playing = false;
    }
  }
}
