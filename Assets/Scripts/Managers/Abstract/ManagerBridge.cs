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
  public new IDoNotUse rigidbody => throw new System.NotImplementedException();
  public new IDoNotUse camera => throw new System.NotImplementedException();
  public new IDoNotUse GetComponent<U>() => throw new System.NotImplementedException();
}
