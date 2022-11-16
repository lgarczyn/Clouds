using UnityEngine;

/// <summary>
/// Align the scaled space root transform to the local space
/// This allows for a single directional light for the sun
/// </summary>
public class AlignLocalAndScaledSpace : MonoBehaviour
{
  [SerializeField][RequiredComponent] LocalSpaceControllerBridge reqLocalSpaceControllerBridge;

  void LateUpdate()
  {
    var localSpace = reqLocalSpaceControllerBridge.instance;
    this.transform.localRotation = (Quaternion)localSpace.frame.toLocalRot(QuaternionD.identity);
  }
}
