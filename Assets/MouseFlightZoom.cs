using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MFlight.MouseFlightController))]
public class MouseFlightZoom : MonoBehaviour
{
    public float scrollPower = 1f;

    float _padScroll;
    private void Update() {
        if (MouseScroll != 0)
        {
            Debug.Log(MouseScroll);
            GetComponent<MFlight.MouseFlightController>().offset *= 1 - MouseScroll * scrollPower ;
        }
    }

    float MouseScroll
    {
        get
        {
            float mouseScroll = Input.GetAxis("Mouse ScrollWheel");

            if (mouseScroll != 0)
                return mouseScroll;
            else
                return _padScroll;
        }
    }

    //Get TrackPad Scroll
    void OnGUI()
    {
        if (Event.current.type == EventType.ScrollWheel)
            _padScroll = -Event.current.delta.y / 100;
        else
            _padScroll = 0;
    }
}
