using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class ShadowCameraMatrix : MonoBehaviour
{
    public Transform directionalLight;
    public bool active = true;
    public bool debug = false;
    public bool useSun = true;
    public Vector3 alternativeAngles;
    Matrix4x4 matrix = Matrix4x4.identity;


    private void LateUpdate() {

        Camera shadowCamera = GetComponent<Camera>();

        if (active)
        {
            // Either use the sun or the input as our shadow direction
            Quaternion target;
            if (useSun && directionalLight != null)
                target = directionalLight.rotation * Quaternion.Euler(-90,0,0);
            else
                target = Quaternion.Euler(alternativeAngles);

            // Set the initial camera matrix
            // Doesn't set the scaling, as Unity adds it from the camera parameters
            Matrix4x4 shadowMatrix = Matrix4x4.Ortho(-1, 1, -1, 1, -1, 1);

            // Get the angles of sunlight
            Vector3 angles = (target).eulerAngles;

            // Add shear to the custom matrix to match the sunlight
            matrix.m02 = Mathf.Tan(Mathf.Deg2Rad * angles.y);
            matrix.m12 = Mathf.Tan(Mathf.Deg2Rad * -angles.x);

            // Combines the custom matrix, the ortho matrix, the current rotation, the current position
            shadowMatrix =
                matrix
                * shadowMatrix
                * Matrix4x4.Rotate(Quaternion.Inverse(transform.rotation))
                * Matrix4x4.Translate(-transform.position);

            // Apply the matrix
            shadowCamera.worldToCameraMatrix = shadowMatrix;
        }
        else
            shadowCamera.ResetWorldToCameraMatrix();

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

        float fovMultiplier = Mathf.Tan(shadowCamera.fieldOfView * Mathf.Deg2Rad / 2);
        for (int i = 0; i < 8; i++)
        {
            if (i < 4)
                frustumCorners[i].z *= shadowCamera.nearClipPlane;
            else
                frustumCorners[i].z *= shadowCamera.farClipPlane;

            float multiplier;
            if (shadowCamera.orthographic)
                multiplier = shadowCamera.orthographicSize;
            else
                multiplier = frustumCorners[i].z * fovMultiplier;

            frustumCorners[i].x *= multiplier;
            frustumCorners[i].y *= multiplier;

            // Z inversion because OpenGL norm
            frustumCorners[i].z *= -1;

            frustumCorners[i] = shadowCamera.cameraToWorldMatrix * frustumCorners[i];
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
    }

}
