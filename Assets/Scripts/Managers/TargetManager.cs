using UnityEngine;

public class TargetManager : Manager<TargetManager>
{
  public Target plane;

  public Target GetTarget()
  {
    return plane;
  }
}