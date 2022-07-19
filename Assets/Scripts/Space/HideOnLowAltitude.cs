using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HideOnLowAltitude : MonoBehaviour
{
  public LocalSpaceController localSpace;
  public double minHeight;

  // Update is called once per frame
  void Update()
  {
    if (localSpace == null) return;
    GetComponent<Renderer>().enabled = localSpace.altitude > minHeight;
  }
}
