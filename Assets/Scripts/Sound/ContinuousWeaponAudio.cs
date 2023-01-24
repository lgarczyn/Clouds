using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(AudioSource))]
public class ContinuousWeaponAudio : MonoBehaviour, IWeaponAudio
{
  bool isPlaying;
  [SerializeField] ContinuousWeaponAudioProvider provider;

  [SerializeField] AudioSource startAudioSource;
  [SerializeField] AudioSource repeatAudioSource;
  [SerializeField] AudioSource endAudioSource;

  void Start()
  {
    startAudioSource.loop = false;
    startAudioSource.clip = provider.start;
    repeatAudioSource.loop = true;
    repeatAudioSource.clip = provider.repeat;
    endAudioSource.loop = false;
    endAudioSource.clip = provider.end;
  }


  public void StartFire(float rps)
  {
    if (isPlaying) return;

    isPlaying = true;

    double startTime = AudioSettings.dspTime + 0.05;

    startAudioSource.Stop();
    repeatAudioSource.Stop();
    endAudioSource.Stop();

    startAudioSource.PlayScheduled(startTime);
    repeatAudioSource.PlayScheduled(startTime + provider.start.GetLengthAsDouble());

    // reqA
    // reqAudioSource.PlayDelayed(provider.start);
    // reqAudioSource.PlayDelayed(provider.start.length);
  }

  public void EndFire()
  {
    isPlaying = false;
    if (startAudioSource.isPlaying ) {
      double currentTime = startAudioSource.GetTimeAsDouble();
      double expectedTime = startAudioSource.clip.GetLengthAsDouble();
      double remaining = expectedTime-currentTime;
      double endTimestamp = AudioSettings.dspTime + remaining;
      startAudioSource.SetScheduledEndTime(endTimestamp);
      endAudioSource.PlayScheduled(endTimestamp);
    } else {
      startAudioSource.Stop();
    }
    if (repeatAudioSource.isPlaying) {
      double currentTime = repeatAudioSource.GetTimeAsDouble();
      double expectedTime = repeatAudioSource.clip.GetLengthAsDouble();
      double remaining = expectedTime-currentTime;
      double endTimestamp = AudioSettings.dspTime + remaining;
      repeatAudioSource.SetScheduledEndTime(endTimestamp);
      endAudioSource.PlayScheduled(endTimestamp);
    } else {
      repeatAudioSource.Stop();
    }
  }
}
