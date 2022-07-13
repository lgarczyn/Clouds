using System;
using UnityEngine;


/// <summary>
/// Vector helpers.
/// </summary>
public static class VectorHelper
{
  public static Vector2 Scale(this Vector2 a, Vector2 b)
  {
    return Vector2.Scale(a, b);
  }
  public static Vector3 Scale(this Vector3 a, Vector3 b)
  {
    return Vector3.Scale(a, b);
  }
  public static Vector4 Scale(this Vector4 a, Vector4 b)
  {
    return Vector4.Scale(a, b);
  }

  public static Vector2 Inverse(this Vector2 a)
  {
    return new Vector2(1 / a.x, 1 / a.y);
  }
  public static Vector3 Inverse(this Vector3 a)
  {
    return new Vector3(1 / a.x, 1 / a.y, 1 / a.z);
  }
  public static Vector4 Inverse(this Vector4 a)
  {
    return new Vector4(1 / a.x, 1 / a.y, 1 / a.z, 1 / a.w);
  }

  public static Vector2 InverseScale(this Vector2 a, Vector2 b)
  {
    return Vector2.Scale(a, b.Inverse());
  }
  public static Vector3 InverseScale(this Vector3 a, Vector3 b)
  {
    return Vector3.Scale(a, b.Inverse());
  }
  public static Vector4 InverseScale(this Vector4 a, Vector4 b)
  {
    return Vector4.Scale(a, b.Inverse());
  }
}