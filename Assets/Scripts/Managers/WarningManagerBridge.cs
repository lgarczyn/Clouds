using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningManagerBridge : ManagerBridge<WarningManager>
{
  // Warning functions for use in UnityEvent editor window

  public void WarnEnemyLock() { instance.SendWarning(WarningType.EnemyLock); }
  public void WarnEnemySpawn() { instance.SendWarning(WarningType.EnemySpawn); }
  public void WarnLowShield() { instance.SendWarning(WarningType.LowShield); }
  public void WarnBrokenShield() { instance.SendWarning(WarningType.BrokenShield); }
  public void WarnLowHealth() { instance.SendWarning(WarningType.LowHealth); }
  public void WarnLowBoost() { instance.SendWarning(WarningType.LowBoost); }
  public void WarnRegeneratingShield() { instance.SendWarning(WarningType.RegeneratingShield); }
  public void WarnFullShield() { instance.SendWarning(WarningType.FullShield); }
}
