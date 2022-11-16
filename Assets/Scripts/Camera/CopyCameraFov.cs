using UnityEngine;

public class CopyCameraFov : MonoBehaviour
{
  public Camera target;

  [RequiredComponent][SerializeField] Camera reqCamera;

  void LateUpdate()
  {
    reqCamera.fieldOfView = target.fieldOfView;
  }
}
