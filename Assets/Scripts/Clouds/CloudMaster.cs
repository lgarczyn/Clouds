using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CloudMaster : MonoBehaviour {
    const string headerDecoration = " --- ";
    [Header (headerDecoration + "Main" + headerDecoration)]
    public Shader shader;
    public Transform container;
    public Vector3 cloudTestParams;

    [Header (headerDecoration + "March settings" + headerDecoration)]
    public float stepSizeRender = 8;
    public float rayOffsetStrength;
    public Texture2D heightGradientTex;
    public float minTransmittance = 0.05f;

    [Header (headerDecoration + "LOD Settings" + headerDecoration)]
    [Range (0, 15)]
    public float lodLevelMagnitude = 9;
    public float lodMinDistance = 200;

    [Header (headerDecoration + "Base Shape" + headerDecoration)]
    public float cloudScale = 1;
    public float densityMultiplier = 1;
    public float visualDensityMultiplier = 1;
    public float densityOffset;
    public Vector3 shapeOffset;
    public Vector2 heightOffset;
    public Vector4 shapeNoiseWeights;

    [Header (headerDecoration + "Limit Taper Settings" + headerDecoration)]
    [Range (0, 100)]
    public float densityTaperUpStrength = 30;
    [Range (0, 1)]
    public float densityTaperUpStart = 0.8f;
    [Range (0, 100)]
    public float densityTaperDownStrength = 30;
    [Range (0, 1)]
    public float densityTaperDownStart = 0.2f;

    [Header (headerDecoration + "Detail" + headerDecoration)]
    public float detailNoiseScale = 10;
    public float detailNoiseWeight = .1f;
    public Vector3 detailNoiseWeights;
    public Vector3 detailOffset;

    [Header (headerDecoration + "Lighting" + headerDecoration)]
    public int numStepsLight = 8;
    public float lightAbsorptionThroughCloud = 1;
    public float lightAbsorptionTowardSun = 1;
    [Range (0, 1)]
    public float darknessThreshold = .2f;
    [Range (0, 1)]
    public float forwardScattering = .83f;
    [Range (0, 1)]
    public float backScattering = .3f;
    [Range (0, 1)]
    public float baseBrightness = .8f;
    [Range (0, 1)]
    public float phaseFactor = .15f;
    [Range (0, 10)]
    public float godRaysIntensity = 2f;

    [Header (headerDecoration + "Animation" + headerDecoration)]
    public float timeScale = 1;
    public float baseSpeed = 1;
    public float detailSpeed = 2;

    [Header (headerDecoration + "Sky" + headerDecoration)]
    public Color colA;
    public Color colB;
    public Color colC;

    // [Header (headerDecoration + "Shadow Mapping" + headerDecoration)]
    // public ShadowMaster shadowMapper;
    // public RenderTexture shadowMap;

    // Internal
    // [HideInInspector]
    [Header ("Output Material")]
    public Material material;

    bool isMaterialDirty = true;
    private Vector3 lastContainerPosition;

    // The texture generators for the shader
    // TODO: standardize noise generators
    // TODO: allow multiple coexisting generators ?
    // TODO: force the generators on the same object using [Require] ?
    private AltitudeMap altitudeMapGen;
    private NoiseGenerator noiseGen;

    void UpdateMaps()
    {
        altitudeMapGen = FindObjectOfType<AltitudeMap> ();
        noiseGen = FindObjectOfType<NoiseGenerator> ();
        altitudeMapGen.UpdateMap ();
    }

    void Awake () {
        if (Application.isPlaying)
            UpdateMaps();
        Application.targetFrameRate = 60;
    }

    private void Update () {

        if (isMaterialDirty || material == null || Application.isPlaying == false)
        {
            isMaterialDirty = false;

            // Validate inputs
            if (material == null || material.shader != shader) {
                material = new Material (shader);
            }

            SetParams();
        }

        // If the container has drifted by a large amount
        if (Vector3.Distance(lastContainerPosition, container.position) > container.localScale.magnitude / 4)
        {
            material.SetVector ("boundsMin", container.position - container.localScale / 2);
            material.SetVector ("boundsMax", container.position + container.localScale / 2);
            lastContainerPosition = container.position;
        }
    }

    void SetParams ()
    {
        numStepsLight = Mathf.Max (1, numStepsLight);
        stepSizeRender = Mathf.Max(1, stepSizeRender);

        // Noise
        var noise = FindObjectOfType<NoiseGenerator> ();
        noise.UpdateNoise ();

        material.SetTexture ("NoiseTex", noise.shapeTexture);
        material.SetTexture ("DetailNoiseTex", noise.detailTexture);
        material.SetTexture ("HeightGradientTex", heightGradientTex);

        // WeatherMap and AltitudeMap
        UpdateMaps();
        material.SetTexture ("AltitudeMap", altitudeMapGen.altitudeMap);
        material.SetFloat("altitudeOffset", altitudeMapGen.altitudeOffset);
        material.SetFloat("altitudeMultiplier", altitudeMapGen.altitudeMultiplier);
        // material.SetTexture("ShadowMap", shadowMap);
        // material.SetFloat("shadowMapSize", shadowMapper.GetSize());

        // Marching settings
        Vector3 size = container.localScale;
        int width = Mathf.CeilToInt (size.x);
        int height = Mathf.CeilToInt (size.y);
        int depth = Mathf.CeilToInt (size.z);

        material.SetFloat("minTransmittance", minTransmittance);

        material.SetFloat("lodLevelMagnitude", lodLevelMagnitude);
        material.SetFloat("lodMinDistance", lodMinDistance);

        material.SetFloat ("scale", cloudScale);
        material.SetFloat ("densityMultiplier", densityMultiplier);
        material.SetFloat ("visualDensityMultiplier", visualDensityMultiplier);
        material.SetFloat ("densityOffset", densityOffset);
        material.SetFloat ("lightAbsorptionThroughCloud", lightAbsorptionThroughCloud);
        material.SetFloat ("lightAbsorptionTowardSun", lightAbsorptionTowardSun);
        material.SetFloat ("darknessThreshold", darknessThreshold);
        material.SetVector ("params", cloudTestParams);
        material.SetFloat ("rayOffsetStrength", rayOffsetStrength);

        material.SetFloat ("detailNoiseScale", detailNoiseScale);
        material.SetFloat ("detailNoiseWeight", detailNoiseWeight);
        material.SetVector ("shapeOffset", shapeOffset);
        material.SetVector ("detailOffset", detailOffset);
        material.SetVector ("detailWeights", detailNoiseWeights);
        material.SetVector ("shapeNoiseWeights", shapeNoiseWeights);
        material.SetVector ("phaseParams", new Vector4 (forwardScattering, backScattering, baseBrightness, phaseFactor));

        material.SetFloat("densityTaperUpStrength", densityTaperUpStrength);
        material.SetFloat("densityTaperUpStart", densityTaperUpStart);
        material.SetFloat("densityTaperDownStrength", densityTaperDownStrength);
        material.SetFloat("densityTaperDownStart", densityTaperDownStart);

        material.SetVector ("boundsMin", container.position - container.localScale / 2);
        material.SetVector ("boundsMax", container.position + container.localScale / 2);

        material.SetInt ("numStepsLight", numStepsLight);
        material.SetFloat("stepSizeRender", stepSizeRender);

        material.SetVector ("mapSize", new Vector4 (width, height, depth, 0));

        material.SetFloat ("timeScale", (Application.isPlaying) ? timeScale : 0);
        material.SetFloat ("baseSpeed", baseSpeed);
        material.SetFloat ("detailSpeed", detailSpeed);
        material.SetVector("playerPosition", GameObject.FindObjectOfType<MFlight.Demo.Plane>().transform.position);

        material.SetColor ("colA", colA);
        material.SetColor ("colB", colB);
        material.SetColor ("colC", colC);
        material.SetFloat ("godRaysIntensity", godRaysIntensity / 1000);
    }

    void SetDebugParams () {

        int debugModeIndex = 0;
        var noise = this.noiseGen;
        if (noise.viewerEnabled) {
            debugModeIndex = (noise.activeTextureType == NoiseGenerator.CloudNoiseType.Shape) ? 1 : 2;
        }
        if (altitudeMapGen.viewerEnabled) {
            debugModeIndex = 4;
        }

        material.SetInt ("debugViewMode", debugModeIndex);
        material.SetFloat ("debugNoiseSliceDepth", noise.viewerSliceDepth);
        material.SetFloat ("debugTileAmount", noise.viewerTileAmount);
        material.SetFloat ("viewerSize", noise.viewerSize);
        material.SetVector ("debugChannelWeight", noise.ChannelMask);
        material.SetInt ("debugGreyscale", (noise.viewerGreyscale) ? 1 : 0);
        material.SetInt ("debugShowAllChannels", (noise.viewerShowAllChannels) ? 1 : 0);
    }

    void OnValidate() {
        isMaterialDirty = true;
    }
}