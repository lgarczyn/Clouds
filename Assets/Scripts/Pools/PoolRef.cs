using UnityEngine;

/// <summary>
/// Allows anyone to hold a serializable reference to an object pool
/// The pool must actually exist, and hold a unique reference to this object
/// </summary>
[CreateAssetMenu(menuName = "Project/PoolRef")]
public class PoolRef : ScriptableObject
{
  private IPool poolReference;

  [SerializeField] GameObject prefab;

  public IPool GetPool()
  {
    if (poolReference != null)
    {
      return poolReference;
    }
    throw new System.Exception("No pool was initialized for PoolRef");
  }

  public GameObject Get() {
    return GetPool().Get();
  }

  public T Get<T> () {
    return Get().GetComponent<T>()
      ?? throw new System.Exception("Component not found on new prefab instanciation");
  }

  public GameObject GetPrefab() {
    return prefab;
  }

  public void SetRef(IPool pool)
  {
    if (poolReference != null && pool != poolReference)
    {
      Debug.LogError("Pool ref is already initialized!", poolReference as Object);
    }
    if (pool != null) poolReference = pool;
    else throw new System.Exception("Cannot set null as a pool ref");

  }

  public void RemoveRef(IPool pool)
  {
    if (pool == poolReference) poolReference = null;
    else throw new System.Exception("Attempting to remove incorrect ref");
  }
}
