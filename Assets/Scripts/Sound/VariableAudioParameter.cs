using Atoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Sound
{
  public class VariableAudioParameter : AudioParameter
  {
    [SerializeField] FloatReference parameterValue;

    void OnEnable()
    {
      parameterValue.GetOrCreateEvent().Register(SetParameter);
    }

    void OnDisable()
    {
      parameterValue.GetOrCreateEvent().Unregister(SetParameter);
    }

    public void UpdateParameter()
    {
      SetParameter(parameterValue.Value);
    }
  }
}
