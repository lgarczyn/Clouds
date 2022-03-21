using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ResourceExtractorHack : ResourceCalculator
{
    public RenderTexture renderTexture;

    Texture2D outputTexture;

    void Start() {        
        // Add the onPostRender callback
        RenderPipelineManager.endCameraRendering += OnPostRenderCallback;

        outputTexture = new Texture2D(1, 1);
    }

    void OnPostRenderCallback(ScriptableRenderContext context, Camera cam) {
        // Only run once per frame
        if (cam != Camera.main) return;
        
        // Remember currently active render texture
        RenderTexture currentActiveRT = RenderTexture.active;

        // Set the supplied RenderTexture as the active one
        RenderTexture.active = renderTexture;

        // Create a new Texture2D and read the RenderTexture image into it
        outputTexture.ReadPixels(new Rect(0, 0, 1, 1), 0, 0, false);

        // Restorie previously active render texture
        RenderTexture.active = currentActiveRT;

        var value = outputTexture.GetPixel(0, 0);
        // Check that the canary values from the shader are still valid
        if (value.b < 0.495f || value.b > 0.505f) Debug.Log("Failed to recover resources from GPU");
        this.sampledOutputPixel = value;
    }

    Color sampledOutputPixel;

    override public float GetLight() {
        return sampledOutputPixel.g;
    }
    override public float GetDensity() {
        return sampledOutputPixel.r;
    }

    void OnDestroy()
    {
         RenderPipelineManager.endCameraRendering -= OnPostRenderCallback;
    }

}
