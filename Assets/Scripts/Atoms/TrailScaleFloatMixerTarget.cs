using UnityEngine;

namespace Atoms
{
  // TODO actual dropdown for propertyName, or better solution
  [RequireComponent(typeof(FloatMixer))]
  [RequireComponent(typeof(TrailRenderer))]
  public class TrailScaleFloatMixerTarget : MonoBehaviour
  {
    void SetValue(float value)
    {
      GetComponent<TrailRenderer>().widthMultiplier = value;
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