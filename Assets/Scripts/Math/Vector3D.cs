// Changed to double from unity sources by lgarczyn
// Original source: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Math/Vector3.cs
// TODO: own implementation to avoid license nightmare

// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Globalization;
using UnityEditor;

namespace UnityEngine
{
  [Serializable]
  /// Representation of 3D vectors and points.
  public partial struct Vector3D : IEquatable<Vector3D>, IFormattable
  {
    // A more precise version of Vector3 kEpsilon
    // Original value is 0.00001F;
    public const double kEpsilon = 0.000000001;

    // A more precise version of Vector3 kEpsilonNormalSqrt
    // Original value is 1e-15F;
    public const double kEpsilonNormalSqrt = 1e-25F;

    ///<summary>
    /// X component of the vector.
    ///</summary>
    public double x;
    ///<summary>
    /// Y component of the vector.
    ///</summary>
    public double y;
    ///<summary>
    /// Z component of the vector.
    ///</summary>
    public double z;

    ///<summary>
    /// Linearly interpolates between two vectors.
    ///</summary>
    public static Vector3D Lerp(Vector3D a, Vector3D b, double t)
    {
      t = MathHelper.Clamp01(t);
      return new Vector3D(
          a.x + (b.x - a.x) * t,
          a.y + (b.y - a.y) * t,
          a.z + (b.z - a.z) * t
      );
    }

    ///<summary>
    /// Linearly interpolates between two vectors without clamping the interpolant
    ///</summary>
    public static Vector3D LerpUnclamped(Vector3D a, Vector3D b, double t)
    {
      return new Vector3D(
          a.x + (b.x - a.x) * t,
          a.y + (b.y - a.y) * t,
          a.z + (b.z - a.z) * t
      );
    }

    ///<summary>
    /// Moves a point /current/ in a straight line towards a /target/ point.
    ///</summary>
    public static Vector3D MoveTowards(Vector3D current, Vector3D target, double maxDistanceDelta)
    {
      // avoid vector ops because current scripting backends are terrible at inlining
      double toVector_x = target.x - current.x;
      double toVector_y = target.y - current.y;
      double toVector_z = target.z - current.z;

      double sqdist = toVector_x * toVector_x + toVector_y * toVector_y + toVector_z * toVector_z;

      if (sqdist == 0 || (maxDistanceDelta >= 0 && sqdist <= maxDistanceDelta * maxDistanceDelta))
        return target;
      var dist = (double)Math.Sqrt(sqdist);

      return new Vector3D(current.x + toVector_x / dist * maxDistanceDelta,
          current.y + toVector_y / dist * maxDistanceDelta,
          current.z + toVector_z / dist * maxDistanceDelta);
    }

    public static Vector3D SmoothDamp(Vector3D current, Vector3D target, ref Vector3D currentVelocity, double smoothTime, double maxSpeed)
    {
      double deltaTime = Time.deltaTime;
      return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    public static Vector3D SmoothDamp(Vector3D current, Vector3D target, ref Vector3D currentVelocity, double smoothTime)
    {
      double deltaTime = Time.deltaTime;
      double maxSpeed = Double.PositiveInfinity;
      return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    ///<summary>
    /// Gradually changes a vector towards a desired goal over time.
    ///</summary>
    public static Vector3D SmoothDamp(Vector3D current, Vector3D target, ref Vector3D currentVelocity, double smoothTime, double maxSpeed, double deltaTime)
    {
      double output_x = 0.0;
      double output_y = 0.0;
      double output_z = 0.0;

      // Based on Game Programming Gems 4 Chapter 1.10
      smoothTime = Math.Max(0.0001, smoothTime);
      double omega = 2F / smoothTime;

      double x = omega * deltaTime;
      double exp = 1F / (1F + x + 0.48 * x * x + 0.235 * x * x * x);

      double change_x = current.x - target.x;
      double change_y = current.y - target.y;
      double change_z = current.z - target.z;
      Vector3D originalTo = target;

      // Clamp maximum speed
      double maxChange = maxSpeed * smoothTime;

      double maxChangeSq = maxChange * maxChange;
      double sqrmag = change_x * change_x + change_y * change_y + change_z * change_z;
      if (sqrmag > maxChangeSq)
      {
        var mag = (double)Math.Sqrt(sqrmag);
        change_x = change_x / mag * maxChange;
        change_y = change_y / mag * maxChange;
        change_z = change_z / mag * maxChange;
      }

      target.x = current.x - change_x;
      target.y = current.y - change_y;
      target.z = current.z - change_z;

      double temp_x = (currentVelocity.x + omega * change_x) * deltaTime;
      double temp_y = (currentVelocity.y + omega * change_y) * deltaTime;
      double temp_z = (currentVelocity.z + omega * change_z) * deltaTime;

      currentVelocity.x = (currentVelocity.x - omega * temp_x) * exp;
      currentVelocity.y = (currentVelocity.y - omega * temp_y) * exp;
      currentVelocity.z = (currentVelocity.z - omega * temp_z) * exp;

      output_x = target.x + (change_x + temp_x) * exp;
      output_y = target.y + (change_y + temp_y) * exp;
      output_z = target.z + (change_z + temp_z) * exp;

      // Prevent overshooting
      double origMinusCurrent_x = originalTo.x - current.x;
      double origMinusCurrent_y = originalTo.y - current.y;
      double origMinusCurrent_z = originalTo.z - current.z;
      double outMinusOrig_x = output_x - originalTo.x;
      double outMinusOrig_y = output_y - originalTo.y;
      double outMinusOrig_z = output_z - originalTo.z;

      if (origMinusCurrent_x * outMinusOrig_x + origMinusCurrent_y * outMinusOrig_y + origMinusCurrent_z * outMinusOrig_z > 0)
      {
        output_x = originalTo.x;
        output_y = originalTo.y;
        output_z = originalTo.z;

        currentVelocity.x = (output_x - originalTo.x) / deltaTime;
        currentVelocity.y = (output_y - originalTo.y) / deltaTime;
        currentVelocity.z = (output_z - originalTo.z) / deltaTime;
      }

      return new Vector3D(output_x, output_y, output_z);
    }

    /// <summary>
    /// Compares two vectors and returns true if they are similar.
    /// </summary>
    /// <param name="epsilon">
    /// The smallest acceptable difference
    /// </param>
    public static bool Approximately(Vector3D a, Vector3D b, double epsilon = Double.Epsilon)
    {
      return MathHelper.Approximately(a.x, a.x, epsilon)
        && MathHelper.Approximately(a.y, a.y, epsilon)
        && MathHelper.Approximately(a.z, a.z, epsilon);
    }

    /// <summary>
    /// Compares to another vector and returns true if they are similar.
    /// </summary>
    /// <param name="epsilon">
    /// The smallest acceptable difference
    /// </param>
    public bool Approximately(Vector3D b, double epsilon = Double.Epsilon)
    {
      return Vector3D.Approximately(this, b, epsilon);
    }


    ///<summary>
    /// Access the x, y, z components using [0], [1], [2] respectively.
    ///</summary>
    public double this[int index]
    {

      get
      {
        switch (index)
        {
          case 0: return x;
          case 1: return y;
          case 2: return z;
          default:
            throw new IndexOutOfRangeException("Invalid Vector3D index!");
        }
      }


      set
      {
        switch (index)
        {
          case 0: x = value; break;
          case 1: y = value; break;
          case 2: z = value; break;
          default:
            throw new IndexOutOfRangeException("Invalid Vector3D index!");
        }
      }
    }

    ///<summary>
    /// Creates a new vector with given x, y, z components.
    ///</summary>
    public Vector3D(double x, double y, double z) { this.x = x; this.y = y; this.z = z; }
    ///<summary>
    /// Creates a new vector with given x, y components and sets /z/ to zero.
    ///</summary>
    public Vector3D(double x, double y) { this.x = x; this.y = y; z = 0.0; }

    ///<summary>
    /// Set x, y and z components of an existing Vector3D.
    ///</summary>
    public void Set(double newX, double newY, double newZ) { x = newX; y = newY; z = newZ; }

    ///<summary>
    /// Multiplies two vectors component-wise.
    ///</summary>
    public static Vector3D Scale(Vector3D a, Vector3D b) { return new Vector3D(a.x * b.x, a.y * b.y, a.z * b.z); }

    ///<summary>
    /// Multiplies every component of this vector by the same component of /scale/.
    ///</summary>
    public void Scale(Vector3D scale) { x *= scale.x; y *= scale.y; z *= scale.z; }

    ///<summary>
    /// Cross Product of two vectors.
    ///</summary>
    public static Vector3D Cross(Vector3D lhs, Vector3D rhs)
    {
      return new Vector3D(
          lhs.y * rhs.z - lhs.z * rhs.y,
          lhs.z * rhs.x - lhs.x * rhs.z,
          lhs.x * rhs.y - lhs.y * rhs.x);
    }

    ///<summary>
    /// used to allow Vector3Ds to be used as keys in hash tables
    ///</summary>
    public override int GetHashCode()
    {
      return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
    }

    ///<summary>
    /// also required for being able to use Vector3Ds as keys in hash tables
    ///</summary>
    public override bool Equals(object other)
    {
      if (!(other is Vector3D)) return false;

      return Equals((Vector3D)other);
    }

    public bool Equals(Vector3D other)
    {
      return x == other.x && y == other.y && z == other.z;
    }

    ///<summary>
    /// Reflects a vector off the plane defined by a normal.
    ///</summary>
    public static Vector3D Reflect(Vector3D inDirection, Vector3D inNormal)
    {
      double factor = -2.0 * Dot(inNormal, inDirection);
      return new Vector3D(factor * inNormal.x + inDirection.x,
          factor * inNormal.y + inDirection.y,
          factor * inNormal.z + inDirection.z);
    }

    ///<summary>
    /// *undoc* --- we have normalized property now
    ///</summary>
    public static Vector3D Normalize(Vector3D value)
    {
      double mag = value.magnitude;
      if (mag > kEpsilon)
        return value / mag;
      else
        return zero;
    }

    ///<summary>
    /// Makes this vector have a ::ref::magnitude of 1.
    ///</summary>
    public void Normalize()
    {
      double mag = this.magnitude;
      if (mag > kEpsilon)
        this = this / mag;
      else
        this = zero;
    }

    ///<summary>
    /// Returns this vector with a ::ref::magnitude of 1 (RO).
    ///</summary>
    public Vector3D normalized
    {
      get
      {
        double mag = this.magnitude;
        if (mag > kEpsilon)
          return this / mag;
        else
          return zero;
      }
    }

    ///<summary>
    /// Dot Product of two vectors.
    ///</summary>
    public static double Dot(Vector3D lhs, Vector3D rhs) { return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z; }

    ///<summary>
    /// Projects a vector onto another vector.
    ///</summary>
    public static Vector3D Project(Vector3D vector, Vector3D onNormal)
    {
      double sqrMag = Dot(onNormal, onNormal);
      if (sqrMag < Double.Epsilon)
        return zero;
      else
      {
        var dot = Dot(vector, onNormal);
        return new Vector3D(onNormal.x * dot / sqrMag,
            onNormal.y * dot / sqrMag,
            onNormal.z * dot / sqrMag);
      }
    }

    ///<summary>
    /// Projects a vector onto a plane defined by a normal orthogonal to the plane.
    ///</summary>
    public static Vector3D ProjectOnPlane(Vector3D vector, Vector3D planeNormal)
    {
      double sqrMag = Dot(planeNormal, planeNormal);
      if (sqrMag < Double.Epsilon)
        return vector;
      else
      {
        var dot = Dot(vector, planeNormal);
        return new Vector3D(vector.x - planeNormal.x * dot / sqrMag,
            vector.y - planeNormal.y * dot / sqrMag,
            vector.z - planeNormal.z * dot / sqrMag);
      }
    }

    ///<summary>
    /// Returns the angle in degrees between /from/ and /to/. This is always the smallest
    ///</summary>
    public static double Angle(Vector3D from, Vector3D to)
    {
      // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
      double denominator = (double)Math.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
      if (denominator < kEpsilonNormalSqrt)
        return 0.0;

      double dot = Math.Clamp(Dot(from, to) / denominator, -1.0, 1.0);
      return ((double)Math.Acos(dot)) * MathHelper.Rad2Deg;
    }

    // The smaller of the two possible angles between the two vectors is returned, therefore the result will never be greater than 180 degrees or smaller than -180 degrees.
    // If you imagine the from and to vectors as lines on a piece of paper, both originating from the same point, then the /axis/ vector would point up out of the paper.
    // The measured angle between the two vectors would be positive in a clockwise direction and negative in an anti-clockwise direction.
    public static double SignedAngle(Vector3D from, Vector3D to, Vector3D axis)
    {
      double unsignedAngle = Angle(from, to);

      double cross_x = from.y * to.z - from.z * to.y;
      double cross_y = from.z * to.x - from.x * to.z;
      double cross_z = from.x * to.y - from.y * to.x;
      double sign = Math.Sign(axis.x * cross_x + axis.y * cross_y + axis.z * cross_z);
      return unsignedAngle * sign;
    }

    ///<summary>
    /// Returns the distance between /a/ and /b/.
    ///</summary>
    public static double Distance(Vector3D a, Vector3D b)
    {
      double diff_x = a.x - b.x;
      double diff_y = a.y - b.y;
      double diff_z = a.z - b.z;
      return (double)Math.Sqrt(diff_x * diff_x + diff_y * diff_y + diff_z * diff_z);
    }

    ///<summary>
    /// Returns a copy of /vector/ with its magnitude clamped to /maxLength/.
    ///</summary>
    public static Vector3D ClampMagnitude(Vector3D vector, double maxLength)
    {
      double sqrmag = vector.sqrMagnitude;
      if (sqrmag > maxLength * maxLength)
      {
        double mag = (double)Math.Sqrt(sqrmag);
        //these intermediate variables force the intermediate result to be
        //of double precision. without this, the intermediate result can be of higher
        //precision, which changes behavior.
        double normalized_x = vector.x / mag;
        double normalized_y = vector.y / mag;
        double normalized_z = vector.z / mag;
        return new Vector3D(normalized_x * maxLength,
            normalized_y * maxLength,
            normalized_z * maxLength);
      }
      return vector;
    }

    ///<summary>
    /// Returns the length of this vector (RO).
    ///</summary>
    public double magnitude
    {
      get { return (double)Math.Sqrt(x * x + y * y + z * z); }
    }

    ///<summary>
    /// Returns the squared length of this vector (RO).
    ///</summary>
    public double sqrMagnitude { get { return x * x + y * y + z * z; } }

    ///<summary>
    /// Returns a vector that is made from the smallest components of two vectors.
    ///</summary>
    public static Vector3D Min(Vector3D lhs, Vector3D rhs)
    {
      return new Vector3D(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y), Math.Min(lhs.z, rhs.z));
    }

    ///<summary>
    /// Returns a vector that is made from the largest components of two vectors.
    ///</summary>
    public static Vector3D Max(Vector3D lhs, Vector3D rhs)
    {
      return new Vector3D(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z));
    }

    static readonly Vector3D zeroVector = new Vector3D(0.0, 0.0, 0.0);
    static readonly Vector3D oneVector = new Vector3D(1.0, 1.0, 1.0);
    static readonly Vector3D upVector = new Vector3D(0.0, 1.0, 0.0);
    static readonly Vector3D downVector = new Vector3D(0.0, -1.0, 0.0);
    static readonly Vector3D leftVector = new Vector3D(-1.0, 0.0, 0.0);
    static readonly Vector3D rightVector = new Vector3D(1.0, 0.0, 0.0);
    static readonly Vector3D forwardVector = new Vector3D(0.0, 0.0, 1.0);
    static readonly Vector3D backVector = new Vector3D(0.0, 0.0, -1.0);
    static readonly Vector3D positiveInfinityVector = new Vector3D(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
    static readonly Vector3D negativeInfinityVector = new Vector3D(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);

    ///<summary>
    /// Shorthand for writing `Vector3D(0, 0, 0)`
    ///</summary>
    public static Vector3D zero { get { return zeroVector; } }
    ///<summary>
    /// Shorthand for writing `Vector3D(1, 1, 1)`
    ///</summary>
    public static Vector3D one { get { return oneVector; } }
    ///<summary>
    /// Shorthand for writing `Vector3D(0, 0, 1)`
    ///</summary>
    public static Vector3D forward { get { return forwardVector; } }
    public static Vector3D back { get { return backVector; } }
    ///<summary>
    /// Shorthand for writing `Vector3D(0, 1, 0)`
    ///</summary>
    public static Vector3D up { get { return upVector; } }
    public static Vector3D down { get { return downVector; } }
    public static Vector3D left { get { return leftVector; } }
    ///<summary>
    /// Shorthand for writing `Vector3D(1, 0, 0)`
    ///</summary>
    public static Vector3D right { get { return rightVector; } }
    ///<summary>
    /// Shorthand for writing `Vector3D(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity)`
    ///</summary>
    public static Vector3D positiveInfinity { get { return positiveInfinityVector; } }
    ///<summary>
    /// Shorthand for writing `Vector3D(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity)`
    ///</summary>
    public static Vector3D negativeInfinity { get { return negativeInfinityVector; } }

    ///<summary>
    /// Adds two vectors.
    ///</summary>
    public static Vector3D operator +(Vector3D a, Vector3D b) { return new Vector3D(a.x + b.x, a.y + b.y, a.z + b.z); }
    ///<summary>
    /// Subtracts one vector from another.
    ///</summary>
    public static Vector3D operator -(Vector3D a, Vector3D b) { return new Vector3D(a.x - b.x, a.y - b.y, a.z - b.z); }
    ///<summary>
    /// Negates a vector.
    ///</summary>
    public static Vector3D operator -(Vector3D a) { return new Vector3D(-a.x, -a.y, -a.z); }
    ///<summary>
    /// Multiplies a vector by a number.
    ///</summary>
    public static Vector3D operator *(Vector3D a, double d) { return new Vector3D(a.x * d, a.y * d, a.z * d); }
    ///<summary>
    /// Multiplies a vector by a number.
    ///</summary>
    public static Vector3D operator *(double d, Vector3D a) { return new Vector3D(a.x * d, a.y * d, a.z * d); }
    ///<summary>
    /// Divides a vector by a number.
    ///</summary>
    public static Vector3D operator /(Vector3D a, double d) { return new Vector3D(a.x / d, a.y / d, a.z / d); }

    ///<summary>
    /// Returns true if the vectors are equal.
    ///</summary>
    public static bool operator ==(Vector3D lhs, Vector3D rhs)
    {
      // Returns false in the presence of NaN values.
      double diff_x = lhs.x - rhs.x;
      double diff_y = lhs.y - rhs.y;
      double diff_z = lhs.z - rhs.z;
      double sqrmag = diff_x * diff_x + diff_y * diff_y + diff_z * diff_z;
      return sqrmag < kEpsilon * kEpsilon;
    }

    ///<summary>
    /// Returns true if vectors are different.
    ///</summary>
    public static bool operator !=(Vector3D lhs, Vector3D rhs)
    {
      // Returns true in the presence of NaN values.
      return !(lhs == rhs);
    }

    public override string ToString()
    {
      return ToString(null, null);
    }

    public string ToString(string format)
    {
      return ToString(format, null);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
      if (string.IsNullOrEmpty(format))
        format = "F2";
      if (formatProvider == null)
        formatProvider = CultureInfo.InvariantCulture.NumberFormat;
      return String.Format("({0}, {1}, {2})", x.ToString(format, formatProvider), y.ToString(format, formatProvider), z.ToString(format, formatProvider));
    }

    public static explicit operator UnityEngine.Vector3(Vector3D me)
    {
      return new UnityEngine.Vector3((float)me.x, (float)me.y, (float)me.z);
    }

    public static implicit operator Vector3D(UnityEngine.Vector3 other)
    {
      return new Vector3D(other.x, other.y, other.z);
    }
  }
}