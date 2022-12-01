using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleTutorial : MonoBehaviour
{
  public GameObject target;

  public void OnToggleTutorial(InputAction.CallbackContext context)
  {
    target.SetActive(context.ReadValueAsButton());
  }
}
