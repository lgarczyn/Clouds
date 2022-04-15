using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides an inertial frame for the current game space
/// Handles the jupiter space coordinates of the current game space
/// </summary>
public class LocalSpaceController : MonoBehaviour
{
  public InertialFrame frame;

  public OrbitController jupiterSpace;

  public double timeScale;

  [Range(-90, +90)]
  public double latitude;
  [Range(-180, +180)]
  public double longitude;
  [Range(-180, +180)]
  public double bearing;

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
    TransformD transform = jupiterSpace.frame.GetSurfaceTransform(
      latitude,
      longitude,
      bearing
    );

    frame = new InertialFrame(transform);
  }
}
