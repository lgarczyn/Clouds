using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Sound
{
  [RequireComponent(typeof(StudioEventEmitter))]
  public class AudioEmitter : MonoBehaviour
  {
    [SerializeField] [RequiredComponent] StudioEventEmitter emitter;
    [SerializeField] Rigidbody rigidbodyOverride;
    [SerializeField] bool playOnStart;

    public void Play() => emitter.Play();
    public void Stop() => emitter.Stop();
    public EventDescription EventDescription => RuntimeManager.GetEventDescription(EventReference);
    public EventReference EventReference => emitter.EventReference;

    public void SetParameter(ParameterId id, float value, bool ignoreSeek = false)
    {
      emitter.SetParameter(id, value, ignoreSeek);
    }

    void Start()
    {
      if (playOnStart) Play();
    }

    public void SetPlaying(bool value)
    {
      if (value)
      {
        emitter.Play();
        if (rigidbodyOverride && emitter.EventInstance.isValid())
        {
          var attributes = RuntimeUtils.To3DAttributes(rigidbodyOverride.gameObject, rigidbodyOverride);
          emitter.EventInstance.set3DAttributes(attributes);
        }

        foreach (VariableAudioParameter param in GetComponents<VariableAudioParameter>())
        {
          param.UpdateParameter();
        }
      }
      else
      {
        emitter.Stop();
      }
    }
  }
}