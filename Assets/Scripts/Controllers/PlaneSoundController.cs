using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlaneSoundController : MonoBehaviour
{
  public float volume = 1f;
  [SerializeField] AnimationCurve volumeVsSpeed;
  [SerializeField] AnimationCurve pitchVsSpeed;
  [SerializeField] AnimationCurve volumeVsAltitude;
  [SerializeField] Rigidbody plane;

  // Update is called once per frame
  void Update()
  {
    float speed = plane.velocity.magnitude;
    float altitude = plane.position.y;

    GetComponents<AudioSource>().Do((s) => s.pitch = pitchVsSpeed.Evaluate(speed));
    GetComponents<AudioSource>().Do((s) => s.volume =
      volumeVsSpeed.Evaluate(speed)
      * volumeVsAltitude.Evaluate(altitude)
      * volume);
  }
}
