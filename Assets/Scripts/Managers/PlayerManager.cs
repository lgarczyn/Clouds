using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerManager : Manager<PlayerManager>
{
  public Rigidbody playerRigidbody
  {
    get
    {
      return GetComponent<Rigidbody>();
    }
  }

  public Transform playerTransform
  {
    get
    {
      return transform;
    }
  }

  public float Distance(Vector3 position) {
    return Vector3.Distance(position, transform.position);
  }
}
