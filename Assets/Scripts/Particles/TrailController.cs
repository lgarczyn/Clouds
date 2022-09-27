using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class TrailController : MonoBehaviour
{
  [PercentCurve(4)]
  public AnimationCurve widthVsValue;
  public float widthMultiplier = 1f;

  public bool forceMax = false;

  TrailRenderer trail
  {
    get
    {
      return GetComponent<TrailRenderer>();
    }
  }

  public void Start()
  {
    trail.widthMultiplier = 0f;//forceMax ? widthMultiplier : 0f;
  }

  public void SetValue(float percent)
  {
    float clampedPercent = Mathf.Clamp(percent, 0, 100);

    float width = widthVsValue.Evaluate(clampedPercent) * widthMultiplier;

    if (forceMax) width = widthMultiplier;

    trail.widthMultiplier = width;
  }

  public void SetEmitting(bool value)
  {
    trail.emitting = value;
  }

  public void Clear()
  {
    trail.Clear();
  }
}
