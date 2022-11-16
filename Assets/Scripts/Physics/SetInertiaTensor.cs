using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Overrides the Rigidbody inertiaTensor value on Start
/// Prevents automatic calculation from colliders
/// </summary>

public class SetInertiaTensor : MonoBehaviour
{
  /// <summary>
  /// Inertia tensor is a rotational analog of mass:
  /// the larger the inertia component about a particular axis is,
  /// the more torque that is required to achieve the same angular acceleration about that axis.
  /// </summary>
  public Vector3 inertiaTensor = new Vector3(1, 1, 1);

  [SerializeField][RequiredComponent] Rigidbody reqRigidbody;

  void Start()
  {
    reqRigidbody.inertiaTensor = inertiaTensor;
  }
}
