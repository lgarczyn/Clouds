using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine;

public abstract class ResourceCalculator : MonoBehaviour
{
  public abstract float GetLight();
  public abstract float GetDensity();
}

public class PlaneResourceController : MonoBehaviour
{
  public PlaneEntity plane;
  public ResourceCalculator resourceCalculator;

  public PlaneDeathController deathController;
  public AnimationCurve energyVsDensity;

  void FixedUpdate()
  {
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
