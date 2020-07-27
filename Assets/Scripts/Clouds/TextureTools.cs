using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public static class TextureTools
{
    static public bool VerifyTexture(ref RenderTexture tex, RenderTextureDescriptor desc, string name = null)
    {
        bool recreated = false;
        desc.msaaSamples = 1;
        desc.mipCount = 0;
        if (tex == null)
        {
            tex = new RenderTexture(desc);
        }
        else
        {
            tex.enableRandomWrite = desc.enableRandomWrite;

            if (tex.dimension != desc.dimension
            || tex.width != desc.width
            || tex.height != desc.height
            || tex.volumeDepth != desc.volumeDepth
            || tex.graphicsFormat != desc.graphicsFormat)
            {
                tex.Release();
                tex.dimension = desc.dimension;
                tex.width = desc.width;
                tex.height = desc.height;
                tex.volumeDepth = desc.volumeDepth;
                tex.graphicsFormat = desc.graphicsFormat;
                recreated = true;
            }
        }
        if (tex.IsCreated() == false)
        {
            tex.Create();
        }
        tex.name = name;
        return (recreated);
    }

    public static RenderTextureDescriptor GetDescriptorBase(int width, int height, int depth = 1)
    {
        RenderTextureDescriptor desc = new RenderTextureDescriptor(width, height);
        desc.mipCount = 0;
        desc.msaaSamples = 0;
        desc.volumeDepth = depth;
        if (depth == 1)
            desc.dimension = TextureDimension.Tex2D;
        else
            desc.dimension = TextureDimension.Tex3D;
        desc.enableRandomWrite = true;
        return (desc);
    }

    public static RenderTextureDescriptor GetDescriptorNoise1D_R(int dimension)
    {
        RenderTextureDescriptor desc = GetDescriptorBase(dimension, 1);
        desc.graphicsFormat = GraphicsFormat.R16_UNorm;
        return desc;
    }

    public static RenderTextureDescriptor GetDescriptorNoise2D_R(int dimension)
    {
        RenderTextureDescriptor desc = GetDescriptorBase(dimension, dimension);
        desc.graphicsFormat = GraphicsFormat.R16_UNorm;
        return desc;
    }

    public static RenderTextureDescriptor GetDescriptorNoise2D_RGBA(int dimension)
    {
        RenderTextureDescriptor desc = GetDescriptorBase(dimension, dimension);
        desc.graphicsFormat = GraphicsFormat.R16G16B16A16_UNorm;
        return desc;
    }

    public static RenderTextureDescriptor GetDescriptorNoise3D_RGBA(int dimension)
    {
        RenderTextureDescriptor desc = GetDescriptorBase(dimension, dimension, dimension);
        desc.graphicsFormat = GraphicsFormat.R16G16B16A16_UNorm;
        return desc;
    }
}
