using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleUI : MonoBehaviour
{
  public CanvasGroup target;

  public void OnToggleUI(InputAction.CallbackContext context)
  {
    if (context.performed == false) return;

    target.alpha = target.alpha == 0f ? 1f : 0f;
  }
}
