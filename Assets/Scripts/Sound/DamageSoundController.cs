using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DamageSoundController : MonoBehaviour
{
  [Range(0.2f, 4)]
  [SerializeField] float minPitch;
  [Range(0.2f, 4)]
  [SerializeField] float maxPitch;
  [Range(0.2f, 4)]
  [SerializeField] float minVolume;
  [Range(0.2f, 4)]
  [SerializeField] float maxVolume;

  public void PlayDamageSound()
  {
    AudioSource source = GetComponent<AudioSource>();

    if (source.isPlaying) return;

    source.pitch = Random.Range(minPitch, maxPitch);
    source.volume = Random.Range(minVolume, maxVolume);
    source.Play();
  }
}
