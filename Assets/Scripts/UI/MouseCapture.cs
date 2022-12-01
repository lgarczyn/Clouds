using UnityEngine;
using UnityEngine.InputSystem;

public class MouseCapture : MonoBehaviour
{
  bool isCaptured {
    get => Cursor.lockState != CursorLockMode.None;
    set {
      Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
      Cursor.visible = !value;
    }
  }

  [SerializeField] bool shouldCapture = true;

  public bool ShouldCapture {
    set { shouldCapture = true; isCaptured = true; }
  }

  public void OnClickIn() {
    
    if (shouldCapture) isCaptured = true;
  }

  public void OnEscape() {
    isCaptured = false;
  }
}
