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

    QuaternionD revolution = QuaternionD.AngleAxis(
      360.0 * time / JUPITER_REVOLUTION_PERIOD,
      Vector3D.up
    );

    this.parameters = new TransformD(
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

  public QuaternionD GetSkyboxRotation()
  {
    // Take the default rotation of the solar system
    QuaternionD posAbsolute = QuaternionD.identity;
    // Find the relative rotation from Jupiter's point of view
    QuaternionD sunPosRelative = this.toLocalRot(posAbsolute);
    // Return that rotation
    return sunPosRelative;
  }

  public Vector3D GetSurfacePoint(QuaternionD latLong)
  {
    // Remove 1.1 once jupiter is precise enough to avoid clipping
    return latLong * (Vector3D.forward * JUPITER_RADIUS * 1.1);
  }

  public TransformD GetSurfaceTransform(QuaternionD latLong, double angle)
  {
    return new TransformD(
      GetSurfacePoint(latLong),
      latLong * QuaternionD.AngleAxis(90, Vector3D.right) * QuaternionD.AngleAxis(angle, Vector3D.up)
    );
  }
}