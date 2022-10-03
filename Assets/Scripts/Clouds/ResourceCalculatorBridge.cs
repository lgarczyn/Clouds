using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCalculatorBridge : ResourceCalculator
{
  public ResourceCalculator source;

  public override float GetLight()
  {
    return source.GetLight();
  }

  public override float GetDensity()
  {
    return source.GetDensity();
  }
}
