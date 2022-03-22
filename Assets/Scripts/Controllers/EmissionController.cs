using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionController : MonoBehaviour
{
    public AnimationCurve intensityCurve;
    public float intensity = 300;

    public bool testEmitter = false;

    public void Start() {
        var system = GetComponent<ParticleSystem>();

        // Clean scene values used for editing

        var main = system.main;
        main.gravityModifier = 0f;

        var emission = system.emission;        
        emission.rateOverTime = 0f;
    }

    public void SetValue(float percent)
    {
        float clampedIndex = Mathf.Clamp01(percent / 100);

        // Force max emitting
        if (testEmitter) clampedIndex = 0;

        float value = intensityCurve.Evaluate(clampedIndex);

        float scaledValue = Mathf.Max(value, 0) * intensity;

        var emission = GetComponent<ParticleSystem>().emission;

        emission.rateOverTime = scaledValue;
    }
}
