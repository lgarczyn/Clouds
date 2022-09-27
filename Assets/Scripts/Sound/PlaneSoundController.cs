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

    AudioSource[] sources = GetComponents<AudioSource>();

    float currentPitch = pitchVsSpeed.Evaluate(speed);
    float currentVolume = volume *
      volumeVsSpeed.Evaluate(speed) *
      volumeVsAltitude.Evaluate(altitude);

    sources.Do((s) => s.pitch = currentPitch);
    sources.Do((s) => s.volume = currentVolume);
  }
}
