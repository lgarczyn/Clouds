using UnityEngine;

namespace Atoms
{
  // TODO actual dropdown for propertyName, or better solution
  [RequireComponent(typeof(FloatMixer))]
  public class MaterialFloatMixerTarget : MonoBehaviour
  {
    [SerializeField] Material material;
    [SerializeField] string propertyName;

    float _oldValue;

    void SetValue(float value)
    {
      material.SetFloat(propertyName, value);
    }

    float GetValue()
    {
      return material.GetFloat(propertyName);
    }

    void OnEnable()
    {
      GetComponent<FloatMixer>().onMixFloat += SetValue;
      _oldValue = GetValue();
      if (GetComponent<FloatMixer>().TryGetLastOutput(out float last)) SetValue(last);
    }

    void OnDisable()
    {
      GetComponent<FloatMixer>().onMixFloat -= SetValue;
      SetValue(_oldValue);
    }
  }
}