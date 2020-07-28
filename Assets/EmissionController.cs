using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionController : MonoBehaviour
{
    public AnimationCurve intensityCurve;
    public float intensity = 300;

    public void SetValue(float percent)
    {
        float clampedIndex = Mathf.Clamp01(percent / 100);

        float value = intensityCurve.Evaluate(clampedIndex);

        float scaledValue = Mathf.Max(value, 0) * intensity;

        var emission = GetComponent<ParticleSystem>().emission;

        emission.rateOverTime = scaledValue;
    }
}
