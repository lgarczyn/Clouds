using UnityEngine;

/// <summary>
/// Provides an inertial frame for the current game space
/// Handles the jupiter space coordinates of the current game space
/// </summary>
[RequireComponent(typeof(OrbitControllerBridge))]
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

  void Update()
  {
    // TODO: conserve movement instead of resetting each frame
    UpdatePos();
  }

  void Awake()
  {
    UpdatePos();
  }

  void UpdatePos()
  {
    var jupiterSpace = GetComponent<OrbitControllerBridge>().instance;
    TransformD transform = jupiterSpace.frame.GetSurfaceTransform(
      latitude,
      longitude,
      bearing,
      altitude
    );

    frame = new InertialFrame(transform);
  }
}
