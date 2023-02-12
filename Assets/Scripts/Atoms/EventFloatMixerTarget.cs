using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Atoms
{
  // TODO actual dropdown for propertyName, or better solution
  [RequireComponent(typeof(FloatMixer))]
  public class EventFloatMixerTarget : MonoBehaviour
  {
    [SerializeField] FloatEventReference onMix;

    void OnEnable()
    {
      GetComponent<FloatMixer>().onMixFloat += onMix.Event.Raise;
      if (GetComponent<FloatMixer>().TryGetLastOutput(out float last)) onMix.Event.Raise(last);
    }
    void OnDisable()
    {
      GetComponent<FloatMixer>().onMixFloat -= onMix.Event.Raise;
    }
  }
}