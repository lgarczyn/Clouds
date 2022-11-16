using UnityEngine;

public class MouseFlightZoom : MonoBehaviour
{
  public float scrollPower = 1f;
  public float maxScroll = 4f;
  public float minScroll = -0.8f;
  float _padScroll;
  Vector3 _zoomOrigin;
  float _currentZoom = 0;
  float _temporaryZoom = 0f;

  [SerializeField][RequiredComponent] MouseFlightController reqMouseFlightController;

  private void Start()
  {
    _zoomOrigin = reqMouseFlightController.offset;
  }

  private void Update()
  {
    _currentZoom -= MouseScroll * scrollPower / 10;
    _currentZoom = Mathf.Clamp(_currentZoom, minScroll, maxScroll);
    Vector3 zoomOffset = _zoomOrigin * Mathf.Pow(2, _currentZoom + _temporaryZoom);
    reqMouseFlightController.offset = zoomOffset;
  }

  float MouseScroll
  {
    get
    {
      float mouseScroll = Input.GetAxis("Mouse ScrollWheel");

      float scroll = mouseScroll == 0 ? _padScroll : mouseScroll;

      return Mathf.Clamp(scroll, -1, 1);
    }
  }

  public void SetTemporaryZoom(float value)
  {
    _temporaryZoom = value;
  }

  //Get TrackPad Scroll
  void OnGUI()
  {
    if (Event.current.type == EventType.ScrollWheel)
      _padScroll = Event.current.delta.y;
    else
      _padScroll = 0;
  }
}
