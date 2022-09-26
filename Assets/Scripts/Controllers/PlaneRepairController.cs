using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneRepairController : MonoBehaviour
{
  public PlaneEntity plane;
  public float repairPerSecond = 1f;
  public float delayBeforeHeal = 10f;

  private float timeSinceLastDamage = float.PositiveInfinity;

  public void OnDamage()
  {
    timeSinceLastDamage = 0;
  }

  void FixedUpdate()
  {
    timeSinceLastDamage += Time.fixedDeltaTime;

    if (timeSinceLastDamage > delayBeforeHeal)
    {
      float repairs = repairPerSecond * Time.fixedDeltaTime;
      plane.Repair(repairs);
    }
  }
}
