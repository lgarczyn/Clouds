using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Vector list helpers.
/// </summary>
public static class VectorEnumerableHelper
{
  public static Vector2 Average(this IEnumerable<Vector2> l)
  {
    return l.Aggregate((a, b) => a + b) / l.Count();
  }

  public static Vector3 Average(this IEnumerable<Vector3> l)
  {
    return l.Aggregate((a, b) => a + b) / l.Count();
  }

  public static Vector4 Average(this IEnumerable<Vector4> l)
  {
    return l.Aggregate((a, b) => a + b) / l.Count();
  }
}