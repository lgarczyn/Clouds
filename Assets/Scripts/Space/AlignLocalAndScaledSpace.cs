using UnityEngine;

/// <summary>
/// Align the scaled space root transform to the local space
/// This allows for a single directional light for the sun
/// </summary>
[RequireComponent(typeof(LocalSpaceControllerBridge))]
public class AlignLocalAndScaledSpace : MonoBehaviour
{
  void LateUpdate()
  {
    var localSpace = GetComponent<LocalSpaceControllerBridge>().instance;
    this.transform.localRotation = (Quaternion)localSpace.frame.toLocalRot(QuaternionD.identity);
  }
}
