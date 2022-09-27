using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FaunaType
{
  public GameObject copy;
  public int population;
  [HideInInspector]
  public List<GameObject> pool;
}

public class FaunaSpawner : MonoBehaviour
{
  public List<FaunaType> types;

  private void Start()
  {
    SpawnAndDestroy();
  }

  private void Update()
  {
    SpawnAndDestroy();
  }

  public void Reset()
  {
    foreach (FaunaType type in types)
    {
      int oldPop = type.population;
      type.population = 0;
      SpawnAndDestroy();
      type.population = oldPop;
    }
  }

  private void SpawnAndDestroy()
  {
    foreach (FaunaType type in types)
    {
      // disable the copy used for spawning
      type.copy.SetActive(false);

      // check the pool is instanciated
      if (type.pool == null) type.pool = new List<GameObject>();

      // Add copies as needed
      for (int j = type.pool.Count; j < type.population; j++)
      {
        GameObject instance = GameObject.Instantiate(type.copy);
        instance.transform.SetParent(transform);
        instance.SetActive(true);
        type.pool.Add(instance);
      }

      // Remove copies as needed
      for (int j = type.pool.Count - 1; j >= type.population; j--)
      {
        GameObject.Destroy(type.pool[j]);
        type.pool.RemoveAt(j);
      }
    }
  }
}
