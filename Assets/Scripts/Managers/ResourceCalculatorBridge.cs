using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCalculatorBridge : ManagerBridge<ResourceCalculator>, IResourceCalculator
{
  public float GetLight()
  {
    return Manager<ResourceCalculator>.instance.GetLight();
  }

  public float GetDensity()
  {
    return Manager<ResourceCalculator>.instance.GetDensity();
  }
}
