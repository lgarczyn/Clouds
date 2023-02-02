using UnityEngine;

/// <summary>
/// Force the camera in scaled space to exactly mirror the main game camera in local space
/// Since the camera in scaled space renders the rings, planet and skybox, any lag will be perceived
/// </summary>
public class AlignGlobalCameraToLocal : MonoBehaviour
{
  [SerializeField][RequiredComponent] ScaledSpaceControllerBridge reqScaledSpaceControllerBridge;
  [SerializeField][RequiredComponent] LocalSpaceControllerBridge reqLocalSpaceControllerBridge;
  [SerializeField][RequiredComponent] MainCameraBridge reqMainCameraBridge;


  // Update is called once per frame
  void LateUpdate()
  {
    var scaledSpaceController = reqScaledSpaceControllerBridge.instance;
    var target = reqMainCameraBridge.instance;

    TransformD originPos =
        reqLocalSpaceControllerBridge.instance.frame.fromLocalTransform(target.transform);

    TransformD scaledSpace =
        scaledSpaceController.frame.toGlobalCoord(originPos);

    this.transform.localRotation = (Quaternion)scaledSpace.rotation;
    this.transform.localPosition = (Vector3)scaledSpace.position;
  }
}
