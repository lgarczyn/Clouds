using UnityEngine;

public class TrailController : MonoBehaviour
{
  [PercentCurve(4)]
  public AnimationCurve widthVsValue;
  public float widthMultiplier = 1f;

  public bool forceMax = false;

  [SerializeField][RequiredComponent] TrailRenderer reqTrailRenderer;

  public void Start()
  {
    reqTrailRenderer.widthMultiplier = 0f;//forceMax ? widthMultiplier : 0f;
  }

  public void SetValue(float percent)
  {
    float clampedPercent = Mathf.Clamp(percent, 0, 100);

    float width = widthVsValue.Evaluate(clampedPercent) * widthMultiplier;

    if (forceMax) width = widthMultiplier;

    reqTrailRenderer.widthMultiplier = width;
  }

  public void SetBaseValue(float width)
  {
    widthMultiplier = width;
  }

  public void SetEmitting(bool value)
  {
    reqTrailRenderer.emitting = value;
  }

  public void Clear()
  {
    reqTrailRenderer.Clear();
  }
}
