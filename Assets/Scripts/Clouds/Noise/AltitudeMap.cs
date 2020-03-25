using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class AltitudeMap : MonoBehaviour {

    public AltitudeMapSettings altitudeSettings;

    public bool viewerEnabled;
    [HideInInspector]
    public bool showSettingsEditor = true;

    public RenderTexture altitudeMap{
        get {
            UpdateMap();
            return (_altitudeMap);
        }
    }
    public float altitudeOffset {
        get {
            UpdateMap();
            return (_altitudeOffset);
        }
    }
    public float altitudeMultiplier {
        get {
            UpdateMap();
            return (_altitudeMultiplier);
        }
    }

    RenderTexture _altitudeMap;
    float _altitudeOffset;
    float _altitudeMultiplier;

    double Remap (double v, double minOld, double maxOld, double minNew, double maxNew) {
        return minNew + (v - minOld) * (maxNew - minNew) / (maxOld - minOld);
    }

    double GetAltitudeDensity (double heightPercent) {

        var set = altitudeSettings;

        double height = Remap (heightPercent, set.lowestLayerAltitude, set.highestLayerAltitude, -0.1, 1.2);
        if (height > 1)
            height = 2 - height;
        else if (height < 0)
            height = System.Math.Abs (height);

        double result = System.Math.Abs (set.linearOffsetStart + height * set.linearOffsetFactor);
        double multiplier = set.initialPowerFactor;
        double originalHeight = height;

        for (int i = 0; i < set.iterativePowerCount; i++) {
            double invertedGradient = System.Math.Abs (Remap (height, 0, 1, -1.2, 1.2));
            if (invertedGradient > 1)
                invertedGradient = (2 - System.Math.Abs (invertedGradient));

            result += System.Math.Pow (invertedGradient, 4) * multiplier;
            multiplier *= set.iterativePowerFactor;
            if (height > 0.5)
                height = height - 0.5;
            height *= 2;
        }

        return result
            * System.Math.Pow (1 - originalHeight, set.powerOffsetPower) * set.powerOffsetRatio
            + System.Math.Pow (originalHeight, set.powerOffsetPowerTop) * set.powerOffsetRatioTop;
    }

    public void UpdateMap () {
        if (_altitudeMap != null && altitudeSettings.isDirty == false)
            return;

        altitudeSettings.isDirty = false;

        int resolution = altitudeSettings.resolution;

        CreateTexture1D (ref _altitudeMap, resolution, "altitudeMap");
        Texture2D temp = new Texture2D(resolution, 1);

        double min = float.PositiveInfinity;
        double max = float.NegativeInfinity;

        double[] valueArray = new double[resolution];


        for (int i = 0; i < resolution; i++) {
            double value = GetAltitudeDensity(i / (float)(resolution - 1));
            if (value < min) min = value;
            if (value > max) max = value;
            valueArray[i] = value;
        }

        for (int i = 0; i < resolution; i++) {
            double value = valueArray[i];
            value = (value - min) / (max - min);
            temp.SetPixel (i, 0, new Color((float)value, (float)value, (float)value, 0));
        }

        _altitudeOffset = (float)min;
        _altitudeMultiplier = (float)(max - min);

        temp.Apply();

        RenderTexture.active = _altitudeMap;
        Graphics.Blit(temp, _altitudeMap);
        RenderTexture.active = null;
    }

    //TODO move all CreateTexture to new TextureTools static file
    void CreateTexture1D (ref RenderTexture texture, int resolution, string name) {
        var format = GraphicsFormat.R16_UNorm;
        if (texture == null || !texture.IsCreated () ||
            texture.dimension != TextureDimension.Tex2D ||
            texture.width != resolution ||
            texture.height != 1 ||
            texture.graphicsFormat != format) {
            if (texture != null) {
                texture.Release ();
            }
            texture = new RenderTexture (resolution, 1, 0);
            texture.graphicsFormat = format;
            texture.enableRandomWrite = true;
            texture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            texture.name = name;

            texture.Create ();
        }
        texture.wrapMode = TextureWrapMode.Mirror;
        texture.filterMode = FilterMode.Bilinear;
    }
}