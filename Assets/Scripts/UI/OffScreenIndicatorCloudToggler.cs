using UnityEngine;
using PixelPlay.OffScreenIndicator;
using UnityAtoms.BaseAtoms;

[RequireComponent(typeof(OffScreenIndicator))]
[RequireComponent(typeof(ResourceBarManagerBridge))]
public class OffScreenIndicatorCloudToggler : MonoBehaviour
{
    [SerializeField] FloatReference playerDensity;
    [SerializeField][RequiredComponent] OffScreenIndicator reqOffscreenIndicator;

    public float maxDensity = 1f;
    public float maxTimeWithoutVisibility = 1f;

    float timeWithoutDensity = 0f;

    void Update()
    {
        bool countdownStarted = playerDensity > maxDensity;
        if (countdownStarted) timeWithoutDensity += Time.deltaTime;
        else timeWithoutDensity = 0f;

        reqOffscreenIndicator.IsVisible = !(timeWithoutDensity > maxTimeWithoutVisibility);
    }
}
