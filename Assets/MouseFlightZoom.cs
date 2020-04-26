using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MFlight.MouseFlightController))]
public class MouseFlightZoom : MonoBehaviour
{
    public float scrollPower = 1f;
    public float maxScroll = 4f;
    public float minScroll = -0.8f;
    float _padScroll;
    Vector3 _zoomOrigin;
    float _currentZoom = 0;

    private void Start()
    {
        _zoomOrigin = GetComponent<MFlight.MouseFlightController>().offset;
    }

    private void Update() {
        if (MouseScroll != 0)
        {
            _currentZoom -= MouseScroll * scrollPower / 10;
            _currentZoom = Mathf.Clamp(_currentZoom, minScroll, maxScroll);
            Vector3 zoomOffset = _zoomOrigin * Mathf.Pow(2, _currentZoom);
            GetComponent<MFlight.MouseFlightController>().offset = zoomOffset;
        }
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

    //Get TrackPad Scroll
    void OnGUI()
    {
        if (Event.current.type == EventType.ScrollWheel)
            _padScroll = Event.current.delta.y;
        else
            _padScroll = 0;
    }
}
