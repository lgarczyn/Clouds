using UnityEngine;

namespace Atoms
{
  // TODO actual dropdown for propertyName, or better solution
  [RequireComponent(typeof(FloatMixer))]
  public class TransformScaleFloatMixerTarget : MonoBehaviour
  {
    void SetValue(float value)
    {
      transform.localScale = Vector3.one * value;
    }
    void OnEnable()
    {
      GetComponent<FloatMixer>().onMixFloat += SetValue;
      if (GetComponent<FloatMixer>().TryGetLastOutput(out float last)) SetValue(last);
    }
    void OnDisable()
    {
      GetComponent<FloatMixer>().onMixFloat -= SetValue;
    }
  }
}