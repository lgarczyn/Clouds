using UnityEngine;
using UnityEngine.SceneManagement;

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
    if (Input.GetKeyDown(KeyCode.P))
    {
      // Get the current state paused state
      bool playing = Time.timeScale != 0f;
      SetPlaying(!playing);
    }
    if (Input.GetKeyDown(KeyCode.O))
    {
      SetPlaying(true);
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
  }
}
