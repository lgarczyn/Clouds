using UnityEngine;

[ExecuteAlways]
//TODO remove need for execute-always
//Be careful, it gets its data from own transform, sun pos and player pos
public class ShadowCameraMatrix : MonoBehaviour
{
  public bool active = true;
  public bool debug = false;
  public float roundFact = 1f;

  [SerializeField][RequiredComponent] Camera reqCamera;
  [SerializeField][RequiredComponent] LocalSpaceControllerBridge reqLocalSpaceController;
  [SerializeField][RequiredComponent] PlayerManagerBridge reqPlayerManager;

  private static Vector3 CalculatePos(Vector3 targetPos, Vector3 sunlightDir)
  {
    float verticalPos = targetPos.y;
    Vector3 newPos = new Vector3(targetPos.x, 0, targetPos.z);

    // If the sunlight is not horizontal (would cause division by 0)
    if (Mathf.Abs(sunlightDir.y) > 0)
    {
      // offset the camera so that it still aims at the target despite angle
      newPos -= (targetPos.y / sunlightDir.y) * new Vector3(sunlightDir.x, 0, sunlightDir.z);
    }
    return newPos;
  }

  private static Vector3 Round(Vector3 pos, float factor)
  {
    if (factor == 0) return pos;
    return (Vector3)Vector3Int.RoundToInt(pos / factor) * factor;
  }

  private static Matrix4x4 CalculateMatrix(Vector3 sunlightDir, Transform camera)
  {
    // Set the initial camera matrix
    // Doesn't set the scaling, as Unity adds it from the camera parameters
    Matrix4x4 shadowMatrix = Matrix4x4.Ortho(-1, 1, -1, 1, -1, 1);


    // Project the vector onto the y=-1 plane
    sunlightDir /= -sunlightDir.y;

    // Draw the calculated line
    // should be completely overlapped by the magenta frustrum center line when functional
    Debug.DrawLine(Vector3.zero, sunlightDir, Color.green);

    // Add shear to a new matrix to match the sunlight
    Matrix4x4 shearMatrix = Matrix4x4.identity;
    shearMatrix.m02 = sunlightDir.x;
    shearMatrix.m12 = sunlightDir.z;

    // Combines the shear matrix, the ortho matrix, the current rotation, the current position
    shadowMatrix =
        shearMatrix
        * shadowMatrix
        * Matrix4x4.Rotate(Quaternion.Inverse(camera.rotation))
        * Matrix4x4.Translate(-camera.position);

    return shadowMatrix;
  }

  private void LateUpdate()
  {
    if (active)
    {
      // Get the direction of sunlight
      Vector3 sunlightDir = (Vector3)reqLocalSpaceController.instance.GetSunDirection();

      // Get the target follow position
      Vector3 targetPos = reqPlayerManager.playerTransform.position;

      // Update position and matrix
      transform.position = Round(CalculatePos(targetPos, sunlightDir), roundFact);
      reqCamera.worldToCameraMatrix = CalculateMatrix(sunlightDir, transform);
    }
    else
      reqCamera.ResetWorldToCameraMatrix();

    if (debug == false)
      return;

    // z is left positive for later signing
    // Vector4 is needed for translations to work
    // Near 0-3
    // Far 4-7
    Vector4[] frustumCorners = new Vector4[]{
            new Vector4(1, 1, 1, 1),
            new Vector4(1, -1, 1, 1),
            new Vector4(-1, 1, 1, 1),
            new Vector4(-1, -1, 1, 1),
            new Vector4(1, 1, 1, 1),
            new Vector4(1, -1, 1, 1),
            new Vector4(-1, 1, 1, 1),
            new Vector4(-1, -1, 1, 1)
        };

    // Because the custom camera projection matrix doesn't do either scaling or projection itself
    // Only position rotation and shear
    // We do the inverse projection and scaling ourselves to display the result

    float fovMultiplier = Mathf.Tan(reqCamera.fieldOfView * Mathf.Deg2Rad / 2);
    for (int i = 0; i < 8; i++)
    {
      if (i < 4)
        frustumCorners[i].z *= reqCamera.nearClipPlane;
      else
        frustumCorners[i].z *= reqCamera.farClipPlane;

      float multiplier;
      if (reqCamera.orthographic)
        multiplier = reqCamera.orthographicSize;
      else
        multiplier = frustumCorners[i].z * fovMultiplier;

      frustumCorners[i].x *= multiplier;
      frustumCorners[i].y *= multiplier;

      // Z inversion because OpenGL norm
      frustumCorners[i].z *= -1;

      frustumCorners[i] = reqCamera.cameraToWorldMatrix * frustumCorners[i];
    }

    // Frustum

    Debug.DrawLine(frustumCorners[0], frustumCorners[4], Color.red);
    Debug.DrawLine(frustumCorners[1], frustumCorners[5], Color.blue);
    Debug.DrawLine(frustumCorners[2], frustumCorners[6], Color.yellow);
    Debug.DrawLine(frustumCorners[3], frustumCorners[7], Color.green);

    // Near culling plane

    Debug.DrawLine(frustumCorners[0], frustumCorners[1], Color.black);
    Debug.DrawLine(frustumCorners[1], frustumCorners[3], Color.black);
    Debug.DrawLine(frustumCorners[2], frustumCorners[0], Color.black);
    Debug.DrawLine(frustumCorners[3], frustumCorners[2], Color.black);

    // Far culling plane

    Debug.DrawLine(frustumCorners[4], frustumCorners[5], Color.white);
    Debug.DrawLine(frustumCorners[5], frustumCorners[7], Color.white);
    Debug.DrawLine(frustumCorners[6], frustumCorners[4], Color.white);
    Debug.DrawLine(frustumCorners[7], frustumCorners[6], Color.white);

    // Center line

    Debug.DrawLine(
        (frustumCorners[0] + frustumCorners[3]) / 2,
        (frustumCorners[4] + frustumCorners[7]) / 2,
        Color.magenta);
  }

}
