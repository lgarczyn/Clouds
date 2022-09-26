using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerPlane))]
[RequireComponent(typeof(Plane))]
[RequireComponent(typeof(PlaneResourceController))]
public class PlaneThrustController : MonoBehaviour
{
  public ResourceCalculator resourceCalculator;
  public float baseThrust = 60;
  public float boostMultiplier = 4;
  public float boostEnergyPerSecond = 10;

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
    PlaneEntity planeEntity = GetComponent<PlaneEntity>();

    float multiplier = densityVsThrust.Evaluate(density);

    if (Input.GetKey(KeyCode.LeftShift))
    {
      float multDiff = Mathf.Abs(boostMultiplier - multiplier) / boostMultiplier;
      if (planeEntity.TrySpendEnergy(multDiff * boostEnergyPerSecond * Time.deltaTime))
      {
        multiplier = Mathf.Max(multiplier, boostMultiplier);
      }
    }

    plane.thrust = baseThrust * multiplier;

    foreach (TrailRenderer trail in trails)
    {
      trail.widthMultiplier = thrustMultVsTrailWidth.Evaluate(multiplier);
    }
  }
}
