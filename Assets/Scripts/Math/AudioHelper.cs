using System;
using UnityEngine;

namespace UnityEngine
{
  /// <summary>
  /// Vector helpers.
  /// </summary>
  public static class AudioHelper
  {
    public static double GetLengthAsDouble(this AudioClip clip)
    {
      return clip.length;
      //return (double)clip.samples / clip.frequency;
    }

    public static double GetTimeAsDouble(this AudioSource source)
    {
      if (source.clip == null) return 0;
      return (double)source.timeSamples / source.clip.frequency;
    }
  }
}