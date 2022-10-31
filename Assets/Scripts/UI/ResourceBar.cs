using UnityEngine;

public class ResourceBar : MonoBehaviour
{
  public RectTransform indicator;
  public RectTransform bar;

  [Range(0.1f, 0.99f)]
  public float floatingAverageReactivity = 0.8f;

  private float floatingAverage;
  private float currentValue;

  public void SetValue(float value)
  {
    currentValue = Mathf.Clamp01(value / 100);
    bar.anchorMax = new Vector2(1, currentValue);
  }

  void FixedUpdate()
  {
    floatingAverage = Mathf.Lerp(floatingAverage, currentValue, floatingAverageReactivity);

    if (currentValue > floatingAverage + 0.00001f)
      indicator.localScale = new Vector3(1f, 1f, 1f);
    else if (currentValue < floatingAverage - 0.00001f)
      indicator.localScale = new Vector3(1f, -1f, 1f);
    else
      indicator.localScale = new Vector3(0f, 0f, 1f);
  }
}