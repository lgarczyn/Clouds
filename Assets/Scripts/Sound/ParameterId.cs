using System;
using FMOD.Studio;

namespace Sound
{
  /// <summary>
  /// A Serializable replacement to PARAMETER_ID
  /// Allows the storage of events ids instead of their name, for easier renaming of events
  /// </summary>
  [Serializable]
  public struct ParameterId : IEquatable<ParameterId>
  {
    public bool Equals(ParameterId other)
    {
      return data == other.data;
    }

    public override bool Equals(object obj)
    {
      return obj is ParameterId other && Equals(other);
    }

    public override int GetHashCode()
    {
      return data.GetHashCode();
    }

    public static bool operator ==(ParameterId left, ParameterId right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(ParameterId left, ParameterId right)
    {
      return !left.Equals(right);
    }

    public ulong data;
    static ulong Combine(uint a, uint b) {
      return (ulong)b << 0x20 | a;
    }
    static void Decombine(ulong c, out uint a, out uint b) {
      a = (uint)(c & 0xFFFFFFFFUL);
      b = (uint)(c >> 0x20);
    }

    public static implicit operator ParameterId(FMOD.Studio.PARAMETER_ID source)
    {
      ulong data = Combine(source.data1, source.data2);
      return new ParameterId { data = data };
    }

    public static implicit operator FMOD.Studio.PARAMETER_ID(ParameterId source)
    {
      PARAMETER_ID result;
      Decombine(source.data, out result.data1, out result.data2);
      return result;
    }

    public bool Equals(FMOD.Studio.PARAMETER_ID other)
    {
      return data == Combine(other.data1, other.data2);
    }
  }
}