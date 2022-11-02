using UnityEngine;

/// <summary>
/// Offers a transform from real-scale Jupiter space to Unity scaled space
/// </summary>
public class ScaledSpaceController : Manager<ScaledSpaceController>
{
  public FrameOfReference frame;

  public const double SPACE_SCALE = 50 / JupiterSpace.JUPITER_RADIUS;

  protected override void Awake()
  {
    base.Awake();
    frame = new FrameOfReference(new TransformD(
        Vector3D.zero,
        QuaternionD.identity,
        SPACE_SCALE
    ));
  }
}
