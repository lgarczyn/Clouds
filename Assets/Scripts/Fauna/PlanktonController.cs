using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PlanktonController : MonoBehaviour
{
    new public Transform camera;

    public float intensity = 200f;
    public float ceiling = 700f;
    public float floor = -1200f;

    public AnimationCurve relativeIntensity; 
    void LateUpdate()
    {
        transform.position = camera.position;

        float index = Mathf.InverseLerp(floor, ceiling, camera.position.y);

        float clampedIndex = Mathf.Clamp01(index);

        float value = relativeIntensity.Evaluate(clampedIndex);

        float scaledValue = value * intensity;

        var emission = GetComponent<ParticleSystem>().emission;

        emission.rateOverTime = scaledValue;
    }
}
