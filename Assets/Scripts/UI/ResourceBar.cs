using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceBar : MonoBehaviour
{
    public RectTransform indicator;
    private float lastValue = 0;

    public void SetValue(float value)
    {
        float ratio = Mathf.Clamp01(value / 100);
        RectTransform bar = GetComponent<RectTransform>();

        bar.sizeDelta = new Vector2(bar.sizeDelta.x, Mathf.Round(100 * ratio));

        if (ratio > lastValue)
            indicator.localScale = new Vector3(1f, 1f, 1f);
        else if (ratio < lastValue)
            indicator.localScale = new Vector3(1f, -1f, 1f);
        else
            indicator.localScale = new Vector3(0f, 0f, 1f);

        lastValue = ratio;
    }
}