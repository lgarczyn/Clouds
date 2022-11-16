using UnityEngine;

public class WeaponAudio : MonoBehaviour
{
  bool isPlaying;
  [SerializeField] WeaponAudioProvider provider;

  [SerializeField][RequiredComponent] AudioSource reqAudioSource;

  public void StartFire(float rps)
  {
    //uniDebug.Log("Starting fire fps:" + rps);
    AudioSource source = reqAudioSource;

    Range range = provider.GetClip(rps);
    float pitch = rps * range.clip.length / range.shotCount;

    if (source.clip != range.clip)
    {
      source.clip = range.clip;
      if (isPlaying)
        source.Play();//might be a problem
    }

    if (pitch < 1f && range.shotCount == 1)
    {
      source.loop = false;
      source.pitch = 1f;
      isPlaying = false;
    }
    else
    {
      source.loop = true;
      source.pitch = pitch;
    }

    if (isPlaying == false)
    {
      isPlaying = true;
      source.Play();
    }
  }

  public void EndFire()
  {
    isPlaying = false;
    reqAudioSource.Stop();
  }
}
