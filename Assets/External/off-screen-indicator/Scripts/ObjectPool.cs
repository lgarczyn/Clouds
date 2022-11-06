using System.Collections.Generic;
using UnityEngine;

namespace PixelPlay.OffScreenIndicator
{

class IndicatorObjectPool : MonoBehaviour
{
    [Tooltip("Assign the prefab.")]
    public Indicator pooledObject;
    [Tooltip("Initial pooled amount.")]
    public int pooledAmount = 1;
    [Tooltip("Should the pooled amount increase.")]
    public bool willGrow = true;

    List<Indicator> pooledObjects;

    void Start()
    {
        pooledObjects = new List<Indicator>();

        for (int i = 0; i < pooledAmount; i++)
        {
            Indicator indicator = Instantiate(pooledObject);
            indicator.transform.SetParent(transform, false);
            indicator.Activate(false);
            pooledObjects.Add(indicator);
        }
    }

    /// <summary>
    /// Gets pooled objects from the pool.
    /// </summary>
    /// <returns></returns>
    public Indicator GetPooledObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].Active)
            {
                return pooledObjects[i];
            }
        }
        if (willGrow)
        {
            Indicator indicator = Instantiate(pooledObject);
            indicator.transform.SetParent(transform, false);
            indicator.Activate(false);
            pooledObjects.Add(indicator);
            return indicator;
        }
        return null;
    }

    /// <summary>
    /// Deactive all the objects in the pool.
    /// </summary>
    public void DeactivateAllPooledObjects()
    {
        foreach (Indicator indicator in pooledObjects)
        {
            indicator.Activate(false);
        }
    }
}

}
