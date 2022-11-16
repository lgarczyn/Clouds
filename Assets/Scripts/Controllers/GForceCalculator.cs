using UnityEngine;

public class GForceCalculator : Manager<GForceCalculator>
{

  [SerializeField][RequiredComponent] Rigidbody reqRigidbody;

  float gForce;

  Vector3 lastVelocity;

  public float GetGForce()
  {
    return gForce;
  }

  void FixedUpdate()
  {
    Vector3 velocity = reqRigidbody.velocity;

    Vector3 diff = velocity - lastVelocity;
    Vector3 scaledDiff = diff / Time.fixedDeltaTime;
    Vector3 correctedDiff = scaledDiff - Physics.gravity;
    gForce = correctedDiff.magnitude / 9.8f;

    lastVelocity = velocity;
  }
}
