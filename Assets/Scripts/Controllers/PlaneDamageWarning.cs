using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlaneDamageWarning : MonoBehaviour
{
  public void OnHealthChange(float health)
  {
    if (health < 0.2f) WarningManager.instance.SendWarning(WarningType.LowHealth);
  }
}
