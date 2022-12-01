using UnityEngine;
using UnityEngine.InputSystem;

public class MouseFlightZoom : MonoBehaviour
{
  public float scrollPower = 1f;
  public float maxScroll = 4f;
  public float minScroll = -0.8f;
  Vector3 _zoomOrigin;
  float _currentZoom = 0;
  float _temporaryZoom = 0f;

  [SerializeField][RequiredComponent] MouseFlightController reqMouseFlightController;

  private void Start()
  {
    _zoomOrigin = reqMouseFlightController.offset;
  }

  public void OnZoom(InputAction.CallbackContext context)
  {
    _currentZoom -= context.ReadValue<float>() * scrollPower / 10;
    UpdateZoom();
  }

  void UpdateZoom()
  {
    _currentZoom = Mathf.Clamp(_currentZoom, minScroll, maxScroll);
    Vector3 zoomOffset = _zoomOrigin * Mathf.Pow(2, _currentZoom + _temporaryZoom);
    reqMouseFlightController.offset = zoomOffset;
  }

  public void SetTemporaryZoom(float value)
  {
    _temporaryZoom = value;
    UpdateZoom();
  }
}
