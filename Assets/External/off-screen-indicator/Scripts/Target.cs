using UnityEngine;

namespace PixelPlay.OffScreenIndicator
{

/// <summary>
/// Attach this script to all the target game objects in the scene.
/// </summary>
[DefaultExecutionOrder(0)]
public class Target : MonoBehaviour
{
    [Tooltip("The info to scale and display on-screen boxes")]
    [SerializeField] private IndicatorInfo box = new IndicatorInfo(Color.red);

    [Tooltip("The info to scale and display off-screen arrows")]
    [SerializeField] private IndicatorInfo arrow = new IndicatorInfo(Color.red);
    
    [Tooltip("The info to scale and display text")]
    [SerializeField] private IndicatorInfo text = new IndicatorInfo(Color.black);

    [Tooltip("Select if arrows should be replaced by a centered reticule")]
    [SerializeField] private bool useCenteredIndicator = false;

    /// <summary>
    /// Please do not assign its value yourself without understanding its use.
    /// A reference to the target's indicator, 
    /// its value is assigned at runtime by the offscreen indicator script.
    /// </summary>
    [HideInInspector] public Indicator indicator;

    public IndicatorInfo BoxIndicator { get { return box; } }
    public IndicatorInfo ArrowIndicator { get { return arrow; } }
    public IndicatorInfo TextIndicator { get { return text; } }

    public bool UseCenteredIndicator { get { return useCenteredIndicator; } }


    /// <summary>
    /// On enable add this target object to the targets list.
    /// </summary>
    private void OnEnable()
    {
        if(OffScreenIndicator.TargetStateChanged != null)
        {
            OffScreenIndicator.TargetStateChanged.Invoke(this, true);
        }
    }

    /// <summary>
    /// On disable remove this target object from the targets list.
    /// </summary>
    private void OnDisable()
    {
        if(OffScreenIndicator.TargetStateChanged != null)
        {
            OffScreenIndicator.TargetStateChanged.Invoke(this, false);
        }
    }

    /// <summary>
    /// Gets the distance between the camera and the target.
    /// </summary>
    /// <param name="cameraPosition">Camera position</param>
    /// <returns></returns>
    public float GetDistanceFromCamera(Vector3 cameraPosition)
    {
        float distanceFromCamera = Vector3.Distance(cameraPosition, transform.position);
        return distanceFromCamera;
    }
}

}
