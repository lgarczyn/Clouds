using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerPlane))]
[RequireComponent(typeof(PlaneEntity))]
public class PlayerManager : Manager<PlayerManager>
{
  [SerializeField][RequiredComponent] Rigidbody reqRigidbody;

  [SerializeField][RequiredComponent] PlayerPlane reqPlayerPlane;
  [SerializeField][RequiredComponent] PlaneEntity reqPlaneEntity;

  public Rigidbody playerRigidbody => reqRigidbody;

  public PlaneEntity planeEntity => reqPlaneEntity;
  public PlayerPlane playerPlane => reqPlayerPlane;

  public Transform playerTransform => transform;

  public float Distance(Vector3 position)
  {
    return Vector3.Distance(position, transform.position);
  }
}
