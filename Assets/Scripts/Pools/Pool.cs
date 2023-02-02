using UnityEngine;
using UnityEngine.Events;
using System.Linq;

/// <summary>
/// Scriptable-object based pool
/// Stores a private ref inside a scriptable object on init
/// Anyone with a reference to said object can request a pool item
/// TODO: handles multi-scene setup
/// </summary>
public class Pool : PoolBehavior
{
  [SerializeField] PoolRef poolRef;
  [Min(0)]
  [SerializeField] protected int initOnLoad = 10;

  override protected GameObject Prefab
  {
    get { return poolRef.GetPrefab(); }
  }

  // Set ref on game start and domain reload
  void OnEnable()
  {
    poolRef.SetRef(this);

    Enumerable.Range(0, (int)initOnLoad)
      .Select(i => poolRef.Get<PoolSubject>())
      .Do(s => s.Release());
  }

  void OnDisable()
  {
    poolRef.RemoveRef(this);
  }
}
