using UnityEngine;
using System.Linq;

[RequireComponent(typeof(TrailRenderer))]
public class TrailNoiseEffect : MonoBehaviour
{
  public float noiseScale = 1f;
  public float noiseRate = 1f;

  [SerializeField][RequiredComponent] TrailRenderer reqTrailRenderer;

  Vector3 OffsetPosition(Vector3 pos, int index)
  {
    if (index < 3) return pos;
    var p = pos / noiseScale;
    return pos + new Vector3(
      Mathf.PerlinNoise(p.y, p.z),
      Mathf.PerlinNoise(p.x, p.z),
      Mathf.PerlinNoise(p.x, p.y)
    ) * Time.deltaTime * noiseRate;
  }

  void Update() {

    if (reqTrailRenderer.enabled == false) return;

    var points = new Vector3[reqTrailRenderer.positionCount];
    reqTrailRenderer.GetPositions(points);

    points = points.Select(OffsetPosition).ToArray();

    reqTrailRenderer.SetPositions(points);
  }
}
