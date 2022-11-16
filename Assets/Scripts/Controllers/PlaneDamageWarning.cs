using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlaneDamageWarning : MonoBehaviour
{

  [SerializeField][RequiredComponent] WarningManagerBridge reqWarningManagerBridge;

  public void OnHealthChange(float health)
  {
    if (health < 0.2f) reqWarningManagerBridge.WarnLowHealth();
  }
}
