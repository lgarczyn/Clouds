using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MFlight.Demo.Plane))]
public class PlaneCheatController : MonoBehaviour
{
  public float baseSpeed = 60;
  public float shiftSpeedMultiplier = 10;
  public float controlSpeedMultiplier = 100;

  void Update()
  {
    GetComponent<MFlight.Demo.Plane>().thrust = baseSpeed;
    if (Input.GetKey(KeyCode.LeftShift)) GetComponent<MFlight.Demo.Plane>().thrust *= shiftSpeedMultiplier;
    if (Input.GetKey(KeyCode.LeftControl)) GetComponent<MFlight.Demo.Plane>().thrust *= controlSpeedMultiplier;
    if (Input.GetKeyDown(KeyCode.K)) GetComponent<Rigidbody>().position = Vector3.zero;
    if (Input.GetKeyDown(KeyCode.K)) GetComponent<Rigidbody>().velocity = Vector3.zero;
  }
}
