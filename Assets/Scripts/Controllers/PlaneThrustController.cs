using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerPlane))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlaneEntity))]
[RequireComponent(typeof(PlaneResourceController))]
[RequireComponent(typeof(WarningManagerBridge))]
public class PlaneThrustController : MonoBehaviour
{
  public ResourceCalculator resourceCalculator;
  public float baseThrust = 60;
  public float baseDrag = 1f;
  public float dragMultiplier = 1f;
  public float boostMultiplier = 4;
  public float boostEnergyPerSecond = 10;

  public AnimationCurve densityVsThrust;

  public UnityEvent<float> thrustChange;

  [SerializeField][RequiredComponent] PlayerPlane reqPlayerPlane;
  [SerializeField][RequiredComponent] Rigidbody reqRigidbody;
  [SerializeField][RequiredComponent] PlaneEntity reqPlaneEntity;
  [SerializeField][RequiredComponent] PlaneResourceController reqPlaneResourceController;
  [SerializeField][RequiredComponent] WarningManagerBridge reqWarningManagerBridge;

  void Start() {
    baseDrag = reqRigidbody.drag;
  }

  public void OnThrust(InputAction.CallbackContext context)
  {
    throttle = context.ReadValue<float>();
  }

  public void OnBrake(InputAction.CallbackContext context)
  {
    brake = context.ReadValue<float>();
    // Debug.Log(throttle);
  }

  float throttle = 0f;
  float brake = 0f;

  void Update()
  {
    reqRigidbody.drag = baseDrag + dragMultiplier * brake;
    float density = -10;
    float currentMultiplier = boostMultiplier * throttle * (1 - brake);

    if (resourceCalculator)
    {
      density = resourceCalculator.GetDensity();
    }

    float multiplier = densityVsThrust.Evaluate(density);

    if (throttle > 0f)
    {
      float multDiff = Mathf.Abs(currentMultiplier - multiplier) / currentMultiplier;
      if (reqPlaneEntity.TrySpendEnergy(multDiff * boostEnergyPerSecond * Time.deltaTime))
      {
        multiplier = Mathf.Max(multiplier, currentMultiplier);
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
