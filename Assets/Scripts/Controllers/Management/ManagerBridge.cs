using UnityEngine;

public abstract class ManagerBridge<T> : MonoBehaviour
where T : Manager<T>
{
  public T instance
  {
    get
    {
      return Manager<T>.instance;
    }
  }
}
