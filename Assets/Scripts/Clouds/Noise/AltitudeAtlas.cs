using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System.Collections.Generic;

[Serializable]
public struct Atlas
{
  public Atlas(string name)
  {
    this.curve = new AnimationCurve();
    this.name = name;
  }

  public string name;
  public AnimationCurve curve;
}

[Serializable]
public enum AtlasFormat
{
  SFloat_16 = 1,
  SFloat_32 = 2,
  UNorm_8 = 3,
  UNorm_16 = 4,
}

public class AltitudeAtlas : MonoBehaviour
{

  [SerializeField] List<Atlas> atlases;
  [SerializeField] int resolution;
  [SerializeField] double startAltitude;
  [SerializeField] double endAltitude;
  [SerializeField] AtlasFormat format = AtlasFormat.UNorm_16;

  [HideInInspector]
  public bool isDirty = true;

  public RenderTexture altitudeAtlas
  {
    get
    {
      UpdateMap();
      return (_altitudeAtlas);
    }
  }
  public Vector4 altitudeValueOffsets
  {
    get
    {
      UpdateMap();
      return (_atlasOffsets);
    }
  }
  public Vector4 altitudeValueMultipliers
  {
    get
    {
      UpdateMap();
      return (_atlasMultipliers);
    }
  }
  public double altitudeOffset
  {
    get
    {
      UpdateMap();
      return -startAltitude;
    }
  }
  public double altitudeMultiplier
  {
    get
    {
      UpdateMap();
      return 1 / (endAltitude - startAltitude);
    }
  }

  [SerializeField]
  RenderTexture _altitudeAtlas;

  Vector4 _atlasOffsets;
  Vector4 _atlasMultipliers;

  double Remap(double v, double minOld, double maxOld, double minNew, double maxNew)
  {
    return minNew + (v - minOld) * (maxNew - minNew) / (maxOld - minOld);
  }

  float GetAltitudeDensity(int index, double height)
  {
    return atlases[index].curve.Evaluate((float)height);
  }

  GraphicsFormat graphicsFormat
  {
    get
    {
      return getFormat(atlases.Count, format);
    }
  }

  public void UpdateMap()
  {
    if (_altitudeAtlas != null && isDirty == false)
      return;

    if (atlases.Count > 4) atlases.RemoveRange(4, atlases.Count - 4);

    if (atlases.Count < 1) atlases.Add(new Atlas("Density"));

    if (resolution < 16) resolution = 16;

    isDirty = false;

    // Create a temporary texture to hold the new values
    CreateTexture(ref _altitudeAtlas, resolution, atlases.Count);
    Texture2D temp = new Texture2D(
      resolution,
      1,
      graphicsFormat,
      TextureCreationFlags.None
    );

    // the min and max values of each atlas
    Vector4 mins = Vector4.positiveInfinity;
    Vector4 maxs = Vector4.negativeInfinity;

    // The pixels to write to the atlas, before normalization
    Vector4[] valueArray = new Vector4[resolution];

    // Get the values from the curves, and calculate the min and max values
    for (int i = 0; i < resolution; i++)
    {
      for (int j = 0; j < atlases.Count; j++)
      {
        double height = Remap(i, 0, resolution - 1, startAltitude, endAltitude);
        float value = GetAltitudeDensity(j, height);
        if (value < mins[j]) mins[j] = value;
        if (value > maxs[j]) maxs[j] = value;
        valueArray[i][j] = value;
      }
    }

    // The multiplier to restore the original range
    Vector4 multiplier = (maxs - mins);

    // Apply the normalization and store it in the texture
    for (int i = 0; i < resolution; i++)
    {
      Vector4 pixel = valueArray[i];
      Vector4 normalized = (pixel - mins).InverseScale(multiplier);
      temp.SetPixel(i, 0, normalized);
    }
    // Apply the texture and store the parameters
    temp.Apply();

    RenderTexture.active = _altitudeAtlas;
    Graphics.Blit(temp, _altitudeAtlas);
    RenderTexture.active = null;

    _atlasOffsets = mins;
    _atlasMultipliers = multiplier;
  }

  void CreateTexture(ref RenderTexture texture, int resolution, int colorNum)
  {
    RenderTextureDescriptor desc = TextureTools.GetDescriptorBase(resolution, 1);
    desc.graphicsFormat = graphicsFormat;
    desc.enableRandomWrite = false;

    TextureTools.VerifyTexture(ref texture, desc);

    texture.wrapMode = TextureWrapMode.Clamp;
  }

  void OnValidate()
  {
    isDirty = true;
  }

  static GraphicsFormat getFormat(int colorNum, AtlasFormat format)
  {
    switch (format)
    {
      case AtlasFormat.SFloat_16:
        if (colorNum == 1) return GraphicsFormat.R16_SFloat;
        if (colorNum == 2) return GraphicsFormat.R16G16_SFloat;
        if (colorNum == 3) return GraphicsFormat.R16G16B16_SFloat;
        if (colorNum == 4) return GraphicsFormat.R16G16B16A16_SFloat;
        break;
      case AtlasFormat.SFloat_32:
        if (colorNum == 1) return GraphicsFormat.R32_SFloat;
        if (colorNum == 2) return GraphicsFormat.R32G32_SFloat;
        if (colorNum == 3) return GraphicsFormat.R32G32B32_SFloat;
        if (colorNum == 4) return GraphicsFormat.R32G32B32A32_SFloat;
        break;
      case AtlasFormat.UNorm_8:
        if (colorNum == 1) return GraphicsFormat.R8_UNorm;
        if (colorNum == 2) return GraphicsFormat.R8G8_UNorm;
        if (colorNum == 3) return GraphicsFormat.R8G8B8_UNorm;
        if (colorNum == 4) return GraphicsFormat.R8G8B8A8_UNorm;
        break;
      case AtlasFormat.UNorm_16:
        if (colorNum == 1) return GraphicsFormat.R16_UNorm;
        if (colorNum == 2) return GraphicsFormat.R16G16_UNorm;
        if (colorNum == 3) return GraphicsFormat.R16G16B16_UNorm;
        if (colorNum == 4) return GraphicsFormat.R16G16B16A16_UNorm;
        break;
    }
    throw new System.Exception("Textures only support 1 to 4 colors");
  }
}