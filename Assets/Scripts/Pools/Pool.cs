using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Scriptable-object based pool
/// Stores a private ref inside a scriptable object on init
/// Anyone with a reference to said object can request a pool item
/// TODO: handles multi-scene setup
/// </summary>
public class Pool : PoolBehavior
{
  [SerializeField] PoolRef poolRef;

  override protected GameObject Prefab
  {
    get { return poolRef.GetPrefab(); }
  }
  
  // Set ref on game start and domain reload
  void OnEnable()
  {
    poolRef.SetRef(this);
  }

  void OnDisable()
  {
    poolRef.RemoveRef(this);
  }
}
