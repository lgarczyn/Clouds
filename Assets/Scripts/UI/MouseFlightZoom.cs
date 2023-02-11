using UnityEngine;
using UnityEngine.InputSystem;

public class MouseFlightZoom : MonoBehaviour
{
  [SerializeField] float scrollPower = 1f;
  [SerializeField] float maxScroll = 4f;
  [SerializeField] float minScroll = -0.8f;
  [SerializeField] float currentZoom = 0;
  float _targetScale = 1f;
  float _temporaryZoom = 0f;

  [SerializeField][RequiredComponent] MouseFlightController reqMouseFlightController;

  public void OnZoom(InputAction.CallbackContext context)
  {
    currentZoom -= context.ReadValue<float>() * scrollPower / 10;
    UpdateZoom();
  }

  void UpdateZoom()
  {
    currentZoom = Mathf.Clamp(currentZoom, minScroll, maxScroll);
    float zoom = Mathf.Pow(2, currentZoom + _temporaryZoom);
    zoom *= _targetScale;
    reqMouseFlightController.offsetDistanceMultiplier = zoom;
  }

  public void SetTemporaryZoom(float value)
  {
    _temporaryZoom = value;
    UpdateZoom();
  }

  public void SetTargetScale(float value)
  {
    _targetScale = value;
    UpdateZoom();
  }
}
