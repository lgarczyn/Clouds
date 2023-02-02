using UnityEngine;
using ScriptableObjectArchitecture;

[CreateAssetMenu(menuName = "Project/SmoothDampFloatProcessor")]
public class SmoothDampFloatProcessor : ScriptableObject {

    [SerializeField] FloatGameEvent input;
    [SerializeField] FloatVariable output;
    [SerializeField] float timeToAdjust;
    [SerializeField] float maxVelocity;

    float timeOfLastCall = 0f;
    float currentValue = 0f;
    float velocity = 0f;

    void OnEnable()
    {
        input.AddListener(OnChange);
    }

    void OnChange(float value) {
        if (timeOfLastCall == 0) {
            currentValue = value;
            timeOfLastCall = Time.time;
            velocity = 0f;
            output.SetValue(value);
            return;
        }

        currentValue = Mathf.SmoothDamp(
            currentValue,
            value,
            ref velocity,
            Time.time - timeOfLastCall,
            maxVelocity);

        timeOfLastCall = Time.time;

        output.SetValue(currentValue);
    }

    void OnDisable()
    {
        input.RemoveListener(OnChange);
    }

}