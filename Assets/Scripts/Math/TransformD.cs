using UnityEngine;
using System;

[Serializable]
public struct TransformD : IEquatable<TransformD>
{
  public Vector3D position => _position;
  public QuaternionD rotation => _rotation;
  public double scale => _scale;

  [SerializeField]
  Vector3D _position;
  [SerializeField]
  QuaternionD _rotation;
  [SerializeField]
  double _scale;

  public static TransformD identity = new TransformD(Vector3D.zero);
  public TransformD(Vector3D position) : this(position, QuaternionD.identity) { }
  public TransformD(Vector3D position, QuaternionD rotation, double scale = 1)
  {
    this._position = position;
    this._rotation = rotation;
    this._scale = scale;
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
