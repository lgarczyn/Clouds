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

        // Force the camera to calculate depth
        shadowCamera.depthTextureMode = DepthTextureMode.Depth;

        if (active) {
            // Either use the sun or the input as our shadow direction
            Quaternion target;
            if (useSun && directionalLight != null)
                target = Quaternion.Euler(-90,0,0) * directionalLight.rotation;
            else
                target = Quaternion.Euler(alternativeAngles);

            // Set the initial camera matrix
            // Doesn't set the scaling, as Unity adds it from the camera parameters

            float w = shadowCamera.orthographicSize;
            float near = shadowCamera.nearClipPlane;
            float far = shadowCamera.farClipPlane;
            Matrix4x4 shadowMatrix = Matrix4x4.Ortho(-w, w, -w, w, near, far);

            // Get the angles of sunlight
            Vector3 angles = (target).eulerAngles;

            // Add shear to the custom matrix to match the sunlight
            matrix.m02 = Mathf.Tan(Mathf.Deg2Rad * angles.y);
            matrix.m12 = Mathf.Tan(Mathf.Deg2Rad * -angles.x);

            // Combines the custom matrix, the ortho matrix, the current rotation, the current position
            shadowMatrix =
                shadowMatrix * matrix;

            // Apply the matrix
            shadowCamera.projectionMatrix = shadowMatrix;
        }
        else {
            shadowCamera.ResetProjectionMatrix();
        }

        if (debug == false)
            return;

        // z is left positive for later signing
        // Vector4 is needed for translations to work
        // Near 0-3
        // Far 4-7
        Vector4[] frustumCorners = new Vector4[] {
            new Vector4(1, 1, -1, 1),
            new Vector4(1, -1, -1, 1),
            new Vector4(-1, -1, -1, 1),
            new Vector4(-1, 1, -1, 1),
            new Vector4(1, 1, 1, 1),
            new Vector4(1, -1, 1, 1),
            new Vector4(-1, -1, 1, 1),
            new Vector4(-1, 1, 1, 1)
        };

        // Apply the inverse camera projection to every frustum points
        float fovMultiplier = Mathf.Tan(shadowCamera.fieldOfView * Mathf.Deg2Rad / 2);
        for (int i = 0; i < 8; i++) {
            frustumCorners[i] =
                shadowCamera.cameraToWorldMatrix
                * Matrix4x4.Inverse(shadowCamera.nonJitteredProjectionMatrix)
                * frustumCorners[i];
        }

        Color[] frustumColors = new Color[4]{Color.red, Color.blue, Color.yellow, Color.green}; 

        for (int i = 0; i < 4; i++) {
            // Frustum Sides
            Debug.DrawLine(frustumCorners[i], frustumCorners[i + 4], frustumColors[i]);
            // Near culling plane
            Debug.DrawLine(frustumCorners[i], frustumCorners[(i + 1) % 4], Color.black);
            // Far culling plane
            Debug.DrawLine(frustumCorners[i + 4], frustumCorners[(i + 1) % 4 + 4], Color.white);
        }
    }

}
