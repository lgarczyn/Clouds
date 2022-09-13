using UnityEngine;
using UnityEngine.SceneManagement;

public class TogglePlayMode : MonoBehaviour
{
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.P))
    {
      Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
    }
    if (Input.GetKeyDown(KeyCode.O))
    {
      Time.timeScale = 1f;
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
  }
}
