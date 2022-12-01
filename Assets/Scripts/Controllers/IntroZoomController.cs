using UnityEngine;

[RequireComponent(typeof(MouseFlightZoom))]
public class IntroZoomController : MonoBehaviour
{
    public float scrollInDuration = 2f;
    public float scrollInDistance = 10f;
    float scrollInTimer;

    [RequiredComponent][SerializeField] MouseFlightZoom reqMouseFlightZoom;

    void Start()
    {
        scrollInTimer = 0;
    }

    // Scroll towards the plane at game start
    void LateUpdate() {
        scrollInTimer += Time.deltaTime;

        float ratio = scrollInTimer / scrollInDuration;

        reqMouseFlightZoom.SetTemporaryZoom(Mathf.Lerp(scrollInDistance, 0, ratio));

        if (scrollInTimer >= scrollInDuration)
            enabled = false;
    }

}
