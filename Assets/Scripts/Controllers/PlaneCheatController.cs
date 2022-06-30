using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MFlight.Demo.Plane))]
public class PlaneCheatController : MonoBehaviour
{
  public float shiftSpeedMultiplier = 10;
  public float controlSpeedMultiplier = 100;

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.LeftShift)) GetComponent<MFlight.Demo.Plane>().thrust *= shiftSpeedMultiplier;
    if (Input.GetKeyUp(KeyCode.LeftShift)) GetComponent<MFlight.Demo.Plane>().thrust /= shiftSpeedMultiplier;
    if (Input.GetKeyDown(KeyCode.LeftControl)) GetComponent<MFlight.Demo.Plane>().thrust *= controlSpeedMultiplier;
    if (Input.GetKeyUp(KeyCode.LeftControl)) GetComponent<MFlight.Demo.Plane>().thrust /= controlSpeedMultiplier;
  }
}
