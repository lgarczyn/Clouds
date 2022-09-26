using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GForceCalculator : MonoBehaviour
{
  float gForce;

  Vector3 lastVelocity;

  public float GetGForce()
  {
    return gForce;
  }

  void FixedUpdate()
  {
    Vector3 velocity = GetComponent<Rigidbody>().velocity;

    Vector3 diff = velocity - lastVelocity;
    Vector3 scaledDiff = diff / Time.fixedDeltaTime;
    Vector3 correctedDiff = scaledDiff - Physics.gravity;
    gForce = correctedDiff.magnitude / 9.8f;

    lastVelocity = velocity;
  }
}
