using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MFlight.Demo.Plane))]
[RequireComponent(typeof(Rigidbody))]
public class PlaneCheatController : MonoBehaviour
{
  public float baseSpeed = 60;
  public float shiftSpeedMultiplier = 7;
  public float controlSpeedMultiplier = 40;

  void Update()
  {
    MFlight.Demo.Plane plane = GetComponent<MFlight.Demo.Plane>();
    Rigidbody rigidbody = GetComponent<Rigidbody>();

    plane.thrust = baseSpeed;
    if (Input.GetKey(KeyCode.LeftShift)) plane.thrust *= shiftSpeedMultiplier;
    if (Input.GetKey(KeyCode.LeftControl)) plane.thrust *= controlSpeedMultiplier;
    if (Input.GetKeyDown(KeyCode.L)) rigidbody.isKinematic = !rigidbody.isKinematic;
    if (Input.GetKeyDown(KeyCode.K))
    {
      rigidbody.position = Vector3.zero;
      if (!rigidbody.isKinematic) rigidbody.velocity = Vector3.zero;
    }
  }
}
