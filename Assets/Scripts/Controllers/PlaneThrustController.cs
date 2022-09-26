using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerPlane))]
[RequireComponent(typeof(Plane))]
public class PlaneThrustController : MonoBehaviour
{
  public ResourceCalculator resourceCalculator;
  public float baseThrust = 60;

  public AnimationCurve densityVsThrust;
  public AnimationCurve thrustMultVsTrailWidth;

  public List<TrailRenderer> trails;

  void Update()
  {
    float density = -10;

    if (resourceCalculator)
    {
      density = resourceCalculator.GetDensity();
    }

    PlayerPlane plane = GetComponent<PlayerPlane>();
    float multiplier = densityVsThrust.Evaluate(density);
    plane.thrust = baseThrust * multiplier;

    foreach (TrailRenderer trail in trails)
    {
      trail.widthMultiplier = thrustMultVsTrailWidth.Evaluate(multiplier);
    }
  }
}
