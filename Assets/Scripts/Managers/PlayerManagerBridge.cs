using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManagerBridge : ManagerBridge<PlayerManager>
{
  public Rigidbody playerRigidbody => instance.playerRigidbody;

  public Transform playerTransform => instance.playerTransform;

  public float Distance(Vector3 position) { return instance.Distance(position); }
}
