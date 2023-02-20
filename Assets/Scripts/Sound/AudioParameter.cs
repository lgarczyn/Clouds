using FMOD.Studio;
using UnityEngine;

namespace Sound
{
  [RequireComponent(typeof(AudioEmitter))]
  public class AudioParameter : MonoBehaviour
  {
    [SerializeField][HideInInspector] ParameterId parameterId;
    [SerializeField][HideInInspector] AudioEmitter emitter;

    public ParameterId Id => parameterId;
    public AudioEmitter AudioEmitter => emitter;

    public PARAMETER_DESCRIPTION ParameterDescription
    {
      get
      {
        AudioEmitter.EventDescription.getParameterDescriptionByID(Id, out PARAMETER_DESCRIPTION desc);
        return desc;
      }
    }

    void OnValidate()
    {
      emitter = GetComponent<AudioEmitter>();
    }

    public void SetParameter(float value)
    {
      emitter.SetParameter(parameterId, value);
    }

    public void PlayWithParameterValue(float value)
    {
      SetParameter(value);
      GetComponent<AudioEmitter>().Play();
    }
  }
}
