using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
public class ShadowMaster : MonoBehaviour
{
    public Material material;
    public Camera shadowCamera;

    private void Start() {
        shadowCamera = GetComponent<Camera>();
        shadowCamera.forceIntoRenderTexture = true;
        shadowCamera.depthTextureMode |= DepthTextureMode.Depth;
        shadowCamera.enabled = false;
    }

    [ImageEffectOpaque]
    private void OnRenderImage (RenderTexture src, RenderTexture dest) {
        if (material)
            Graphics.Blit (src, dest, material);
        else
            Graphics.Blit (src, dest);
    }

    private void Update() {
        shadowCamera.Render();
    }

    public void SetMaterial(Material src)
    {
        material = src;
    }
}
