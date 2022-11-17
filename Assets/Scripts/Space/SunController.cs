
using UnityEngine;

/// <summary>
/// Controls the position of the sun
/// Do note that the parent scaled space is also rotated
/// </summary>
[DefaultExecutionOrder(-1)]
public class SunController : MonoBehaviour
{
  [SerializeField] OrbitController orbitController;

  void Update()
  {
    this.transform.localRotation = (Quaternion)orbitController.frame.GetSunRotation();
  }
}
