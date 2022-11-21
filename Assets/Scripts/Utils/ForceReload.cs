using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ForceReload : MonoBehaviour
{
  void Update()
  {
    if (Keyboard.current.oKey.wasPressedThisFrame && Keyboard.current.shiftKey.isPressed) {
      SceneManager.LoadScene(0);
    }
  }
}
