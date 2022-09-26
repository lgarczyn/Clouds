using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CopyCameraFov : MonoBehaviour
{
  public Camera target;

  void LateUpdate()
  {
    Camera camera = GetComponent<Camera>();

    camera.fieldOfView = target.fieldOfView;
  }
}
