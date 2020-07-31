using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneStartController : MonoBehaviour
{
    public MouseFlightZoom zoomController;
    public float scrollInDuration = 2f;
    public float scrollInDistance = 10f;
    float scrollInTimer;

    void Start()
    {
        scrollInTimer = 0;
    }

    // Scroll towards the plane at game start
    void LateUpdate() {
        scrollInTimer += Time.deltaTime;

        float ratio = scrollInTimer / scrollInDuration;

        zoomController.SetTemporaryZoom(Mathf.Lerp(scrollInDistance, 0, ratio));

        if (scrollInTimer >= scrollInDuration)
            enabled = false;
    }

}
