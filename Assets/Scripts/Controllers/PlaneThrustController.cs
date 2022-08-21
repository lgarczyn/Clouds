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

  public float trailWidthMultiplier = 0.1f;
  public List<TrailRenderer> trails;

  void Start()
  {
    PlayerPlane plane = GetComponent<PlayerPlane>();
  }

  void Update()
  {
    float multiplier = 1;

    if (resourceCalculator)
    {
      float density = resourceCalculator.GetDensity();
      multiplier = densityVsThrust.Evaluate(density);
    }

    PlayerPlane plane = GetComponent<PlayerPlane>();
    plane.thrust = baseThrust * multiplier;

    foreach (TrailRenderer trail in trails)
    {
      trail.widthMultiplier = multiplier * multiplier * trailWidthMultiplier;
    }
  }
}
