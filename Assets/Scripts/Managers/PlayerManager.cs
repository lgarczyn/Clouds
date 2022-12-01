using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerPlane))]
public class PlayerManager : Manager<PlayerManager>
{
  [SerializeField][RequiredComponent] Rigidbody reqRigidbody;

  [SerializeField][RequiredComponent] PlayerPlane reqPlayerPlane;

  public Rigidbody playerRigidbody => reqRigidbody;

  public PlayerPlane playerPlane => reqPlayerPlane;

  public Transform playerTransform => transform;

  public float Distance(Vector3 position)
  {
    return Vector3.Distance(position, transform.position);
  }
}
