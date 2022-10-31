using UnityEngine;

/// <summary>
/// Component that allows other components to hold a reference to a manager
/// Allows to explicitely declare a dependency
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ManagerBridge<T> : MonoBehaviour
where T : class, IManager<T>
{
  public T instance
  {
    get
    {
      return Manager<T>.instance as T;
    }
  }

  public T tryInstance
  {
    get
    {
      return Manager<T>.tryInstance as T;
    }
  }
}
