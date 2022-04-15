using UnityEngine;

/// <summary>
/// Align the scaled space root transform to the local space
/// This allows for a single directional light for the sun
/// </summary>
public class AlignLocalAndScaledSpace : MonoBehaviour
{
  public LocalSpaceController localSpace;

  void LateUpdate()
  {
    this.transform.localRotation = (Quaternion)localSpace.frame.toLocalRot(QuaternionD.identity);
  }
}
