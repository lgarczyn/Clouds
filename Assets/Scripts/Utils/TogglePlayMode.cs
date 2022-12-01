using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Linq;

[RequireComponent(typeof(MouseCapture))]
public class TogglePlayMode : MonoBehaviour
{
  [SerializeField][RequiredComponent] MouseCapture reqMouseCapture;

  bool playing
  {
    get => Time.timeScale != 0f;
    set
    {
      Time.timeScale = value ? 1f : 0f;
      AudioListener.pause = !value;
    }
  }

  // Check just in case scene was reloaded while paused
  void Start()
  {
    playing = true;
    reqMouseCapture.ShouldCapture = true;
  }

  public void OnPlayPause(InputAction.CallbackContext context)
  {
    if (context.phase != InputActionPhase.Performed) return;

    playing = !playing;
    reqMouseCapture.ShouldCapture = playing;
  }

  public void OnReload(InputAction.CallbackContext context)
  {
    if (context.phase != InputActionPhase.Performed) return;

    playing = true;
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
  }
}
