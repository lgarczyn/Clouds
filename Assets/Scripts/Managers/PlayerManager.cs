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
      return GetComponent<Transform>();
    }
  }
}
