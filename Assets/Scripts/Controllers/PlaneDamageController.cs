using UnityEngine;

public class PlaneDamageController : MonoBehaviour
{
  public PlaneEntity plane;

  public ResourceCalculator resourceCalculator;

  public float bodyCollisionDamage = 50f;
  public float collisionDps = 10f;
  public float minDepth = -1000f;
  public float pressureDpsPerMeter = 0.01f;

  public float densityDamageMultiplier = 1f;

  public AnimationCurve damageVsDensity;

  void OnCollisionStay(Collision collision)
  {
    plane.Damage(collisionDps * Time.fixedDeltaTime, false);
  }

  void OnTriggerEnter()
  {
    plane.Damage(bodyCollisionDamage, true);
  }

  void FixedUpdate()
  {

    float extraDepth = minDepth - plane.transform.position.y;

    if (extraDepth > 0f)
    {
      plane.Damage(extraDepth * pressureDpsPerMeter * Time.fixedDeltaTime, false);
    }

    float density = resourceCalculator.GetDensity();

    if (density < 0f)
      return;

    float damage = damageVsDensity.Evaluate(density);
    plane.Damage(density * Time.fixedDeltaTime * densityDamageMultiplier, false);
  }
}
