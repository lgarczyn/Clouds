using UnityEngine;

public interface ITargetManager: IManager<ITargetManager> {
  public ITarget GetTarget();
}

public class TargetManager : Manager<ITargetManager>, ITargetManager
{
  [SerializeField] Target plane;

  public ITarget GetTarget()
  {
    return plane;
  }
}