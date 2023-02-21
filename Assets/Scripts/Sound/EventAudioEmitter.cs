using UnityAtoms;
using UnityEngine;

namespace Sound
{
  public class EventAudioEmitter : AudioEmitter
  {
    [SerializeField] AtomEventBase emitterEvent;
    void OnEnable() => emitterEvent.Register(Play);
    void OnDisable() => emitterEvent.Unregister(Play);
  }
}