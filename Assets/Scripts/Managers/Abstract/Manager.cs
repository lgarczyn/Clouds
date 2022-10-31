using UnityEngine;

/// <summary>
/// Manager singleton using the curiously recursive patterns
/// Managers inherit from it, and feed themselves as the T parameter
/// This allows the creation of a ManagerBridge, which allows other components
/// to explicitely declare a dependency on a manager
/// </summary>
/// <typeparam name="T"></typeparam>
[DefaultExecutionOrder(-2000)]
public class Manager<T> : ManagerBridge<T>, IManager<T>
where T : class, IManager<T>
{
  static IManager<T> managerInstance;

  // This fixes the issue of Manager<T> and Manager<IT> not using the same instance
  static IManager<T> realInstance
  {
    get
    {
      return Manager<T>.managerInstance;
    }
    set
    {
      Manager<T>.managerInstance = value;
    }
  }

  void Awake()
  {
    if (realInstance != null) Debug.LogError("Duplicate Manager: " + typeof(T));
    realInstance = this as IManager<T>;
  }

  void OnDestroy()
  {
    if (Object.ReferenceEquals(realInstance, this)) realInstance = null;
    else Debug.LogError("Manager instance was changed during lifetime: " + typeof(T));
  }

  public new static T instance
  {
    get
    {
      if (realInstance != null)
      {
        return realInstance as T;
      }

      Debug.Log("Manager was not found, attempting search: " + typeof(T));
      Manager<T> newInstance = FindObjectOfType<Manager<T>>();
      if (newInstance != null)
      {
        realInstance = newInstance;
        return realInstance as T;
      }

      Debug.LogError("Could not find manager: " + typeof(T));
      return null;
    }
  }

  public new static T tryInstance
  {
    get
    {
      return realInstance as T;
    }
  }
}
