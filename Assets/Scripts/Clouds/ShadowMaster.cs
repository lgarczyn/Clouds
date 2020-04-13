using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
public class ShadowMaster : MonoBehaviour
{
    public Material material;
    public Camera shadowCamera;
    public bool updateInEditor = true;
    public bool updateInGame = true;

    void Start() {
        shadowCamera = GetComponent<Camera>();
        shadowCamera.enabled = false;

        EditorApplication.playModeStateChanged += HandleOnPlayModeChanged;
    }

    [ImageEffectOpaque]
    void OnRenderImage (RenderTexture src, RenderTexture dest) {
        src.DiscardContents(true, false);
        if (material)
            Graphics.Blit (src, dest, material);
        else
            Graphics.Blit (src, dest);
    }

    public void Update() {
        if ((Application.isPlaying == true && updateInGame)
        || (Application.isPlaying == false && updateInEditor))
        {
            ForceRender();
        }
    }

    public void SetMaterial(Material src)
    {
        material = src;
    }

    public float GetSize()
    {
        return shadowCamera.orthographicSize;
    }

    public void ForceRender()
    {
        shadowCamera.Render();
    }

    void HandleOnPlayModeChanged(PlayModeStateChange mode)
    {
        if (mode == PlayModeStateChange.EnteredEditMode || mode == PlayModeStateChange.EnteredPlayMode)
        {
            ForceRender();
        }
    }
}
