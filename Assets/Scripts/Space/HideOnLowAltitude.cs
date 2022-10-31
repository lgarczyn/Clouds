using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(LocalSpaceControllerBridge))]
public class HideOnLowAltitude : MonoBehaviour
{
  public double minHeight;

  // Update is called once per frame
  void Update()
  {
    var localSpace = GetComponent<LocalSpaceControllerBridge>().instance;
    GetComponent<Renderer>().enabled = localSpace.altitude > minHeight;
  }
}
