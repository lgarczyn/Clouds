using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlaneThrustController))]
[RequireComponent(typeof(Rigidbody))]
public class PlaneCheatController : MonoBehaviour
{
  public float shiftSpeedMultiplier = 7;
  public float controlSpeedMultiplier = 40;

  [HideInInspector]
  public float baseSpeed = 60;
  [HideInInspector]
  public Vector3 startPos = Vector3.zero;
  [HideInInspector]
  public Quaternion startRotation = Quaternion.identity;

  void Start()
  {
    PlaneThrustController thrustController = GetComponent<PlaneThrustController>();
    Rigidbody rigidbody = GetComponent<Rigidbody>();

    baseSpeed = thrustController.baseThrust;
    startPos = rigidbody.position;
    startRotation = rigidbody.rotation;
  }

  void Update()
  {
    PlaneThrustController thrustController = GetComponent<PlaneThrustController>();
    Rigidbody rigidbody = GetComponent<Rigidbody>();

    thrustController.baseThrust = baseSpeed;
    if (Input.GetKey(KeyCode.LeftShift)) thrustController.baseThrust *= shiftSpeedMultiplier;
    if (Input.GetKey(KeyCode.LeftControl)) thrustController.baseThrust *= controlSpeedMultiplier;
    if (Input.GetKeyDown(KeyCode.L)) rigidbody.isKinematic = !rigidbody.isKinematic;
    if (Input.GetKeyDown(KeyCode.K))
    {
      rigidbody.position = startPos;
      rigidbody.rotation = startRotation;
      if (!rigidbody.isKinematic)
      {
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
      }
    }
  }
}
