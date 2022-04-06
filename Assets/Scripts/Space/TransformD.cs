using UnityEngine;
using System;
using UnityEngine.Internal;

public struct TransformD : IEquatable<TransformD>
{
  public readonly Vector3D position;
  public readonly QuaternionD rotation;

  public readonly double scale;

  public TransformD(Vector3D position, QuaternionD rotation, double scale = 1)
  {
    this.position = position;
    this.rotation = rotation;
    this.scale = scale;
  }

  public override bool Equals(object obj)
  {
    if (!(obj is TransformD)) return false;
    return TransformD.Equals(this, (TransformD)obj);
  }

  public bool Equals(TransformD rhs)
  {
    return Tuple.Equals(this.toTuple(), rhs.toTuple());
  }

  Tuple<Vector3D, QuaternionD, double> toTuple()
  {
    return Tuple.Create(this.position, this.rotation, this.scale);
  }

  public override int GetHashCode()
  {
    return this.toTuple().GetHashCode();
  }

  public static bool operator ==(TransformD lhs, TransformD rhs)
  {
    return lhs.toTuple() == rhs.toTuple();
  }

  public static bool operator !=(TransformD lhs, TransformD rhs)
  {
    return lhs.toTuple() != rhs.toTuple();
  }
  public static bool Approximately(TransformD a, TransformD b, double epsilon = Double.Epsilon)
  {
    return a.position.Approximately(b.position, epsilon)
      && a.rotation.Approximately(b.rotation, epsilon)
      && a.scale.Approximately(b.scale, epsilon);
  }

  public bool Approximately(TransformD b, double epsilon = Double.Epsilon)
  {
    return TransformD.Approximately(this, b, epsilon);
  }

  public override string ToString()
  {
    return ToString(null);
  }

  public string ToString(string format)
  {
    if (string.IsNullOrEmpty(format))
      format = "F10";
    return String.Format(
      "( position: {0},\n   rotation: {1},\n   scale: {2} )",
      this.position.ToString(format),
      this.rotation.ToString(format),
      this.scale.ToString(format));
  }
}
