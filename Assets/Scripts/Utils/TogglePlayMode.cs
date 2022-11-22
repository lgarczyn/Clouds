using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Linq;

public class TogglePlayMode : MonoBehaviour
{
  void SetPlaying(bool playing)
  {
    Time.timeScale = playing ? 1f : 0f;
    AudioListener.pause = !playing;
  }

  // Check just in case scene was reloaded while paused
  void Start()
  {
    SetPlaying(true);
  }

  void Update()
  {
    if (Keyboard.current.pKey.wasPressedThisFrame)
    {
      // Get the current state paused state
      bool playing = Time.timeScale != 0f;
      SetPlaying(!playing);
    }
    if (Keyboard.current.oKey.wasPressedThisFrame)
    {
      SetPlaying(true);
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
  }
}
