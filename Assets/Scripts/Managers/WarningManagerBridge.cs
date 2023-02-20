using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningManagerBridge : ManagerBridge<WarningManager>
{
  // Warning functions for use in UnityEvent editor window

  public void TrySendWarning(WarningType type) {
    if (tryInstance)
      tryInstance.SendWarning(type);
    else
      Debug.Log("Sent Warning: " + System.Enum.GetName(typeof(WarningType), type));
  }

  public void WarnEnemyLock() { TrySendWarning(WarningType.EnemyLock); }
  public void WarnEnemySpawn() { TrySendWarning(WarningType.EnemySpawn); }
  public void WarnLowShield() { TrySendWarning(WarningType.LowShield); }
  public void WarnBrokenShield() { TrySendWarning(WarningType.BrokenShield); }
  public void WarnLowHealth() { TrySendWarning(WarningType.LowHealth); }
  public void WarnLowBoost() { TrySendWarning(WarningType.LowBoost); }
  public void WarnLowLaser() { TrySendWarning(WarningType.LowLaser); }
  public void WarnRegeneratingShield() { TrySendWarning(WarningType.RegeneratingShield); }
  public void WarnFullShield() { TrySendWarning(WarningType.FullShield); }
}
