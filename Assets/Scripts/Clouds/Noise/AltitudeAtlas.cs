using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System.Collections.Generic;
using System.Linq;

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
  [SerializeField] bool normalize;

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
  public Vector4 meanSquareError
  {
    get
    {
      return _meanSquareError;
    }
  }

  public List<AnimationCurve> outputCurves
  {
    get
    {
      return _curvesOut;
    }
  }

  [SerializeField]
  RenderTexture _altitudeAtlas;

  Vector4 _atlasOffsets;
  Vector4 _atlasMultipliers;
  Vector4 _meanSquareError;

  List<AnimationCurve> _curvesOut;

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
    // Check all parameters are valid
    if (_altitudeAtlas != null && isDirty == false)
      return;

    if (atlases == null) atlases = new List<Atlas>();

    if (atlases.Count > 4) atlases = atlases.Take(4).ToList();

    if (atlases.Count < 1) atlases.Add(new Atlas("Density"));

    if (resolution < 16) resolution = 16;

    // Initialize output display values
    _curvesOut = new List<AnimationCurve>();
    _meanSquareError = Vector4.zero;

    // Update the render texture if necessary
    CreateTexture(ref _altitudeAtlas, resolution, atlases.Count);
    // Create a temporary texture to hold the new values
    // TODO: Check that the format is supported
    Texture2D temp = new Texture2D(
      resolution,
      1,
      graphicsFormat,
      TextureCreationFlags.None
    );

    // The pixels to write to the atlas, before normalization
    Vector4[] valueArray = new Vector4[resolution];

    // Get the values from the curves
    for (int i = 0; i < resolution; i++)
    {
      for (int j = 0; j < atlases.Count; j++)
      {
        double height = Remap(i, 0, resolution - 1, startAltitude, endAltitude);
        valueArray[i][j] = GetAltitudeDensity(j, height);
      }
    }

    // The multiplier to restore the original range
    Vector4 multipliers = Vector4.one;
    Vector4 offsets = Vector4.zero;

    // If should normalize
    if (normalize)
    {
      // the min and max values of each atlas
      Vector4 mins = valueArray.Aggregate((a, b) => a.Min(b));
      Vector4 maxs = valueArray.Aggregate((a, b) => a.Max(b));
      // For each atlas, set non identity multiplier
      for (int j = 0; j < atlases.Count; j++)
      {
        multipliers[j] = Math.Max(maxs[j] - mins[j], 0.01f);
        offsets[j] = mins[j];
      }
    }

    // Convert to pixel and normalize
    Color[] pixels = valueArray.Select((v) =>
      (Color)(v - offsets).InverseScale(multipliers)
    ).ToArray();
    // Set the pixels
    temp.SetPixels(0, 0, resolution, 1, pixels);
    // Ask unity to store the texture
    temp.Apply();

    // Render to render texture
    RenderTexture.active = _altitudeAtlas;
    Graphics.Blit(temp, _altitudeAtlas);
    RenderTexture.active = null;

    // Update the scale parameters
    _atlasOffsets = offsets;
    _atlasMultipliers = multipliers;
    // Set as clean
    isDirty = false;

    // Try to read out the texture and compare with what was stored
    try
    {
      // The squared diff between each atlas and the output
      Vector4 squareError = Vector4.zero;

      //Read the pixels back into the texture
      RenderTexture.active = _altitudeAtlas;
      temp.ReadPixels(new Rect(0, 0, resolution, 1), 0, 0, false);
      RenderTexture.active = null;

      // Apply just to be sure
      temp.Apply();

      // Get the pixel array
      var pixelsOut = temp.GetPixels(0, 0, resolution, 1, 0);

      // Initialize the output curves
      _curvesOut = new List<AnimationCurve>();
      for (int i = 0; i < atlases.Count; i++) _curvesOut.Add(new AnimationCurve());

      // For each pixel
      for (int i = 0; i < resolution; i++)
      {
        // Get the output pixel
        Color c = pixelsOut[i];
        // Calculate the square diff with the original pixel
        Vector4 diff = (Vector4)(c - pixels[i]);
        squareError += Vector4.Scale(diff, diff);
        // Calculate the original cuve values
        c = (Color)(Vector4.Scale(c, _atlasMultipliers) + _atlasOffsets);
        // Calculate the original curve time
        float alt = (float)Remap(i, 0, resolution, startAltitude, endAltitude);
        // Add the keys to each curve
        for (int j = 0; j < atlases.Count; i++)
        {
          _curvesOut[j].AddKey(alt, c[j]);
        }
      };
      // Average the errors
      _meanSquareError = squareError / resolution;
    }
    catch (Exception e)
    {
      Debug.Log(e.Message);
    }
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
        if (colorNum == 3) return GraphicsFormat.R16G16B16A16_SFloat;
        if (colorNum == 4) return GraphicsFormat.R16G16B16A16_SFloat;
        break;
      case AtlasFormat.SFloat_32:
        if (colorNum == 1) return GraphicsFormat.R32_SFloat;
        if (colorNum == 2) return GraphicsFormat.R32G32_SFloat;
        if (colorNum == 3) return GraphicsFormat.R32G32B32A32_SFloat;
        if (colorNum == 4) return GraphicsFormat.R32G32B32A32_SFloat;
        break;
      case AtlasFormat.UNorm_8:
        if (colorNum == 1) return GraphicsFormat.R8_UNorm;
        if (colorNum == 2) return GraphicsFormat.R8G8_UNorm;
        if (colorNum == 3) return GraphicsFormat.R8G8B8A8_UNorm;
        if (colorNum == 4) return GraphicsFormat.R8G8B8A8_UNorm;
        break;
      case AtlasFormat.UNorm_16:
        if (colorNum == 1) return GraphicsFormat.R16_UNorm;
        if (colorNum == 2) return GraphicsFormat.R16G16_UNorm;
        if (colorNum == 3) return GraphicsFormat.R16G16B16A16_UNorm;
        if (colorNum == 4) return GraphicsFormat.R16G16B16A16_UNorm;
        break;
    }
    throw new System.Exception("Textures only support 1 to 4 colors");
  }
}