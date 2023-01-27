using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Script allowing a pooled object to self-release
/// Also allows the object to reset itself on release/init
/// </summary>
public class PoolSubject : MonoBehaviour
{
    public PoolBehavior parent;
    public UnityEvent onInit;
    public UnityEvent onRelease;

    public void Release()
    {
        parent.Release(gameObject);
    }

    public void Release(float time)
    {
        StartCoroutine(ReleaseCoroutine(time));
    }

    public IEnumerator ReleaseCoroutine(float time) {
        yield return new WaitForSeconds(time);
        Release(); 
    } 
}
