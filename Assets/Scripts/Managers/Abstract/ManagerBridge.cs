using UnityEngine;

public interface IDoNotUse {}

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

  // Hide some calls on the bridge that may be mistaken for implementation
  public new IDoNotUse transform => throw new System.NotImplementedException();
  public new IDoNotUse GetComponent<U>() => throw new System.NotImplementedException();

  // absence of new errors on editor
  // presence of new errors on build
  // so I have to check for that
  #if UNITY_EDITOR
  new
#endif
  public IDoNotUse rigidbody => throw new System.NotImplementedException();
#if UNITY_EDITOR
  new
#endif
  public IDoNotUse camera => throw new System.NotImplementedException();
}
