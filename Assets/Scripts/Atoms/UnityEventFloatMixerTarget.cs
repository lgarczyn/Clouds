using System;
using UnityEngine;
using UnityEngine.Events;

namespace Atoms
{
  // TODO actual dropdown for propertyName, or better solution
  [RequireComponent(typeof(FloatMixer))]
  public class UnityEventFloatMixerTarget : MonoBehaviour
  {
    [SerializeField] UnityEvent<float> onMix;

    void OnValidate()
    {
      if (onMix == null) onMix = new();
      for (int i = 0; i < onMix.GetPersistentEventCount(); i++)
      {
        if (onMix.GetPersistentListenerState(i) != UnityEventCallState.RuntimeOnly)
        {
          onMix.SetPersistentListenerState(i, UnityEventCallState.RuntimeOnly);
        }
      }
    }

    void OnEnable()
    {
      GetComponent<FloatMixer>().onMixFloat += onMix.Invoke;
      if (GetComponent<FloatMixer>().TryGetLastOutput(out float last)) onMix.Invoke(last);
    }
    void OnDisable()
    {
      GetComponent<FloatMixer>().onMixFloat -= onMix.Invoke;
    }
  }
}