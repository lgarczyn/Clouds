using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Atoms
{
  // TODO actual dropdown for propertyName, or better solution
  [RequireComponent(typeof(FloatMixer))]
  public class VariableFloatMixerTarget : MonoBehaviour
  {
    [SerializeField] FloatReference mixed;

    void SetValue(float value)
    {
      mixed.Value = value;
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