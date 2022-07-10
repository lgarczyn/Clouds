using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMaster : MonoBehaviour
{
  const string headerDecoration = " --- ";
  [Header(headerDecoration + "Main" + headerDecoration)]
  public Shader shader;
  public Transform container;
  public Transform player;
  public Vector4 testParams;

  [Header(headerDecoration + "March settings" + headerDecoration)]
  public float stepSizeRender = 8;
  public float firstStepNoiseMultiplier = 3;
  public float rayOffsetStrength;
  public float minTransmittance = 0.05f;
  public float maxDensity = 57.38f;

  [Header(headerDecoration + "LOD Settings" + headerDecoration)]
  [Range(0, 100)]
  public float lodLevelMagnitude = 9;
  public float lodMinDistance = 200;

  [Header(headerDecoration + "Layers" + headerDecoration)]
  public float scaleGlobal = 1000;
  public float weightGlobal = 1;
  public float scale3 = 400;
  public float weight3 = 1;
  public float scale2 = 10;
  public float weight2 = 1;
  public float scale1 = 1;
  public float weight1 = 1;
  public float scale0 = 0.2f;
  public float weight0 = 1;

  [Header(headerDecoration + "Animation" + headerDecoration)]
  public float timeScale = 1;
  public Vector3 windDirection = new Vector3(1, .1f, 0.3f);
  public float speed3 = 0;
  public float speed2 = 0;
  public float speed1 = 0;
  public float speed0 = 0;

  [Header(headerDecoration + "Lighting" + headerDecoration)]
  public int numStepsLight = 8;
  [Range(0.01f, 10)]
  public float lightAbsorptionThroughCloud = 1;
  [Range(0.01f, 10)]
  public float lightAbsorptionTowardSun = 1;
  [Range(0, 1)]
  public float darknessThreshold = .2f;
  [Range(0, 1)]
  public float forwardScattering = .83f;
  [Range(0, 1)]
  public float backScattering = .3f;
  [Range(0, 1)]
  public float baseBrightness = .8f;
  [Range(0, 1)]
  public float phaseFactor = .15f;
  [Range(0, 10)]
  public float godRaysIntensity = 2f;

  [Header(headerDecoration + "Sky" + headerDecoration)]
  public Color colA;
  public Color colB;
  public Color colC;
  public float distanceFogMultiplier;

  [Header(headerDecoration + "Shadow Mapping" + headerDecoration)]
  public RenderTexture shadowMap;
  public Camera shadowCamera;

  [Header(headerDecoration + "Output Material" + headerDecoration)]
  public Material material;

  bool isMaterialDirty = true;
  private Vector3 lastContainerPosition;

  // The texture generators for the shader
  // TODO: standardize noise generators
  // TODO: allow multiple coexisting generators ?
  // TODO: force the generators on the same object using [Require] ?
  private AltitudeAtlas altitudeAtlasGen;
  private NoiseGenerator noiseGen;

  void UpdateMaps()
  {
    altitudeAtlasGen = FindObjectOfType<AltitudeAtlas>();
    noiseGen = FindObjectOfType<NoiseGenerator>();
    altitudeAtlasGen.UpdateMap();
  }

  void Awake()
  {
    if (Application.isPlaying)
      UpdateMaps();
  }


  private void LateUpdate()
  {

    if (isMaterialDirty || material == null || Application.isPlaying == false)
    {
      isMaterialDirty = false;
      SetParams();
    }

    // If the container has drifted by a large amount
    // TODO: put optimization back
    // if (Vector3.Distance(lastContainerPosition, container.position) > container.localScale.magnitude / 4)
    // {
    material.SetVector("testParams", testParams);
    material.SetVector("boundsMin", container.position - container.localScale / 2);
    material.SetVector("boundsMax", container.position + container.localScale / 2);
    material.SetVector("playerPosition", player.position);
    material.SetVector("shadowMapPosition", shadowCamera.transform.position);
    lastContainerPosition = container.position;
    // }
  }

  void SetParams()
  {
    numStepsLight = Mathf.Max(1, numStepsLight);
    stepSizeRender = Mathf.Max(1, stepSizeRender);

    // Noise
    var noise = FindObjectOfType<NoiseGenerator>();
    noise.UpdateNoise();

    material.SetTexture("ShapeTex", noise.shapeTextureFlat);
    material.SetTexture("DetailTex", noise.shapeTextureFlat);

    // WeatherMap and AltitudeAtlas
    UpdateMaps();
    material.SetTexture("AltitudeAtlas", altitudeAtlasGen.altitudeAtlas);
    material.SetFloat("altitudeValueOffset", altitudeAtlasGen.altitudeValueOffsets.x);
    material.SetFloat("altitudeValueMultiplier", altitudeAtlasGen.altitudeValueMultipliers.x);
    material.SetFloat("altitudeOffset", (float)altitudeAtlasGen.altitudeOffset);
    material.SetFloat("altitudeMultiplier", (float)altitudeAtlasGen.altitudeMultiplier);

    material.SetTexture("ShadowMap", shadowMap);
    material.SetFloat("shadowMapSize", shadowCamera.orthographicSize);

    // Marching settings
    Vector3 size = container.localScale;
    int width = Mathf.CeilToInt(size.x);
    int height = Mathf.CeilToInt(size.y);
    int depth = Mathf.CeilToInt(size.z);

    material.SetFloat("minTransmittance", minTransmittance);
    material.SetFloat("maxDensity", maxDensity);

    material.SetFloat("lodLevelMagnitude", lodLevelMagnitude);
    material.SetFloat("lodMinDistance", lodMinDistance);

    material.SetFloat("scaleGlobal", scaleGlobal);
    material.SetFloat("weightGlobal", weightGlobal);
    material.SetFloat("scale0", scale0);
    material.SetFloat("weight0", weight0);
    material.SetFloat("scale1", scale1);
    material.SetFloat("weight1", weight1);
    material.SetFloat("scale2", scale2);
    material.SetFloat("weight2", weight2);
    material.SetFloat("scale3", scale3);
    material.SetFloat("weight3", weight3);

    material.SetVector("windDirection", windDirection);
    material.SetFloat("speed0", speed0);
    material.SetFloat("speed1", speed1);
    material.SetFloat("speed2", speed2);
    material.SetFloat("speed3", speed3);

    material.SetFloat("lightAbsorptionThroughCloud", lightAbsorptionThroughCloud);
    material.SetFloat("lightAbsorptionTowardSun", lightAbsorptionTowardSun);
    material.SetFloat("darknessThreshold", darknessThreshold);
    material.SetFloat("rayOffsetStrength", rayOffsetStrength);

    material.SetVector("phaseParams", new Vector4(forwardScattering, backScattering, baseBrightness, phaseFactor));

    material.SetVector("boundsMin", container.position - container.localScale / 2);
    material.SetVector("boundsMax", container.position + container.localScale / 2);

    material.SetInt("numStepsLight", numStepsLight);
    material.SetFloat("stepSizeRender", stepSizeRender);
    material.SetFloat("firstStepNoiseMultiplier", firstStepNoiseMultiplier);

    material.SetVector("mapSize", new Vector4(width, height, depth, 0));

    material.SetFloat("timeScale", (Application.isPlaying) ? timeScale : 0);
    material.SetVector("playerPosition", GameObject.FindObjectOfType<MFlight.Demo.Plane>().transform.position);

    material.SetColor("colA", colA);
    material.SetColor("colB", colB);
    material.SetColor("colC", colC);
    material.SetFloat("distanceFogMultiplier", distanceFogMultiplier / 1000);
    material.SetFloat("godRaysIntensity", godRaysIntensity / 1000);
  }

  void OnValidate()
  {
    isMaterialDirty = true;
  }
}