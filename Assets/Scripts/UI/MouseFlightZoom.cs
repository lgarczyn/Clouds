using UnityEngine;
using UnityEngine.InputSystem;

public class MouseFlightZoom : MonoBehaviour
{
  [SerializeField] float scrollPower = 1f;
  [SerializeField] float maxScroll = 4f;
  [SerializeField] float minScroll = -0.8f;
  [SerializeField] float currentZoom = 0;
  [SerializeField] float targetScale = 1f;
  float temporaryZoom = 0f;

  [SerializeField][RequiredComponent] MouseFlightController reqMouseFlightController;

  public void OnZoom(InputAction.CallbackContext context)
  {
    currentZoom -= context.ReadValue<float>() * scrollPower / 10;
    UpdateZoom();
  }

  void UpdateZoom()
  {
    currentZoom = Mathf.Clamp(currentZoom, minScroll, maxScroll);
    float zoom = Mathf.Pow(2, currentZoom + temporaryZoom);
    zoom *= targetScale;
    reqMouseFlightController.offsetDistanceMultiplier = zoom;
  }

  public void SetTemporaryZoom(float value)
  {
    temporaryZoom = value;
    UpdateZoom();
  }
}
