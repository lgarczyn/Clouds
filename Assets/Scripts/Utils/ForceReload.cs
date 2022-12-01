using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ForceReload : MonoBehaviour
{
  [SerializeField] InputActionReference action;

  public void Reload()
  {
    SceneManager.LoadScene(0);
  }

  public void OnReload(InputAction.CallbackContext context)
  {
    Reload();
  }

  void OnEnable()
  {
    action.action.performed += OnReload;
  }

  void OnDisable()
  {
    action.action.performed -= OnReload;
  }
}
