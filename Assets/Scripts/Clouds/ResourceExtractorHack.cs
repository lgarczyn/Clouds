using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Rendering;

public class ResourceExtractorHack : ResourceCalculator
{
    public RenderTexture renderTexture;

    Texture2D _outputTexture;

    [SerializeField] float densityMultiplier = 400;

    [SerializeField] FloatVariable playerLight;
    [SerializeField] FloatVariable playerCloudDensity;

    void Start() {        
        // Add the onPostRender callback
        RenderPipelineManager.endCameraRendering += OnPostRenderCallback;

        _outputTexture = new Texture2D(1, 1);
    }

    void OnPostRenderCallback(ScriptableRenderContext context, Camera cam) {
        // Only run once per frame
        if (cam != Camera.main) return;
        
        // Remember currently active render texture
        RenderTexture currentActiveRT = RenderTexture.active;

        // Set the supplied RenderTexture as the active one
        RenderTexture.active = renderTexture;

        // Create a new Texture2D and read the RenderTexture image into it
        _outputTexture.ReadPixels(new Rect(0, 0, 1, 1), 0, 0, false);

        // Restore previously active render texture
        RenderTexture.active = currentActiveRT;

        Color value = _outputTexture.GetPixel(0, 0);
        // Check that the canary values from the shader are still valid
        if (value.b < 0.495f || value.b > 0.505f) Debug.Log("Failed to recover resources from GPU");
        this._sampledOutputPixel = value;

        if (playerLight) playerLight.SetValue(GetLight());
        if (playerCloudDensity) playerCloudDensity.SetValue(GetDensity());
    }

    Color _sampledOutputPixel;

    public override float GetLight() {
        return _sampledOutputPixel.g;
    }
    public override float GetDensity() {
        return (_sampledOutputPixel.r - 0.5f) * densityMultiplier;
    }

    void OnDestroy()
    {
         RenderPipelineManager.endCameraRendering -= OnPostRenderCallback;
    }

}
