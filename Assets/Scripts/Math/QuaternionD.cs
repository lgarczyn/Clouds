// Source https://gist.github.com/HelloKitty/91b7af87aac6796c3da9
// Converted to double by lgarczyn
// TODO: do own implementation to avoid license nightmare

// A custom completely managed implementation of UnityEngine.Quaternion
// Base is decompiled UnityEngine.Quaternion
// Doesn't implement methods marked Obsolete
// Does implicit coversions to and from UnityEngine.Quaternion

// Uses code from:
// https://raw.githubusercontent.com/mono/opentk/master/Source/OpenTK/Math/Quaternion.cs
// http://answers.unity3d.com/questions/467614/what-is-the-source-code-of-quaternionlookrotation.html
// http://stackoverflow.com/questions/12088610/conversion-between-euler-quaternion-like-in-unity3d-engine
// http://stackoverflow.com/questions/11492299/quaternion-to-euler-angles-algorithm-how-to-convert-to-y-up-and-between-ha

using System;
using UnityEngine.Internal;

namespace UnityEngine
{
  //
  // Summary:
  //     ///
  //     Quaternions are used to represent rotations.
  //     ///
  [Serializable]
  public struct QuaternionD : IEquatable<QuaternionD>
  {
    const double radToDeg = (180.0 / Math.PI);
    const double degToRad = (Math.PI / 180.0);

    // should probably be used in the 0 tests in LookRotation or Slerp
    // original value 1E-6F
    public const double kEpsilon = 1E-12;
    public Vector3D xyz
    {
      set
      {
        x = value.x;
        y = value.y;
        z = value.z;
      }
      get
      {
        return new Vector3D(x, y, z);
      }
    }
    /// <summary>
    ///   <para>X component of the QuaternionD. Don't modify this directly unless you know quaternionDs inside out.</para>
    /// </summary>
    public double x;
    /// <summary>
    ///   <para>Y component of the QuaternionD. Don't modify this directly unless you know quaternionDs inside out.</para>
    /// </summary>
    public double y;
    /// <summary>
    ///   <para>Z component of the QuaternionD. Don't modify this directly unless you know quaternionDs inside out.</para>
    /// </summary>
    public double z;
    /// <summary>
    ///   <para>W component of the QuaternionD. Don't modify this directly unless you know quaternionDs inside out.</para>
    /// </summary>
    public double w;

    public double this[int index]
    {
      get
      {
        switch (index)
        {
          case 0:
            return this.x;
          case 1:
            return this.y;
          case 2:
            return this.z;
          case 3:
            return this.w;
          default:
            throw new IndexOutOfRangeException("Invalid QuaternionD index!");
        }
      }
      set
      {
        switch (index)
        {
          case 0:
            this.x = value;
            break;
          case 1:
            this.y = value;
            break;
          case 2:
            this.z = value;
            break;
          case 3:
            this.w = value;
            break;
          default:
            throw new IndexOutOfRangeException("Invalid QuaternionD index!");
        }
      }
    }
    /// <summary>
    ///   <para>The identity rotation (RO).</para>
    /// </summary>
    public static QuaternionD identity
    {
      get
      {
        return new QuaternionD(0.0, 0.0, 0.0, 1.0);
      }
    }
    /// <summary>
    ///   <para>Returns the euler angle representation of the rotation.</para>
    /// </summary>
    public Vector3D eulerAngles
    {
      get
      {
        return QuaternionD.Internal_ToEulerRad(this) * radToDeg;
      }
      set
      {
        this = QuaternionD.Internal_FromEulerRad(value * degToRad);
      }
    }





    #region public double Length

    /// <summary>
    /// Gets the length (magnitude) of the quaternionD.
    /// </summary>
    /// <seealso cref="LengthSquared"/>
    public double Length
    {
      get
      {
        return System.Math.Sqrt(x * x + y * y + z * z + w * w);
      }
    }

    #endregion

    #region public double LengthSquared

    /// <summary>
    /// Gets the square of the quaternionD length (magnitude).
    /// </summary>
    public double LengthSquared
    {
      get
      {
        return x * x + y * y + z * z + w * w;
      }
    }

    #endregion

    #region public void Normalize()

    /// <summary>
    /// Scales the QuaternionD to unit length.
    /// </summary>
    public void Normalize()
    {
      double scale = 1.0 / this.Length;
      xyz *= scale;
      w *= scale;
    }

    #endregion


    #region Normalize

    /// <summary>
    /// Scale the given quaternionD to unit length
    /// </summary>
    /// <param name="q">The quaternionD to normalize</param>
    /// <returns>The normalized quaternionD</returns>
    public static QuaternionD Normalize(QuaternionD q)
    {
      QuaternionD result;
      Normalize(ref q, out result);
      return result;
    }

    /// <summary>
    /// Scale the given quaternionD to unit length
    /// </summary>
    /// <param name="q">The quaternionD to normalize</param>
    /// <param name="result">The normalized quaternionD</param>
    public static void Normalize(ref QuaternionD q, out QuaternionD result)
    {
      double scale = 1.0 / q.Length;
      result = new QuaternionD(q.xyz * scale, q.w * scale);
    }

    #endregion


    /// <summary>
    ///   <para>Constructs new QuaternionD with given x,y,z,w components.</para>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="w"></param>
    public QuaternionD(double x, double y, double z, double w)
    {
      this.x = x;
      this.y = y;
      this.z = z;
      this.w = w;
    }

    /// <summary>
    /// Construct a new QuaternionD from vector and w components
    /// </summary>
    /// <param name="v">The vector part</param>
    /// <param name="w">The w part</param>
    public QuaternionD(Vector3D v, double w)
    {
      this.x = v.x;
      this.y = v.y;
      this.z = v.z;
      this.w = w;
    }


    /// <summary>
    ///   <para>Set x, y, z and w components of an existing QuaternionD.</para>
    /// </summary>
    /// <param name="new_x"></param>
    /// <param name="new_y"></param>
    /// <param name="new_z"></param>
    /// <param name="new_w"></param>
    public void Set(double new_x, double new_y, double new_z, double new_w)
    {
      this.x = new_x;
      this.y = new_y;
      this.z = new_z;
      this.w = new_w;
    }
    /// <summary>
    ///   <para>The dot product between two rotations.</para>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static double Dot(QuaternionD a, QuaternionD b)
    {
      return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
    }

    /// <summary>
    /// Compares two quaternions and returns true if they are similar.
    /// </summary>
    /// <param name="epsilon">
    /// The smallest acceptable difference
    /// </param>
    public static bool Approximately(QuaternionD a, QuaternionD b, double epsilon = Double.Epsilon)
    {
      return MathHelper.Approximately(1, QuaternionD.Dot(a, b), epsilon);
    }

    /// <summary>
    /// Compares to another vector and returns true if they are similar.
    /// </summary>
    /// <param name="epsilon">
    /// The smallest acceptable difference
    /// </param>
    public bool Approximately(QuaternionD b, double epsilon = Double.Epsilon)
    {
      return QuaternionD.Approximately(this, b, epsilon);
    }

    /// <summary>
    ///   <para>Creates a rotation which rotates /angle/ degrees around /axis/.</para>
    /// </summary>
    /// <param name="angle"></param>
    /// <param name="axis"></param>
    public static QuaternionD AngleAxis(double angle, Vector3D axis)
    {
      return QuaternionD.INTERNAL_CALL_AngleAxis(angle, ref axis);
    }
    private static QuaternionD INTERNAL_CALL_AngleAxis(double degress, ref Vector3D axis)
    {
      if (axis.sqrMagnitude == 0.0)
        return identity;

      QuaternionD result = identity;
      var radians = degress * degToRad;
      radians *= 0.5;
      axis.Normalize();
      axis = axis * System.Math.Sin(radians);
      result.x = axis.x;
      result.y = axis.y;
      result.z = axis.z;
      result.w = System.Math.Cos(radians);

      return Normalize(result);
    }

    public void ToAngleAxis(out double angle, out Vector3D axis)
    {
      QuaternionD.Internal_ToAxisAngleRad(this, out axis, out angle);
      angle *= radToDeg;
    }
    /// <summary>
    ///   <para>Creates a rotation which rotates from /fromDirection/ to /toDirection/.</para>
    /// </summary>
    /// <param name="fromDirection"></param>
    /// <param name="toDirection"></param>
    public static QuaternionD FromToRotation(Vector3D fromDirection, Vector3D toDirection)
    {
      return RotateTowards(LookRotation(fromDirection), LookRotation(toDirection), double.MaxValue);
    }
    /// <summary>
    ///   <para>Creates a rotation which rotates from /fromDirection/ to /toDirection/.</para>
    /// </summary>
    /// <param name="fromDirection"></param>
    /// <param name="toDirection"></param>
    public void SetFromToRotation(Vector3D fromDirection, Vector3D toDirection)
    {
      this = QuaternionD.FromToRotation(fromDirection, toDirection);
    }
    /// <summary>
    ///   <para>Creates a rotation with the specified /forward/ and /upwards/ directions.</para>
    /// </summary>
    /// <param name="forward">The direction to look in.</param>
    /// <param name="upwards">The vector that defines in which direction up is.</param>
    public static QuaternionD LookRotation(Vector3D forward, [DefaultValue("Vector3D.up")] Vector3D upwards)
    {
      return QuaternionD.INTERNAL_CALL_LookRotation(ref forward, ref upwards);
    }
    [ExcludeFromDocs]
    public static QuaternionD LookRotation(Vector3D forward)
    {
      Vector3D up = Vector3D.up;
      return QuaternionD.INTERNAL_CALL_LookRotation(ref forward, ref up);
    }

    // from http://answers.unity3d.com/questions/467614/what-is-the-source-code-of-quaternionDlookrotation.html
    private static QuaternionD INTERNAL_CALL_LookRotation(ref Vector3D forward, ref Vector3D up)
    {

      forward = Vector3D.Normalize(forward);
      Vector3D right = Vector3D.Normalize(Vector3D.Cross(up, forward));
      up = Vector3D.Cross(forward, right);
      var m00 = right.x;
      var m01 = right.y;
      var m02 = right.z;
      var m10 = up.x;
      var m11 = up.y;
      var m12 = up.z;
      var m20 = forward.x;
      var m21 = forward.y;
      var m22 = forward.z;


      double num8 = (m00 + m11) + m22;
      var quaternionD = new QuaternionD();
      if (num8 > 0.0)
      {
        var num = Math.Sqrt(num8 + 1.0);
        quaternionD.w = num * 0.5;
        num = 0.5 / num;
        quaternionD.x = (m12 - m21) * num;
        quaternionD.y = (m20 - m02) * num;
        quaternionD.z = (m01 - m10) * num;
        return quaternionD;
      }
      if ((m00 >= m11) && (m00 >= m22))
      {
        var num7 = Math.Sqrt(((1.0 + m00) - m11) - m22);
        var num4 = 0.5 / num7;
        quaternionD.x = 0.5 * num7;
        quaternionD.y = (m01 + m10) * num4;
        quaternionD.z = (m02 + m20) * num4;
        quaternionD.w = (m12 - m21) * num4;
        return quaternionD;
      }
      if (m11 > m22)
      {
        var num6 = Math.Sqrt(((1.0 + m11) - m00) - m22);
        var num3 = 0.5 / num6;
        quaternionD.x = (m10 + m01) * num3;
        quaternionD.y = 0.5 * num6;
        quaternionD.z = (m21 + m12) * num3;
        quaternionD.w = (m20 - m02) * num3;
        return quaternionD;
      }
      var num5 = Math.Sqrt(((1.0 + m22) - m00) - m11);
      var num2 = 0.5 / num5;
      quaternionD.x = (m20 + m02) * num2;
      quaternionD.y = (m21 + m12) * num2;
      quaternionD.z = 0.5 * num5;
      quaternionD.w = (m01 - m10) * num2;
      return quaternionD;
    }
    [ExcludeFromDocs]
    public void SetLookRotation(Vector3D view)
    {
      Vector3D up = Vector3D.up;
      this.SetLookRotation(view, up);
    }
    /// <summary>
    ///   <para>Creates a rotation with the specified /forward/ and /upwards/ directions.</para>
    /// </summary>
    /// <param name="view">The direction to look in.</param>
    /// <param name="up">The vector that defines in which direction up is.</param>
    public void SetLookRotation(Vector3D view, [DefaultValue("Vector3D.up")] Vector3D up)
    {
      this = QuaternionD.LookRotation(view, up);
    }
    /// <summary>
    ///   <para>Spherically interpolates between /a/ and /b/ by t. The parameter /t/ is clamped to the range [0, 1].</para>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    public static QuaternionD Slerp(QuaternionD a, QuaternionD b, double t)
    {
      return QuaternionD.INTERNAL_CALL_Slerp(ref a, ref b, t);
    }

    private static QuaternionD INTERNAL_CALL_Slerp(ref QuaternionD a, ref QuaternionD b, double t)
    {
      if (t > 1) t = 1;
      if (t < 0) t = 0;
      return INTERNAL_CALL_SlerpUnclamped(ref a, ref b, t);
    }
    /// <summary>
    ///   <para>Spherically interpolates between /a/ and /b/ by t. The parameter /t/ is not clamped.</para>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    public static QuaternionD SlerpUnclamped(QuaternionD a, QuaternionD b, double t)
    {
      return QuaternionD.INTERNAL_CALL_SlerpUnclamped(ref a, ref b, t);
    }

    private static QuaternionD INTERNAL_CALL_SlerpUnclamped(ref QuaternionD a, ref QuaternionD b, double t)
    {
      // if either input is zero, return the other.
      if (a.LengthSquared == 0.0)
      {
        if (b.LengthSquared == 0.0)
        {
          return identity;
        }
        return b;
      }
      else if (b.LengthSquared == 0.0)
      {
        return a;
      }


      double cosHalfAngle = a.w * b.w + Vector3D.Dot(a.xyz, b.xyz);

      if (cosHalfAngle >= 1.0 || cosHalfAngle <= -1.0)
      {
        // angle = 0.0, so just return one input.
        return a;
      }
      else if (cosHalfAngle < 0.0)
      {
        b.xyz = -b.xyz;
        b.w = -b.w;
        cosHalfAngle = -cosHalfAngle;
      }

      double blendA;
      double blendB;
      if (cosHalfAngle < 0.99)
      {
        // do proper slerp for big angles
        double halfAngle = System.Math.Acos(cosHalfAngle);
        double sinHalfAngle = System.Math.Sin(halfAngle);
        double oneOverSinHalfAngle = 1.0 / sinHalfAngle;
        blendA = System.Math.Sin(halfAngle * (1.0 - t)) * oneOverSinHalfAngle;
        blendB = System.Math.Sin(halfAngle * t) * oneOverSinHalfAngle;
      }
      else
      {
        // do lerp if angle is really small.
        blendA = 1.0 - t;
        blendB = t;
      }

      QuaternionD result = new QuaternionD(blendA * a.xyz + blendB * b.xyz, blendA * a.w + blendB * b.w);
      if (result.LengthSquared > 0.0)
        return Normalize(result);
      else
        return identity;
    }
    /// <summary>
    ///   <para>Interpolates between /a/ and /b/ by /t/ and normalizes the result afterwards. The parameter /t/ is clamped to the range [0, 1].</para>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    public static QuaternionD Lerp(QuaternionD a, QuaternionD b, double t)
    {
      if (t > 1) t = 1;
      if (t < 0) t = 0;
      return INTERNAL_CALL_Slerp(ref a, ref b, t); // TODO: use lerp not slerp, "Because quaternionD works in 4D. Rotation in 4D are linear" ???
    }
    /// <summary>
    ///   <para>Interpolates between /a/ and /b/ by /t/ and normalizes the result afterwards. The parameter /t/ is not clamped.</para>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    public static QuaternionD LerpUnclamped(QuaternionD a, QuaternionD b, double t)
    {
      return INTERNAL_CALL_Slerp(ref a, ref b, t);
    }
    /// <summary>
    ///   <para>Rotates a rotation /from/ towards /to/.</para>
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="maxDegreesDelta"></param>
    public static QuaternionD RotateTowards(QuaternionD from, QuaternionD to, double maxDegreesDelta)
    {
      double num = QuaternionD.Angle(from, to);
      if (num == 0.0)
      {
        return to;
      }
      double t = Math.Min(1.0, maxDegreesDelta / num);
      return QuaternionD.SlerpUnclamped(from, to, t);
    }
    /// <summary>
    ///   <para>Returns the Inverse of /rotation/.</para>
    /// </summary>
    /// <param name="rotation"></param>
    public static QuaternionD Inverse(QuaternionD rotation)
    {
      double lengthSq = rotation.LengthSquared;
      if (lengthSq != 0.0)
      {
        double i = 1.0 / lengthSq;
        return new QuaternionD(rotation.xyz * -i, rotation.w * i);
      }
      return rotation;
    }
    /// <summary>
    ///   <para>Returns a nicely formatted string of the QuaternionD.</para>
    /// </summary>
    /// <param name="format"></param>
    public override string ToString()
    {
      return string.Format("({0:F1}, {1:F1}, {2:F1}, {3:F1})", new object[]
      {
                this.x,
                this.y,
                this.z,
                this.w
      });
    }
    /// <summary>
    ///   <para>Returns a nicely formatted string of the QuaternionD.</para>
    /// </summary>
    /// <param name="format"></param>
    public string ToString(string format)
    {
      return string.Format("({0}, {1}, {2}, {3})", new object[]
      {
                this.x.ToString(format),
                this.y.ToString(format),
                this.z.ToString(format),
                this.w.ToString(format)
      });
    }
    /// <summary>
    ///   <para>Returns the angle in degrees between two rotations /a/ and /b/.</para>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static double Angle(QuaternionD a, QuaternionD b)
    {
      double f = QuaternionD.Dot(a, b);
      return Math.Acos(Math.Min(Math.Abs(f), 1.0)) * 2.0 * radToDeg;
    }
    /// <summary>
    ///   <para>Returns a rotation that rotates z degrees around the z axis, x degrees around the x axis, and y degrees around the y axis (in that order).</para>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public static QuaternionD Euler(double x, double y, double z)
    {
      return QuaternionD.Internal_FromEulerRad(new Vector3D(x, y, z) * degToRad);
    }
    /// <summary>
    ///   <para>Returns a rotation that rotates z degrees around the z axis, x degrees around the x axis, and y degrees around the y axis (in that order).</para>
    /// </summary>
    /// <param name="euler"></param>
    public static QuaternionD Euler(Vector3D euler)
    {
      return QuaternionD.Internal_FromEulerRad(euler * degToRad);

    }

    // from http://stackoverflow.com/questions/12088610/conversion-between-euler-quaternionD-like-in-unity3d-engine
    private static Vector3D Internal_ToEulerRad(QuaternionD rotation)
    {
      double sqw = rotation.w * rotation.w;
      double sqx = rotation.x * rotation.x;
      double sqy = rotation.y * rotation.y;
      double sqz = rotation.z * rotation.z;
      double unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
      double test = rotation.x * rotation.w - rotation.y * rotation.z;
      Vector3D v;

      if (test > 0.4995 * unit)
      { // singularity at north pole
        v.y = 2.0 * Math.Atan2(rotation.y, rotation.x);
        v.x = Math.PI / 2;
        v.z = 0;
        return NormalizeAngles(v * MathHelper.Rad2Deg);
      }
      if (test < -0.4995 * unit)
      { // singularity at south pole
        v.y = -2.0 * Math.Atan2(rotation.y, rotation.x);
        v.x = -Math.PI / 2;
        v.z = 0;
        return NormalizeAngles(v * MathHelper.Rad2Deg);
      }
      QuaternionD q = new QuaternionD(rotation.w, rotation.z, rotation.x, rotation.y);
      v.y = Math.Atan2(2.0 * q.x * q.w + 2.0 * q.y * q.z, 1.0 - 2.0 * (q.z * q.z + q.w * q.w));     // Yaw
      v.x = Math.Asin(2.0 * (q.x * q.z - q.w * q.y));                             // Pitch
      v.z = Math.Atan2(2.0 * q.x * q.y + 2.0 * q.z * q.w, 1.0 - 2.0 * (q.y * q.y + q.z * q.z));      // Roll
      return NormalizeAngles(v * MathHelper.Rad2Deg);
    }
    private static Vector3D NormalizeAngles(Vector3D angles)
    {
      angles.x = NormalizeAngle(angles.x);
      angles.y = NormalizeAngle(angles.y);
      angles.z = NormalizeAngle(angles.z);
      return angles;
    }

    private static double NormalizeAngle(double angle)
    {
      double modAngle = angle % 360.0;

      if (modAngle < 0.0)
        return modAngle + 360.0;
      else
        return modAngle;
    }

    // from http://stackoverflow.com/questions/11492299/quaternionD-to-euler-angles-algorithm-how-to-convert-to-y-up-and-between-ha
    private static QuaternionD Internal_FromEulerRad(Vector3D euler)
    {
      var yaw = euler.x;
      var pitch = euler.y;
      var roll = euler.z;
      double rollOver2 = roll * 0.5;
      double sinRollOver2 = Math.Sin((double)rollOver2);
      double cosRollOver2 = Math.Cos((double)rollOver2);
      double pitchOver2 = pitch * 0.5;
      double sinPitchOver2 = Math.Sin((double)pitchOver2);
      double cosPitchOver2 = Math.Cos((double)pitchOver2);
      double yawOver2 = yaw * 0.5;
      double sinYawOver2 = Math.Sin((double)yawOver2);
      double cosYawOver2 = Math.Cos((double)yawOver2);
      QuaternionD result;
      result.x = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
      result.y = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;
      result.z = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
      result.w = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;
      return result;

    }
    private static void Internal_ToAxisAngleRad(QuaternionD q, out Vector3D axis, out double angle)
    {
      if (Math.Abs(q.w) > 1.0)
        q.Normalize();


      angle = 2.0 * System.Math.Acos(q.w); // angle
      double den = System.Math.Sqrt(1.0 - q.w * q.w);
      if (den > 0.0001)
      {
        axis = q.xyz / den;
      }
      else
      {
        // This occurs when the angle is zero. 
        // Not a problem: just set an arbitrary normalized axis.
        axis = new Vector3D(1, 0, 0);
      }
    }

    public override int GetHashCode()
    {
      return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2 ^ this.w.GetHashCode() >> 1;
    }

    public override bool Equals(object other)
    {
      if (!(other is QuaternionD))
      {
        return false;
      }
      QuaternionD quaternionD = (QuaternionD)other;
      return this.x.Equals(quaternionD.x) && this.y.Equals(quaternionD.y) && this.z.Equals(quaternionD.z) && this.w.Equals(quaternionD.w);
    }

    public bool Equals(QuaternionD other)
    {
      return this.x.Equals(other.x) && this.y.Equals(other.y) && this.z.Equals(other.z) && this.w.Equals(other.w);
    }

    public static QuaternionD operator *(QuaternionD lhs, QuaternionD rhs)
    {
      return new QuaternionD(lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y, lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z, lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x, lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);
    }

    public static Vector3D operator *(QuaternionD rotation, Vector3D point)
    {
      double num = rotation.x * 2.0;
      double num2 = rotation.y * 2.0;
      double num3 = rotation.z * 2.0;
      double num4 = rotation.x * num;
      double num5 = rotation.y * num2;
      double num6 = rotation.z * num3;
      double num7 = rotation.x * num2;
      double num8 = rotation.x * num3;
      double num9 = rotation.y * num3;
      double num10 = rotation.w * num;
      double num11 = rotation.w * num2;
      double num12 = rotation.w * num3;
      Vector3D result;
      result.x = (1.0 - (num5 + num6)) * point.x + (num7 - num12) * point.y + (num8 + num11) * point.z;
      result.y = (num7 + num12) * point.x + (1.0 - (num4 + num6)) * point.y + (num9 - num10) * point.z;
      result.z = (num8 - num11) * point.x + (num9 + num10) * point.y + (1.0 - (num4 + num5)) * point.z;
      return result;
    }

    public static bool operator ==(QuaternionD lhs, QuaternionD rhs)
    {
      return Approximately(lhs, rhs);
    }

    public static bool operator !=(QuaternionD lhs, QuaternionD rhs)
    {
      return !Approximately(lhs, rhs);
    }

    public static explicit operator UnityEngine.Quaternion(QuaternionD me)
    {
      return new UnityEngine.Quaternion((float)me.x, (float)me.y, (float)me.z, (float)me.w);
    }

    public static explicit operator UnityEngine.Vector4(QuaternionD me)
    {
      return new UnityEngine.Vector4((float)me.x, (float)me.y, (float)me.z, (float)me.w);
    }

    public static implicit operator QuaternionD(UnityEngine.Quaternion other)
    {
      return new QuaternionD(other.x, other.y, other.z, other.w);
    }
  }
}