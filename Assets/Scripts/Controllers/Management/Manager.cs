using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Manager<T> : MonoBehaviour
where T : Manager<T>
{
  static T managerInstance;

  void Awake()
  {
    if (managerInstance != null) Debug.LogError("Duplicate instance brige", managerInstance);
    managerInstance = this as T;
  }

  void OnDestroy()
  {
    if (managerInstance == this) managerInstance = null;
    else Debug.LogError("Bridge instance was change during lifetime", managerInstance);
  }

  public static T instance
  {
    get
    {
      if (managerInstance != null)
      {
        return managerInstance;
      }
      Debug.LogError("Target Manager was not found");
      return null;
    }
  }
}
