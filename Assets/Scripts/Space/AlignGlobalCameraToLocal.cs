using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Force the camera in scaled space to exactly mirror the main game camera in local space
/// Since the camera in scaled space renders the rings, planet and skybox, any lag will be perceived
/// </summary>
public class AlignGlobalCameraToLocal : MonoBehaviour
{
  public Camera target;

  public LocalSpaceController LocalSpaceController;

  public ScaledSpaceController scaledSpaceController;


  // Update is called once per frame
  void LateUpdate()
  {
    TransformD originPos =
        LocalSpaceController.frame.fromLocalTransform(target.transform);

    TransformD scaledSpace =
        scaledSpaceController.frame.toGlobalCoord(originPos);

    this.transform.localRotation = (Quaternion)scaledSpace.rotation;
    this.transform.localPosition = (Vector3)scaledSpace.position;
  }
}
