using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Interface to pass the pool object to pool subjects
/// </summary>
public interface IPool
{
  void Release(GameObject item);
  GameObject Get();
}

/// <summary>
/// Component based implementation of the unity pool system
/// Must be overriden to provide prefab source
/// </summary>
public class PoolBehavior : MonoBehaviour, IPool, IObjectPool<GameObject>
{
  [SerializeField] protected uint initializationCount;
  [SerializeField] protected uint maxCount;
  protected uint active;

  public int Count { get { return CountActive + CountInactive; } }
  public int CountActive { get { return (int)active; } }
  public int CountInactive { get { return pool.CountInactive; } }


  IObjectPool<PoolSubject> poolInstance;

  protected virtual GameObject Prefab
  {
    get { throw new System.NotImplementedException(); }
  }

  protected IObjectPool<PoolSubject> pool
  {
    get
    {
      if (poolInstance == null) poolInstance = new ObjectPool<PoolSubject>(
      CreateItem,
      OnTakeFromPool,
      OnReturnToPool,
      OnDestroyObject,
      false,
      (int)initializationCount,
      (int)maxCount);

      return poolInstance;
    }
  }

  public void Clear()
  {
    pool.Clear();
  }

  protected virtual void OnTakeFromPool(PoolSubject subject)
  {
    subject.gameObject.SetActive(true);
    subject.transform.SetParent(transform, true);
    subject.parent = this;
  }

  protected virtual void OnReturnToPool(PoolSubject subject)
  {
    subject.gameObject.SetActive(false);
    subject.transform.SetParent(transform, true);
    subject.transform.position = Vector3.zero;
  }

  protected virtual void OnDestroyObject(PoolSubject subject)
  {
    Destroy(subject.gameObject);
  }

  protected virtual PoolSubject CreateItem()
  {
    GameObject instance = Instantiate(Prefab.gameObject);

    PoolSubject subject = instance.GetComponent<PoolSubject>();

    if (subject == null) subject = instance.AddComponent<PoolSubject>();

    subject.parent = this;
    instance.transform.SetParent(transform);

    InitItem(instance);

    return subject;
  }

  protected virtual void InitItem(GameObject item) { }

  public virtual GameObject Get()
  {
    PoolSubject subject = pool.Get();

    // Ensure subject is correctly initialized
    subject.parent = this;

    subject.gameObject.SetActive(true);

    active++;

    subject.onInit.Invoke();

    return subject.gameObject;
  }

  public virtual PooledObject<GameObject> Get(out GameObject item)
  {
    item = Get();
    return new PooledObject<GameObject>();
  }

  public virtual GameObject Get(Transform parent, Vector3 position)
  {
    GameObject instance = Get();
    instance.transform.SetParent(parent);
    instance.transform.localPosition = position;

    return instance;
  }

  public virtual void Release(GameObject instance)
  {
    PoolSubject subject = instance.GetComponent<PoolSubject>();

    if (active == 0)
    {
      Debug.LogWarning("More released than were produced", this);
    }
    else
    {
      active--;
    }

    if (!subject)
    {
      Debug.LogError("Cannot release subject-less object into pool", subject);
      return;
    }

    subject.onRelease.Invoke();

    pool.Release(subject);
  }

  void OnDisable()
  {
    this.pool.Clear();
  }
}
