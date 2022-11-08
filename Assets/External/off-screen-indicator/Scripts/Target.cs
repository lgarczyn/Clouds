using UnityEngine;

namespace PixelPlay.OffScreenIndicator
{

/// <summary>
/// Attach this script to all the target game objects in the scene.
/// </summary>
[DefaultExecutionOrder(0)]
public class Target : MonoBehaviour
{
    [Tooltip("Change this color to change the indicators color for this target")]
    [SerializeField] private Color targetColor = Color.red;

    [Tooltip("Select if box indicator is required for this target")]
    [SerializeField] private bool needBoxIndicator = true;

    [Tooltip("Select if arrow indicator is required for this target")]
    [SerializeField] private bool needArrowIndicator = true;

    [Tooltip("Select if arrows should be replaced by a centered reticule")]
    [SerializeField] private bool useCenteredIndicator = false;

    [Tooltip("Select if distance text is required for this target")]
    [SerializeField] private bool needDistanceText = true;
    
    [Tooltip("The maximum distance, for hiding and/or scaling")]
    [SerializeField] private float maxDistance = 100f;
    
    [Tooltip("The minimum distance, for hiding and/or scaling")]
    [SerializeField] private float minDistance = 1f;

    [Tooltip("Select if arrow should be hidden outside of range")]
    [SerializeField] private bool hideArrowOutsideRange = false;

    [Tooltip("Select if box should be hidden outside of range")]
    [SerializeField] private bool hideBoxOutsideRange = false;

    [Tooltip("Select if text should be hidden outside of range")]
    [SerializeField] private bool hideTextOutsideRange = false;

    [Tooltip("Select if indicator should scale with distance (log scale)")]
    [SerializeField] private bool scaleIndicatorWithDistance = true;

    [Tooltip("Select if text should scale with distance (log scale)")]
    [SerializeField] private bool scaleTextWithDistance = true;

    [Tooltip("The scale at the closest distance")]
    [SerializeField] private float closeScale = 1f;

    [Tooltip("The scale at the maximum distance")]
    [SerializeField] private float farScale = 0f;

    /// <summary>
    /// Please do not assign its value yourself without understanding its use.
    /// A reference to the target's indicator, 
    /// its value is assigned at runtime by the offscreen indicator script.
    /// </summary>
    [HideInInspector] public Indicator indicator;

    /// <summary>
    /// Gets the color for the target indicator.
    /// </summary>
    public Color TargetColor
    {
        get
        {
            return targetColor;
        }
    }

    /// <summary>
    /// Gets if box indicator is required for the target.
    /// </summary>
    public bool NeedBoxIndicator
    {
        get
        {
            return needBoxIndicator;
        }
    }

    /// <summary>
    /// Gets if arrow indicator is required for the target.
    /// </summary>
    public bool NeedArrowIndicator
    {
        get
        {
            return needArrowIndicator;
        }
    }

    /// <summary>
    /// Gets if arrows should be replaced by a centered reticule
    /// </summary>
    public bool UseCenteredIndicator
    {
        get
        {
            return useCenteredIndicator;
        }
    }

    /// <summary>
    /// Gets if the distance text is required for the target.
    /// </summary>
    public bool NeedDistanceText
    {
        get
        {
            return needDistanceText;
        }
    }

    /// <summary>
    /// Select if arrow should be hidden outside of range
    ///
    public bool HideArrowOutsideRange
    {
        get
        {
            return hideArrowOutsideRange;
        }
    }

    /// <summary>
    /// Select if box should be hidden outside of range
    ///
    public bool HideBoxOutsideRange
    {
        get
        {
            return hideBoxOutsideRange;
        }
    }

    /// <summary>
    /// Select if text should be hidden outside of range
    ///
    public bool HideTextOutsideRange
    {
        get
        {
            return hideTextOutsideRange;
        }
    }

    /// <summary>
    /// The maximum distance, for hiding and/or scaling
    /// </summary>
    public float MaxDistance
    {
        get
        {
            return maxDistance;
        }
    }

    /// <summary>
    /// The minimum distance, for hiding and/or scaling
    /// </summary>
    public float MinDistance
    {
        get
        {
            return minDistance;
        }
    }

    /// <summary>
    /// Select if indicator should scale with distance (log scale)
    /// </summary>
    public bool ScaleIndicatorWithDistance
    {
        get
        {
            return scaleIndicatorWithDistance;
        }
    }

    /// <summary>
    /// Select if text should scale with distance (log scale)
    /// </summary>
    public bool ScaleTextWithDistance
    {
        get
        {
            return scaleTextWithDistance;
        }
    }

    /// <summary>
    /// The scale at the maximum distance
    /// </summary>
    public float FarScale
    {
        get
        {
            return farScale;
        }
    }

    /// <summary>
    /// The scale at the maximum distance
    /// </summary>
    public float CloseScale
    {
        get
        {
            return closeScale;
        }
    }

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
