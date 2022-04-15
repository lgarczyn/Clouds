using UnityEngine;

/// <summary>
/// Offers a transform from real-scale Jupiter space to Unity scaled space
/// </summary>
public class ScaledSpaceController : MonoBehaviour
{
  public FrameOfReference frame;

  public const double SPACE_SCALE = 5 / JupiterSpace.JUPITER_RADIUS;

  void Awake()
  {
    frame = new FrameOfReference(new TransformD(
        Vector3D.zero,
        QuaternionD.identity,
        SPACE_SCALE
    ));
  }
}
