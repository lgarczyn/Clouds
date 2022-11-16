using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(WarningManagerBridge))]
public class PlaneDamageWarning : MonoBehaviour
{
  public void OnHealthChange(float health)
  {
    if (health < 0.2f) GetComponent<WarningManagerBridge>().WarnLowHealth();
  }
}
