using UnityEngine;

public class ResourceBar : MonoBehaviour
{
  [SerializeField] RectTransform indicator;
  [SerializeField] RectTransform bar;

  [Range(0.1f, 0.99f)]
  [SerializeField] float floatingAverageReactivity = 0.8f;

  [SerializeField] bool isVertical = false;

  private float floatingAverage;
  private float currentValue;

  public void SetValue(float value)
  {
    currentValue = Mathf.Clamp01(value / 100);
    bar.anchorMax = isVertical ? new Vector2(1f, currentValue) : new Vector2(currentValue, 1f);
  }

  void FixedUpdate()
  {
    floatingAverage = Mathf.Lerp(floatingAverage, currentValue, floatingAverageReactivity);

    indicator.localRotation = isVertical ? Quaternion.identity : Quaternion.AngleAxis(-90f, Vector3.forward);

    if (currentValue > floatingAverage + 0.00001f)
      indicator.localScale = new Vector3(1f, 1f, 1f);
    else if (currentValue < floatingAverage - 0.00001f)
      indicator.localScale = new Vector3(1f, -1f, 1f);
    else
      indicator.localScale = new Vector3(0f, 0f, 1f);
  }
}