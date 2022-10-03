using System;
using UnityEngine;
using System.Linq;

/// <summary>
/// Vector helpers.
/// </summary>
public static class PhysicsHelper
{
  static ContactPoint[] cache;

  public struct AverageContactPoint
  {
    //     The average point of contact.
    public Vector3 point;
    //     Average normal of the contact points.
    public Vector3 normal;
    //     The average impulse applied to contact pairs to resolve the collision.
    public Vector3 impulse;
    //     The most common collider in contact
    public Collider thisCollider;
    //     The most common other collider in contact
    public Collider otherCollider;
    //     The avverage distance between the colliders at the contact points.
    public float separation;
  }

  public static AverageContactPoint GetAverageContact(this Collision info)
  {
    if (cache == null || info.contactCount > cache.Length)
      cache = new ContactPoint[info.contactCount * 2];

    int count = info.GetContacts(cache);

    AverageContactPoint returnValue;

    returnValue.impulse = cache.Take(count).Select(p => p.impulse).Average();
    returnValue.point = cache.Take(count).Select(p => p.point).Average();
    returnValue.normal = cache.Take(count).Select(p => p.normal).Average();
    returnValue.separation = cache.Take(count).Select(p => p.separation).Average();

    returnValue.thisCollider = cache.Take(count)
        .Select(p => p.thisCollider)
        .GroupBy(i => i)
        .OrderByDescending(grp => grp.Count())
        .Select(grp => grp.Key).First();

    returnValue.otherCollider = cache.Take(count)
        .Select(p => p.otherCollider)
        .GroupBy(i => i)
        .OrderByDescending(grp => grp.Count())
        .Select(grp => grp.Key).First();

    return returnValue;
  }
}