using UnityEngine;

public class PlaneRepairController : MonoBehaviour
{
  public PlaneEntity plane;
  public float repairPerSecond = 1f;
  public float delayBeforeHeal = 10f;

  [SerializeField][RequiredComponent] WarningManagerBridge reqWarningManagerBridge;

  private float timeSinceLastDamage = float.PositiveInfinity;

  public void OnDamage()
  {
    timeSinceLastDamage = 0;
  }

  void FixedUpdate()
  {
    timeSinceLastDamage += Time.fixedDeltaTime;

    if (timeSinceLastDamage > delayBeforeHeal && !plane.shieldFull)
    {
      float repairs = repairPerSecond * Time.fixedDeltaTime;
      plane.RefuelShield(repairs);

      if (plane.shieldFull)
        reqWarningManagerBridge.WarnFullShield();
      else
        reqWarningManagerBridge.WarnRegeneratingShield();
    }
  }
}
