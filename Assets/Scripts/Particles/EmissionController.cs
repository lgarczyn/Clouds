using UnityEngine;

public class EmissionController : MonoBehaviour
{
    public AnimationCurve intensityCurve;
    public float intensity = 300;

    public bool testEmitter = false;

    [SerializeField][RequiredComponent] ParticleSystem reqParticleSystem;

    public void Start() {
        // Clean scene values used for editing
        var main = reqParticleSystem.main;
        main.gravityModifier = 0f;

        var emission = reqParticleSystem.emission;        
        emission.rateOverTime = 0f;
    }

    public void SetValue(float percent)
    {
        float clampedIndex = Mathf.Clamp01(percent / 100);

        // Force max emitting
        if (testEmitter) clampedIndex = 0;

        float value = intensityCurve.Evaluate(clampedIndex);

        float scaledValue = Mathf.Max(value, 0) * intensity;

        var emission = reqParticleSystem.emission;

        emission.rateOverTime = scaledValue;
    }
}
