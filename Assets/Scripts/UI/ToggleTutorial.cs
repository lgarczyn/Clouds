using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleTutorial : MonoBehaviour
{
  public GameObject target;

  public void OnToggleTutorial(InputAction.CallbackContext context)
  {
    if (context.performed) target.SetActive(!target.activeSelf);
  }
}
