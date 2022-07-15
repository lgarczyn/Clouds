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

  public static Vector2 Min(this Vector2 a, Vector2 b)
  {
    return new Vector2(MathF.Min(a.x, b.x), MathF.Min(a.y, b.y));
  }

  public static Vector3 Min(this Vector3 a, Vector3 b)
  {
    return new Vector3(MathF.Min(a.x, b.x), MathF.Min(a.y, b.y), MathF.Min(a.z, b.z));
  }

  public static Vector4 Min(this Vector4 a, Vector4 b)
  {
    return new Vector4(MathF.Min(a.x, b.x), MathF.Min(a.y, b.y), MathF.Min(a.z, b.z), MathF.Min(a.w, b.w));
  }

  public static Vector2 Max(this Vector2 a, Vector2 b)
  {
    return new Vector2(MathF.Max(a.x, b.x), MathF.Max(a.y, b.y));
  }

  public static Vector3 Max(this Vector3 a, Vector3 b)
  {
    return new Vector3(MathF.Max(a.x, b.x), MathF.Max(a.y, b.y), MathF.Max(a.z, b.z));
  }

  public static Vector4 Max(this Vector4 a, Vector4 b)
  {
    return new Vector4(MathF.Max(a.x, b.x), MathF.Max(a.y, b.y), MathF.Max(a.z, b.z), MathF.Max(a.w, b.w));
  }
}