using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Type
{
  public GameObject copy;
  public int population;
  [HideInInspector]
  public List<GameObject> pool;
}

public class FaunaSpawner : MonoBehaviour
{
  public List<Type> types;

  private void Start()
  {
    SpawnAndDestroy();
  }

  private void Update()
  {
    SpawnAndDestroy();
  }

  void SpawnAndDestroy()
  {
    foreach (Type type in types)
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
