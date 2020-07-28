using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceBar : MonoBehaviour
{
    public void SetValue(float value)
    {
        float ratio = Mathf.Clamp01(value / 100);
        GetComponent<RectTransform>().localScale = new Vector2(ratio, 1f);
    }
}