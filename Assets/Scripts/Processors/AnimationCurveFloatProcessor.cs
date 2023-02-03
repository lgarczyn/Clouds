using UnityEngine;
using ScriptableObjectArchitecture;
using System.Linq;

[CreateAssetMenu(menuName = "Project/AnimationCurveFloatProcessor")]
public class AnimationCurveFloatProcessor : ScriptableObject {

    [SerializeField] FloatGameEvent input;
    [SerializeField] FloatVariable output;
    [SerializeField] AnimationCurve processor;

    void OnEnable()
    {
        input.AddListener(OnChange);
    }

    void OnChange(float value) {
        output.SetValue(processor.Evaluate(value));
    }

    void OnDisable()
    {
        input.RemoveListener(OnChange);
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        if (output.IsClamped)
        {
            float processorMin = processor.keys.Select(k => k.value).Min();
            float processorMax = processor.keys.Select(k => k.value).Max();
            if (output.MinClampValue > processorMin) Debug.LogWarning($"AnimationCurve min value {processorMin} is inferior to output range {output.MinClampValue}");
            if (output.MaxClampValue < processorMax) Debug.LogWarning($"AnimationCurve max value {processorMax} is superior to output range {output.MaxClampValue}");
         }
    }
    #endif
}