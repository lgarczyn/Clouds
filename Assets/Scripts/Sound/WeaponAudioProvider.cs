using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Range
{
  public float minRPS;
  public int shotCount;
  public AudioClip clip;
}

public class WeaponAudioProvider : MonoBehaviour
{
  public Range[] ranges;

  public Range GetClip(float rps)
  {
    for (int i = 0; i < ranges.Length; i++)
    {
      if (rps > ranges[i].minRPS)
      {
        return ranges[i];
      }
    }
    return ranges[ranges.Length - 1];
  }
}
