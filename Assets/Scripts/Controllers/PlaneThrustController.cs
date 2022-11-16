using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerPlane))]
[RequireComponent(typeof(Plane))]
[RequireComponent(typeof(PlaneResourceController))]
[RequireComponent(typeof(WarningManagerBridge))]
public class PlaneThrustController : MonoBehaviour
{
  public ResourceCalculator resourceCalculator;
  public float baseThrust = 60;
  public float boostMultiplier = 4;
  public float boostEnergyPerSecond = 10;

  public AnimationCurve densityVsThrust;

  public UnityEvent<float> thrustChange;

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
      else
      {
        GetComponent<WarningManagerBridge>().WarnLowBoost();
      }
    }

    plane.thrust = baseThrust * multiplier;

    thrustChange.Invoke(multiplier / boostMultiplier * 100f);
  }
}
