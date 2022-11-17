using UnityEngine;

/// <summary>
/// Manager singleton using the curiously recursive patterns
/// Managers inherit from it, and feed themselves as the T parameter
/// This allows the creation of a ManagerBridge, which allows other components
/// to explicitely declare a dependency on a manager
/// </summary>
/// <typeparam name="T"></typeparam>
[DefaultExecutionOrder(-2000)]
public class Manager<T> : MonoBehaviour, IManager<T>
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

  protected virtual void Awake()
  {
    if (realInstance != null) Debug.LogError("Duplicate Manager: " + typeof(T));
    realInstance = this as IManager<T>;
  }

  void OnDestroy()
  {
    if (!Object.ReferenceEquals(realInstance, this))
    {
      Debug.LogError("Manager instance was changed during lifetime: " + typeof(T));
    }
    realInstance = null;
  }

  public static T instance
  {
    get
    {
      try {
        if (realInstance != null && realInstance.isActiveAndEnabled)
        {
          return realInstance as T;
        }
      } catch (System.Exception e) {
        // Storing instance in interface prevents me from checking for destruction
        // This is an attempt at ensuring that I don't crash if object is destroyed
        Debug.Log(e, realInstance as UnityEngine.Object);
      }

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

  public static T tryInstance
  {
    get
    {
      return realInstance as T;
    }
  }
}
