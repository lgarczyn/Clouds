
using UnityEngine;

/// <summary>
/// Controls the position of the sun
/// Do note that the parent scaled space is also rotated
/// </summary>
public class SunController : MonoBehaviour
{
  public OrbitController orbitController;

  void Update()
  {
    this.transform.localRotation = (Quaternion)orbitController.frame.GetSunRotation();
  }
}
