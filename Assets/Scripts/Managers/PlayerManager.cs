using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Manager<PlayerManager>
{

  [SerializeField][RequiredComponent] Rigidbody reqRigidbody;

  public Rigidbody playerRigidbody => reqRigidbody;

  public Transform playerTransform => transform;

  public float Distance(Vector3 position)
  {
    return Vector3.Distance(position, transform.position);
  }
}
