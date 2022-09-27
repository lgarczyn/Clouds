using UnityEngine;
using System.Linq;

[RequireComponent(typeof(TrailRenderer))]
public class TrailConstantLength : MonoBehaviour
{
  public float desiredLength = 100;
  public float maxTime = 10;

  Vector3[] cache;

  TrailRenderer trail
  {
    get
    {
      return GetComponent<TrailRenderer>();
    }
  }

  public float GetLength()
  {
    if (cache == null || cache.Length < trail.positionCount)
    {
      cache = new Vector3[trail.positionCount * 2];
    }

    int writtenCount = trail.GetPositions(cache);

    return cache
      .Take(writtenCount)
      .Skip(1)
      .Select((point, index) => (point - cache[index]).magnitude)
      .Sum();
  }

  public void LateUpdate()
  {

    float length = GetLength();

    if (length < 1f) return;

    float newTime = trail.time * desiredLength / length;

    trail.time = Mathf.Min(newTime, maxTime);
  }
}