using UnityEngine;

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

  [SerializeField][RequiredComponent] AudioSource reqAudioSource;

  public void PlayDamageSound()
  {
    if (reqAudioSource.isPlaying) return;

    reqAudioSource.pitch = Random.Range(minPitch, maxPitch);
    reqAudioSource.volume = Random.Range(minVolume, maxVolume);
    reqAudioSource.Play();
  }
}
