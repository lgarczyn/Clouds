using UnityEngine;

public class JupiterSpace : Frame
{

  public const double JUPITER_DISTANCE_FROM_SUN = 149_550_000_000; // Actually Earth's orbit
  public const double JUPITER_ORBIT_PERIOD = 365.256 * 24 * 60 * 60; // Actually Earth's orbit

  public const double JUPITER_REVOLUTION_PERIOD = 10 * 60 * 60;

  public const double JUPITER_RADIUS = 69_911_000;

  public const double JUPITER_AXIAL_TILT = 3.13;

  public void UpdatePos(double time)
  {
    Vector3D orbitalPosition =
      // The rotation for Jupiter's orbit
      QuaternionD.AngleAxis(360.0 * time / JUPITER_ORBIT_PERIOD, Vector3D.up)
      // The starting position of Jupiter
      * (Vector3D.forward * JUPITER_DISTANCE_FROM_SUN);

    QuaternionD axialTilt = QuaternionD.AngleAxis(
      JUPITER_AXIAL_TILT,
      Vector3D.right
    );

    // TODO: remove "+ 0.5" which is used here simply to start the game during the day
    QuaternionD revolution = QuaternionD.AngleAxis(
      360.0 * (time / JUPITER_REVOLUTION_PERIOD + 0.5),
      Vector3D.up
    );

    this.transform = new TransformD(
      orbitalPosition,
      axialTilt * revolution,
      1
    );
  }

  public QuaternionD GetSunRotation()
  {
    // The sun is on the zero position
    Vector3D sunPosAbsolute = Vector3D.zero;
    // Find the relative position to Jupiter
    Vector3D sunPosRelative = this.toLocalPos(sunPosAbsolute);
    // Create a rotation for the directional light, aligning with the sun's rays
    return QuaternionD.LookRotation(-sunPosRelative);
  }

  /// <summary>
  /// Transform a coordinate quaternion to a point on the surface of Jupiter in Jupiter space
  /// </summary>
  /// <param name="latLong">
  /// The rotation representation of the coordinates, which transforms a forward aligned vector
  /// into the local "up" vector
  /// </param>
  /// <returns> A 1:1 scale point on the surface of Jupiter </returns>
  public Vector3D GetSurfacePoint(QuaternionD latLong)
  {
    // Remove 1.1 once jupiter is precise enough to avoid clipping
    return latLong * (Vector3D.forward * JUPITER_RADIUS * 1.1);
  }

  /// <summary>
  /// Transform a coordinate quaternion to a transform on the surface of jupiter, aligned to the horizon
  /// </summary>
  /// <param name="latLong">
  /// The rotation representation of the coordinates, which transforms a forward aligned vector
  /// into the local "up" vector
  /// </param>
  /// <returns> A 1:1 scale transform on the surface of Jupiter </returns>
  public TransformD GetSurfaceTransform(QuaternionD latLong)
  {
    return new TransformD(
      GetSurfacePoint(latLong),
      latLong * QuaternionD.AngleAxis(90, Vector3D.right)
    );
  }

  /// <summary>
  /// Transform GCS coordinate to  a point on the surface of Jupiter in Jupiter space
  /// </summary>
  /// <param name="latitude">The latitude coordinate</param>
  /// <param name="longitude">The longitude coordinate</param>
  /// <param name="bearing">The "bearing", or angle around the local "up" axis</param>
  /// <returns> A 1:1 scale point on the surface of Jupiter </returns>
  public Vector3D GetSurfacePoint(double latitude, double longitude)
  {
    return GetSurfacePoint(GcsToQuaternion(latitude, longitude));
  }

  /// <summary>
  /// Transform GCS coordinates into a transform on the surface of jupiter, aligned to the horizon
  /// </summary>
  /// <param name="latitude">The latitude coordinate</param>
  /// <param name="longitude">The longitude coordinate</param>
  /// <param name="bearing">The "bearing", or angle around the local "up" axis</param>
  /// <returns> A 1:1 scale transform on the surface of Jupiter </returns>
  public TransformD GetSurfaceTransform(double latitude, double longitude, double bearing = 0)
  {
    return GetSurfaceTransform(GcsToQuaternion(latitude, longitude, bearing));
  }

  /// <summary>
  /// Transform coordinates to a point on the surface of Jupiter in Jupiter space
  /// </summary>
  /// <param name="latitude">The latitude coordinate</param>
  /// <param name="longitude">The longitude coordinate</param>
  /// <param name="bearing">The "bearing", or angle around the local "up" axis</param>
  /// <returns></returns>
  public QuaternionD GcsToQuaternion(double latitude, double longitude, double bearing = 0)
  {
    return QuaternionD.AngleAxis(longitude, Vector3.up) *
      QuaternionD.AngleAxis(latitude, Vector3.right) *
      QuaternionD.AngleAxis(bearing, Vector3.forward);
  }
}