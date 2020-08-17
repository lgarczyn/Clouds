using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class SetCameraDepthTextureMode : MonoBehaviour {

    [SerializeField] private DepthTextureMode depthTextureMode = DepthTextureMode.Depth;

    #if UNITY_EDITOR
    private void Update() {
        // Makes sure the editor view is also set to depth
        if (Camera.current)
        {
            Camera.current.depthTextureMode |= depthTextureMode;
        }
    }
    #else
    void OnEnable() {
        GetComponent<Camera>().depthTextureMode |= depthTextureMode;
    }
    #endif
}