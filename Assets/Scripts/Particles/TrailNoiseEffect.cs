using UnityEngine;
using Unity.Collections;

[RequireComponent(typeof(TrailRenderer))]
public class TrailNoiseEffect : MonoBehaviour
{
  public float noiseScale = 20f;
  public float noiseRate = 1f;

  public uint skippedVertices = 2;

  [SerializeField][RequiredComponent] TrailRenderer reqTrailRenderer;

  Vector3 OffsetPosition(Vector3 pos)
  {
    var p = pos / noiseScale;
    return pos + new Vector3(
      Mathf.PerlinNoise(p.y, p.z) - 0.5f,
      Mathf.PerlinNoise(p.x, p.z) - 1f,
      Mathf.PerlinNoise(p.x, p.y) - 0.5f
    ) * Time.deltaTime * noiseRate;
  }

  void Update() {

    if (reqTrailRenderer.enabled == false ||
      reqTrailRenderer.positionCount <= skippedVertices) return;

    int count = reqTrailRenderer.positionCount;

    NativeArray<Vector3> buffer = new NativeArray<Vector3>(count, Allocator.Temp);

    reqTrailRenderer.GetPositions(buffer);

    for (int i = 0; i < count - skippedVertices; i++) {
      buffer[i] = OffsetPosition(buffer[i]);
    }

    reqTrailRenderer.SetPositions(buffer);
  }
}
