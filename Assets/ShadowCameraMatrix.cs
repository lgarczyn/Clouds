using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class ShadowCameraMatrix : MonoBehaviour
{
    public Transform directionalLight;
    public Transform activeCamera;
    public Vector3 offset;
    public bool active = true;
    public bool useSun = true;
    public Vector3 alternativeAngles;
    public Matrix4x4 matrix = Matrix4x4.identity;


    private void Update() {

        Camera shadowCamera = GetComponent<Camera>();

        shadowCamera.depthTextureMode = DepthTextureMode.Depth;
        shadowCamera.orthographic = true;

        if (active)
        {
            Quaternion target;
            if (useSun)
                target = directionalLight.rotation;
            else
                target = Quaternion.Euler(alternativeAngles);
            Matrix4x4 shadowMatrix = Matrix4x4.Ortho(-1, 1, -1, 1, -1, 1);

            Vector3 angles = (Quaternion.Euler(-90,0,0) * directionalLight.rotation).eulerAngles;

            matrix.m02 = Mathf.Tan(Mathf.Deg2Rad * angles.y);
            matrix.m12 = Mathf.Tan(Mathf.Deg2Rad * -angles.x);

            // shadowMatrix =  Matrix4x4.TRS(transform.localPosition, Quaternion.identity, new Vector3(1,1,1));
            shadowMatrix =
                matrix
                * shadowMatrix
                * Matrix4x4.Rotate(target)
                * Matrix4x4.Inverse(Matrix4x4.Rotate(transform.rotation))
                * Matrix4x4.Translate(-transform.position);

            // Plane p = new Plane(Vector3.up, 1000);

            //shadowCamera.worldToCameraMatrix = shadowMatrix;

            //shadowMatrix = shadowCamera.CalculateObliqueMatrix(new Vector4(0,-1,0,0));// * shadowMatrix;

            shadowCamera.worldToCameraMatrix = shadowMatrix;
        }
        else
            shadowCamera.ResetWorldToCameraMatrix();

        // z is left positive for later signing
        // Vector4 is needed for translations to work
        Vector4[] FrustumCornersNear = new Vector4[]{
            new Vector4(1, 1, 1, 1),
            new Vector4(1, -1, 1, 1),
            new Vector4(-1, 1, 1, 1),
            new Vector4(-1, -1, 1, 1)};

        Vector4[] FrustumCornersFar = new Vector4[]{
            new Vector4(1, 1, 1, 1),
            new Vector4(1, -1, 1, 1),
            new Vector4(-1, 1, 1, 1),
            new Vector4(-1, -1, 1, 1)
        };
        Vector4[] NormCorners = new Vector4[8];

        // shadowCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), shadowCamera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, FrustumCornersNear);
        // shadowCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), shadowCamera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, FrustumCornersFar);

        FrustumCornersNear.CopyTo(NormCorners, 0);
        FrustumCornersFar.CopyTo(NormCorners, 4);

        for (int i = 0; i < 8; i++)
        {
            NormCorners[i].x *= shadowCamera.orthographicSize;
            NormCorners[i].y *= shadowCamera.orthographicSize;
            //dirty hack
            if (i < 4)
                NormCorners[i].z *= shadowCamera.nearClipPlane;
            else
                NormCorners[i].z *= shadowCamera.farClipPlane;

            // Z inversion because OpenGL or something
            NormCorners[i].z *= -1;

            NormCorners[i] = shadowCamera.cameraToWorldMatrix /* transform.worldToLocalMatrix*/ * NormCorners[i];
            //NormCorners[i] = shadowCamera.transform.TransformVector(NormCorners[i]);
            //NormCorners[i] = NormCorners[i].normalized;
            //Debug.Log(NormCorners[i]);
            //Debug.Log(transform.TransformPoint(NormCorners[i]));
        }
        Debug.DrawLine(NormCorners[0], NormCorners[4], Color.red);   //BL
        Debug.DrawLine(NormCorners[1], NormCorners[5], Color.blue);  //TL
        Debug.DrawLine(NormCorners[2], NormCorners[6], Color.yellow);//TR
        Debug.DrawLine(NormCorners[3], NormCorners[7], Color.green); //BR

        Debug.DrawLine(NormCorners[0], NormCorners[1], Color.black);   //BL
        Debug.DrawLine(NormCorners[1], NormCorners[3], Color.black);  //TL
        Debug.DrawLine(NormCorners[2], NormCorners[0], Color.black);//TR
        Debug.DrawLine(NormCorners[3], NormCorners[2], Color.black); //BR

        Debug.DrawLine(NormCorners[4], NormCorners[5], Color.white);   //BL
        Debug.DrawLine(NormCorners[5], NormCorners[7], Color.white);  //TL
        Debug.DrawLine(NormCorners[6], NormCorners[4], Color.white);//TR
        Debug.DrawLine(NormCorners[7], NormCorners[6], Color.white); //BR
        // MyMaterial.SetVector("_BL", NormCorners[0]);
        // MyMaterial.SetVector("_TL", NormCorners[1]);
        // MyMaterial.SetVector("_TR", NormCorners[2]);
        // MyMaterial.SetVector("_BR", NormCorners[3]);
    }

}
