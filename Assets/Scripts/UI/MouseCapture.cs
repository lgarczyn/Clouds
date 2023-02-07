using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MouseCapture : MonoBehaviour
{
  public UnityEvent<bool> onCaptureChange;

  bool lastValue = false;

  void Update()
  {
    CheckUpdated();
  }

  void CheckUpdated()
  {
    if (lastValue != IsCaptured)
    {
      lastValue = IsCaptured;
      onCaptureChange.Invoke(IsCaptured);
    }
  }

  public bool IsCaptured
  {
    get => Cursor.lockState != CursorLockMode.None;
    set
    {
      Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
      Cursor.visible = !value;
      CheckUpdated();
    }
  }
}
