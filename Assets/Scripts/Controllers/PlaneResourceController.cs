using UnityEngine;

public class PlaneResourceController : MonoBehaviour
{
  public PlaneEntity plane;
  public PlaneDeathController deathController;
  public AnimationCurve energyVsDensity;

  [SerializeField][RequiredComponent] ResourceCalculatorBridge reqResourceCalculatorBridge;

  void FixedUpdate()
  {
    var resourceCalculator = reqResourceCalculatorBridge.instance;

    float light = resourceCalculator.GetLight();
    float density = resourceCalculator.GetDensity();

    if (light < 0f)
    {
      // TODO display error to client
      // If not on startup and resources still cannot be retrieved
      if (Time.time > 1) Debug.LogWarning("Resource controller cannot calculate resources");
      return;
    }

    plane.RefuelEnergy(energyVsDensity.Evaluate(density) * Time.fixedDeltaTime);
  }
}
