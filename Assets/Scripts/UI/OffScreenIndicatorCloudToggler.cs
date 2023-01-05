using UnityEngine;
using PixelPlay.OffScreenIndicator;

[RequireComponent(typeof(OffScreenIndicator))]
[RequireComponent(typeof(ResourceBarManagerBridge))]
public class OffScreenIndicatorCloudToggler : MonoBehaviour
{
    [SerializeField][RequiredComponent] OffScreenIndicator reqOffscreenIndicator;
    [SerializeField][RequiredComponent] ResourceCalculatorBridge reqResourceCalculatorBridge;

    public float maxDensity = 1f;
    public float maxTimeWithoutVisibility = 1f;
    
    
    float timeWithoutDensity = 0f;

    void Update()
    {
        bool indicatorsVisible = reqResourceCalculatorBridge.instance.GetDensity() > maxDensity;
        if (indicatorsVisible) timeWithoutDensity += Time.deltaTime;
        else timeWithoutDensity = 0f;

        reqOffscreenIndicator.IsVisible = !(timeWithoutDensity > maxTimeWithoutVisibility);
    }
}
