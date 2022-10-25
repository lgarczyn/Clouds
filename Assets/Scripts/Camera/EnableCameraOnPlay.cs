using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class EnableCameraOnPlay : MonoBehaviour
{
  void Start()
  {
    GetComponent<Camera>().enabled = true;
  }
}
