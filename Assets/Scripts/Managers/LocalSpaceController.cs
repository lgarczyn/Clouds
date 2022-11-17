using UnityEngine;

/// <summary>
/// Provides an inertial frame for the current game space
/// Handles the jupiter space coordinates of the current game space
/// </summary>
public class LocalSpaceController : Manager<LocalSpaceController>
{
  public InertialFrame frame;

  public double timeScale;

  [Range(-90, +90)]
  public double latitude;
  [Range(-180, +180)]
  public double longitude;
  [Range(-180, +180)]
  public double bearing;
  public double altitude;

  [SerializeField][RequiredComponent] OrbitControllerBridge reqOrbitControllerBridge;

  void Update()
  {
    // TODO: conserve movement instead of resetting each frame
    UpdatePos();
  }

  protected override void Awake()
  {
    base.Awake();
    UpdatePos();
  }

  void UpdatePos()
  {
    var jupiterSpace = reqOrbitControllerBridge.instance;
    TransformD transform = jupiterSpace.frame.GetSurfaceTransform(
      latitude,
      longitude,
      bearing,
      altitude
    );

    frame = new InertialFrame(transform);
  }

  public QuaternionD GetSunRotation() {
    UpdatePos();
    QuaternionD inJupiterSpace = reqOrbitControllerBridge.instance.frame.GetSunRotation();
    QuaternionD inLocalSpace = frame.toLocalRot(inJupiterSpace);

    return inLocalSpace;
  }

  public Vector3D GetSunDirection() {
    return GetSunRotation() * Vector3D.forward;
  }
}
