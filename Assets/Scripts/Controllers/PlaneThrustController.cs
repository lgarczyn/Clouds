using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlaneThrustController : MonoBehaviour
{
  public ResourceCalculator resourceCalculator;
  public float baseThrust = 60;
  public float boostMultiplier = 4;
  public float boostEnergyPerSecond = 10;

  public AnimationCurve densityVsThrust;

  public UnityEvent<float> thrustChange;

  [SerializeField][RequiredComponent] PlayerPlane reqPlayerPlane;
  [SerializeField][RequiredComponent] PlaneEntity reqPlaneEntity;
  [SerializeField][RequiredComponent] PlaneResourceController reqPlaneResourceController;
  [SerializeField][RequiredComponent] WarningManagerBridge reqWarningManagerBridge;

  void Update()
  {
    float density = -10;

    if (resourceCalculator)
    {
      density = resourceCalculator.GetDensity();
    }

    float multiplier = densityVsThrust.Evaluate(density);

    if (Keyboard.current.shiftKey.isPressed)
    {
      float multDiff = Mathf.Abs(boostMultiplier - multiplier) / boostMultiplier;
      if (reqPlaneEntity.TrySpendEnergy(multDiff * boostEnergyPerSecond * Time.deltaTime))
      {
        multiplier = Mathf.Max(multiplier, boostMultiplier);
      }
      else
      {
        reqWarningManagerBridge.WarnLowBoost();
      }
    }

    reqPlayerPlane.thrust = baseThrust * multiplier;

    thrustChange.Invoke(multiplier / boostMultiplier * 100f);
  }
}
