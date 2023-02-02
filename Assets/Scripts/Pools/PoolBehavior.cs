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
  [Min(1)]
  [SerializeField] protected uint maxCount = 1000;
  protected uint active;

  public int CountAll { get { return pool.CountAll; } }
  public int CountActive { get { return pool.CountActive; } }
  public int CountInactive { get { return pool.CountInactive; } }


  ObjectPool<PoolSubject> poolInstance;

  protected virtual GameObject Prefab
  {
    get { throw new System.NotImplementedException(); }
  }

  protected virtual ObjectPool<PoolSubject> CreatePool() {
    return new ObjectPool<PoolSubject>(
      CreateItem,
      OnTakeFromPool,
      OnReturnToPool,
      OnDestroyObject,
      true,
      0,
      (int)maxCount);
  }

  protected ObjectPool<PoolSubject> pool
  {
    get
    {
      EnsureLoaded();

      return poolInstance;
    }
  }

  public void Clear()
  {
    pool.Clear();
  }

  public void EnsureLoaded() {
    if (poolInstance == null) poolInstance = CreatePool();
  }

  protected virtual void OnTakeFromPool(PoolSubject subject)
  {
    // null/destroyed objects will be caught later in PoolBehavior.Get
    if (subject == null) return;
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
    PoolSubject subject;

    do {
      subject = pool.Get();
      if (subject == null) Debug.LogWarning("Retrieved object has already been deleted", this);
    }
    while (subject == null);

    // Ensure subject is correctly initialized
    subject.parent = this;

    subject.gameObject.SetActive(true);

    active++;

    subject.onInit?.Invoke();

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

    subject.onRelease?.Invoke();

    pool.Release(subject);
  }

  void OnDisable()
  {
    this.pool.Clear();
  }
}
