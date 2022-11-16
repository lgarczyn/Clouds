using UnityEngine;
using System.Linq;

public class TrailConstantLength : MonoBehaviour
{
  public float desiredLength = 100;
  public float maxTime = 10;

  [SerializeField][RequiredComponent] TrailRenderer reqTrailRenderer;

  Vector3[] cache;

  public float GetLength()
  {
    if (cache == null || cache.Length < reqTrailRenderer.positionCount)
    {
      cache = new Vector3[reqTrailRenderer.positionCount * 2];
    }

    int writtenCount = reqTrailRenderer.GetPositions(cache);

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

    float newTime = reqTrailRenderer.time * desiredLength / length;

    reqTrailRenderer.time = Mathf.Min(newTime, maxTime);
  }
}