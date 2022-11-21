using UnityEngine;
using UnityEngine.InputSystem;

public class MouseCapture : MonoBehaviour
{
  void Update()
  {
    // if (Application.platform == RuntimePlatform.LinuxEditor || Application.isEditor == false)
    // {
    if (Mouse.current.leftButton.wasPressedThisFrame)
      Cursor.lockState = CursorLockMode.Locked;
    if (Keyboard.current.escapeKey.wasPressedThisFrame)
      Cursor.lockState = CursorLockMode.None;
    // }
  }
}
