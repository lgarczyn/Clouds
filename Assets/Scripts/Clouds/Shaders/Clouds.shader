
Shader "Clouds"
{

    Properties
    {
        [NoScaleOffset]ShapeTex ("shape", 3D) = "white" {}
        [NoScaleOffset]NoiseTex ("noise", 2D) = "white" {}
        [NoScaleOffset]ShadowMap ("shadows", 2D) = "black" {}
        [Toggle(OUTLINE_ON)]EnableOutlines("enable outlines", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        Cull Off ZWrite Off

        Pass
        {
            Name "CloudsGen"

            HLSLPROGRAM
            #pragma shader_feature OUTLINE_ON

            #ifdef OUTLINE_ON
            // Support for Gather requires 4.1, but can only target to 5.0 
    		#pragma target 5.0
            #endif
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // input structure (Attributes) and output strucutre (Varyings)
            // #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Extracted from Blit
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            TEXTURE2D_X(_BlitTexture);

            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD1
            
            #define REQUIRE_DEPTH_TEXTURE

            #if SHADER_API_GLES
            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            #else
            struct Attributes
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            #endif

            #pragma vertex vert
            #pragma fragment frag
            
            CBUFFER_START(UnityPerMaterial)
            float4 _BlitTexture_TexelSize;
            
            // Shadowmap settings
            float4 ShadowMap_TexelSize;
            static const float4 shadowMapAbsorptionLevels = float4(0.6, 0.4, 0.2, 0.01);
            float shadowMapHalfSize;
            float3 shadowMapPosition;
            float shadowMapNearPlane;
            float shadowMapFarPlane;
            float outOfBoundMaxLightAltitude;
            float outOfBoundMinLightAltitude;

            // Editor settings
            float4 testParams;
            float4 testColor;
            bool enabled;

            // Shape settings
            // TODO: reorganize parameters (eg. phase into light setting)
            int3 mapSize;
            // Pretty self-explanatory noise combination parameters
            float minTransmittance;
            float maxDensity;

            // Parameters for each fractal noise layer
            float scaleGlobal;
            float weightGlobal;
            float3 windDirection;
            float scale3;
            float scale2;
            float scale1;
            float scale0;
            float weight3;
            float weight2;
            float weight1;
            float weight0;
            float speed3;
            float speed2;
            float speed1;
            float speed0;

            // Used for silverlining while looking at the sun, currently broken
            float4 phaseParams;
            // Effects
            float psychedelicEffect;
            // LOD Settings
            float lodLevelMagnitude;
            float lodMinDistance;
            // Private parameters to recreate altitudeAtlas's range
            float altitudeOffset;
            float altitudeMultiplier;
            float4 altitudeValueOffsets;
            float4 altitudeValueMultipliers;

            // March settings
            int numStepsLight;
            float stepSizeRender;
            float firstStepNoiseMultiplier;
            float stepSizeNoiseRatio;
            // Two opposite corners of the cloud container
            float3 boundsMin;
            float3 boundsMax;

            // Light settings
            float3 _WorldSpaceLightPos0;
            float lightAbsorptionTowardSun;
            float lightAbsorptionThroughCloud;
            float hazeColorFactor;
            float hazeColorFactorLinear;
            float hazeTransmittanceFactor;
            float atmosphereTransmittancePower;
            float cloudTransmittancePower;
            float hdrMinSourceValue;
            float hdrMaxSourceValue;
            float hdrTransmittancePower;
            float lightPower;
            float darknessThreshold;
            float4 _LightColor0;
            float3 colA;
            float3 colB;
            float3 colC;
            float3 colD;

            // Animation settings
            float timeScale;

            // Player settings
            float3 playerPosition;
            
            // Outline setting
            #ifdef OUTLINE_ON
            float3 outlineColor;
            float outlineThreshold;
            float outlineStrength;
            #endif

            CBUFFER_END

            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 viewVector : TEXCOORD2;
                #if UNITY_ANY_INSTANCING_ENABLED
                 uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                 uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                 uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
            };

            Varyings vert(Attributes input) {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            #if SHADER_API_GLES
                float4 pos = input.positionOS;
                float2 uv  = input.uv;
            #else
                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
            #endif
            

                output.positionCS = pos;
                output.uv = uv;
                
                #if UNITY_UV_STARTS_AT_TOP
                if (_BlitTexture_TexelSize.y < 0)
                    uv.y = 1-uv.y;
                #endif
                // Render settings
                float near = _ProjectionParams.y;
                float far = _ProjectionParams.z;
                float2 orthoSize = unity_OrthoParams.xy;
 
                if (_ProjectionParams.x < 0)
                    pos.y = -pos.y;

                if (unity_OrthoParams.w)
                {
                    float4 viewVector = float4(0,0,1,0);
                    output.viewVector = mul(unity_CameraToWorld, float4(viewVector)).xyz;

                    float4 worldPos = float4(pos.xy * orthoSize, near, 1);
                    output.worldPos = mul(unity_CameraToWorld, float4(worldPos)).xyz;
                }
                else
                {
                    float3 viewVector = mul(unity_CameraInvProjection, float4(pos.xy, 1, -1)).xyz;
                    output.viewVector = mul(unity_CameraToWorld, float4(viewVector,0)).xyz;

                    float3 worldPos = mul(unity_CameraInvProjection, float4(pos.xy, near, -1)).xyz;
                    output.worldPos = mul(unity_CameraToWorld, float4(worldPos,1)).xyz;
                }

                return output;
            }

            // Textures
            TEXTURE2D_FLOAT(_CameraDepthTexture);
            // The main cloud texture
            Texture3D<float> ShapeTex;
            // 1D texture to give the 'thunderhead''vibe
            Texture2D<float4> AltitudeAtlas;
            // 2D texture to fix banding
            Texture2D<float> NoiseTex;
            // 2D texture containing the heights of 4 absorption levels
            Texture2D<float4> ShadowMap;

            SamplerState sampler_PointClamp;
            SAMPLER(sampler_CameraDepthTexture);
            SAMPLER(samplerShapeTex);
            SAMPLER(samplerNoiseTex);
            SAMPLER(samplerAltitudeAtlas);
            SamplerState samplerShadowMapPointRepeat;


            float remap01(float v, float low, float high) {
                return (v-low)/(high-low);
            }
            float remap01Clamped(float v, float low, float high) {
                return saturate(remap01(v, low, high));
            }

            // Maps a float from an interval to another, without bound checking
            float remap(float v, float minOld, float maxOld, float minNew, float maxNew) {
                return minNew + (v-minOld) * (maxNew - minNew) / (maxOld-minOld);
            }
            // Maps a float from an interval to another, with bound checking
            float remapClamped(float v, float minOld, float maxOld, float minNew, float maxNew) {
                // return clamp(remap(clamp(v, minOld, maxOld), minOld, maxOld, minNew, maxNew), minNew, maxNew);
                float f = saturate(remap01(v, minOld, maxOld));
                return minNew + f * (maxNew - minNew);
            }

            float2 squareUV(float2 uv) {
                float width = _ScreenParams.x;
                float height =_ScreenParams.y;
                //float minDim = min(width, height);
                float scale = 1000;
                float x = uv.x * width;
                float y = uv.y * height;
                return float2 (x/scale, y/scale);
            }

            // Returns (dstToBox, dstInsideBox). If ray misses box, dstInsideBox will be zero
            float2 rayBoxDst(float3 boundsMin, float3 boundsMax, float3 rayOrigin, float3 invRaydir) {
                // Adapted from: http://jcgt.org/published/0007/03/04/
                float3 t0 = (boundsMin - rayOrigin) * invRaydir;
                float3 t1 = (boundsMax - rayOrigin) * invRaydir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);

                float dstA = max(max(tmin.x, tmin.y), tmin.z);
                float dstB = min(tmax.x, min(tmax.y, tmax.z));

                // CASE 1: ray intersects box from outside (0 <= dstA <= dstB)
                // dstA is dst to nearest intersection, dstB dst to far intersection

                // CASE 2: ray intersects box from inside (dstA < 0 < dstB)
                // dstA is the dst to intersection behind the ray, dstB is dst to forward intersection

                // CASE 3: ray misses box (dstA > dstB)

                float dstToBox = max(0, dstA);
                float dstInsideBox = max(0, dstB - dstToBox);
                return float2(dstToBox, dstInsideBox);
            }

            // Same code, but ignores the x and z bounds
            float2 rayBoxDstVertical(float boundsMin, float boundsMax, float rayOrigin, float invRayDir)
            {
                float t0 = (boundsMin - rayOrigin) * invRayDir;
                float t1 = (boundsMax - rayOrigin) * invRayDir;
                float tmin = min(t0, t1);
                float tmax = max(t0, t1);

                float dstA = tmin;
                float dstB = tmax;

                float dstToBox = max(0, dstA);
                float dstInsideBox = max(0, dstB - dstToBox);
                return float2(dstToBox, dstInsideBox);
            }

            // Henyey-Greenstein
            float hg(float a, float g) {
                float g2 = g*g;
                return (1-g2) / (4*3.1415*pow(max(1+g2-2*g*(a), 0), 1.5));
            }

            float phase(float a) {
                float blend = .5;
                float hgBlend = hg(a,phaseParams.x) * (1-blend) + hg(a,-phaseParams.y) * blend;
                return phaseParams.z + hgBlend*phaseParams.w;
            }

            float beer(float d) {
                return exp(-d);
            }

            float reverse_beer(float d) {
                return -log(d);
            }

            float4 altitudeDensity(float height)
            {
                float texturePos = (height + altitudeOffset) * altitudeMultiplier;
                float4 textureValue = AltitudeAtlas.SampleLevel(samplerAltitudeAtlas, texturePos, 0);
                return textureValue * altitudeValueMultipliers + altitudeValueOffsets;
            }

            float2 returnDensity(float density, float prevDensity, float ratio, float haze) {
              return float2(
                lerp(density, prevDensity, ratio),
                haze
              );
            }

            float2 sampleDensity(float3 rayPos, int optimisation, float optiInterpolation) {
                // Calculate texture sample positions
                float time = _Time.x * timeScale;
                float3 uvw = rayPos / scaleGlobal;

                // optimisation is set based on distance, and always uniform to avoid branching
                // later iterations of the loops have higher optimization values

                // optiInterpolation allows smooth lerping between optimization levels
                // it approaches 1 rapidly when reaching the end of the loop

                // SampleLevel is used instead of Sample, because
                // * MIP is always 0 because of 3d textures
                // * Sample does not allow arbitrary depth loops

                float2 altitudeValues = altitudeDensity(rayPos.y).xy;
                float density = altitudeValues.x;
                float haze = altitudeValues.y;
                float prevDensity = density;

                // Early return for distant object
                // Interpolates between the most detailed version available and the previous one
                // This erases the sharp border between levels of details
                // Here of course both versions are the same, but the following returns are not 
                if (optimisation > 5)
                    return returnDensity(density, prevDensity, optiInterpolation, haze);

                float3 wind = windDirection * (_Time.x * timeScale);

                {
                    float3 samplePos = (uvw.zxy + wind * speed3) / scale3;
                    float value = ShapeTex.SampleLevel(samplerShapeTex, samplePos, 0);
                    prevDensity = density;
                    density += (value - 0.5) * scale3 * (weight3 * weightGlobal);
                }

                if (optimisation > 4)
                    return returnDensity(density, prevDensity, optiInterpolation, haze);

                {
                    float3 samplePos = (uvw + wind * speed2) / scale2;
                    float value = ShapeTex.SampleLevel(samplerShapeTex, samplePos, 0);
                    prevDensity = density;
                    density += (value - 0.5) * scale2 * (weight2 * weightGlobal);
                }

                if (optimisation > 3)
                    return returnDensity(density, prevDensity, optiInterpolation, haze);

                {
                    float3 samplePos = (uvw + wind * speed1) / scale1;
                    float value = ShapeTex.SampleLevel(samplerShapeTex, samplePos, 0);

                    prevDensity = density;
                    density += (value - 0.5) * scale1 * (weight1 * weightGlobal);
                }

                if (optimisation > 2)
                    return returnDensity(density, prevDensity, optiInterpolation, haze);

                {
                    float3 samplePos = (uvw + wind * speed0) / scale0;
                    float value = ShapeTex.SampleLevel(samplerShapeTex, samplePos, 0);

                    prevDensity = density;
                    density -= (value - 0.5) * scale0 * (weight0 * weightGlobal);
                }

                return returnDensity(density, prevDensity, optiInterpolation, haze);
            }

            float sampleLightmap(float2 uv, float height)
            {
                // The absorption layers of the shadow map
                float4 heights = ShadowMap.SampleLevel(samplerShadowMapPointRepeat, uv, 0);

                // The absorption fo the layer above and below
                float2 res;
                // The height of the layer above and below, for interpolation
                float2 range;

                // Selecting the right range, absorption and colors
                // Could be done with a few one-liners at the price of readability
                if (height > heights.x)
                {
                    range = float2(1, heights.x);
                    res = float2(1, shadowMapAbsorptionLevels.x);
                }
                else if (height > heights.y)
                {
                    range = float2(heights.x, heights.y);
                    res = float2(shadowMapAbsorptionLevels.x,shadowMapAbsorptionLevels.y);
                }
                else if (height > heights.z)
                {
                    range = float2(heights.y, heights.z);
                    res = float2(shadowMapAbsorptionLevels.y,shadowMapAbsorptionLevels.z);
                }
                else if (height > heights.w)
                {
                    range = float2(heights.z, heights.w);
                    res = float2(shadowMapAbsorptionLevels.z,shadowMapAbsorptionLevels.w);
                }
                else
                {
                    range = float2(heights.w, 0);
                    res = float2(shadowMapAbsorptionLevels.w, 0);
                }

                // The ratio of the layers above and below for mixing
                float rangeRatio = (height - range.x) / (range.y - range.x);
                if (isfinite(rangeRatio) == false)
                    rangeRatio = 0;

                // Interpolation of the absorption layers
                float absorption = lerp(res.x, res.y, rangeRatio);

                return absorption;
            }

            // the max absolute component of a vector
            // used for quadratic scaling of the shadowmap sampling
            float manhattanLength(float2 a) {
              float2 m = abs(a);
              return max(m.x, m.y);
            }

            // Calculate proportion of light that reaches the given point from the lightsource
            float lightmarch(float3 position) {
                float posY = position.y;
                float3 lightDir = _WorldSpaceLightPos0;
                // How much the sample should be offset due to the angle of the sun
                float2 shearOffset = (posY / -lightDir.y) * lightDir.xz;
                // The position in the shadow volume
                float2 centeredPosition = position.xz + shearOffset - shadowMapPosition.xz;
                // The [-1;1] position in the shadow volume
                float2 scaledPosition = centeredPosition / shadowMapHalfSize;
                float mDistanceFromCenter = manhattanLength(scaledPosition); 

                // calculate a replacement value using altitude
                float simulatedSample = remapClamped(posY, // Altitude
                   outOfBoundMinLightAltitude, outOfBoundMaxLightAltitude, // Range of altitudes gradient
                   darknessThreshold, 1 // Output range
                );

                // If the position is out of the texture, return the fake sample
                if (mDistanceFromCenter > 1) {
                  return simulatedSample;
                }
                
                // Remap the uv coordinates to a quadratic model
                // This allows for higher resolution shadows close to the player
                // by giving a larger area to elements closer to the center line
                // manhattan length is used instead of length, to avoid wasting the corners of the shadowmap
                scaledPosition = scaledPosition / sqrt(mDistanceFromCenter);

                // The position inside the shadow map texture, range [0; 1]
                float2 samplePos = scaledPosition / 2 + 0.5;

                // The height inside the container, from 0 to 1
                float height = ((posY - shadowMapFarPlane) / (shadowMapNearPlane - shadowMapFarPlane));

                if (height < 0)
                    return (darknessThreshold);
                if (height > 1)
                    return (1);

                {
                  // Multiple samples
                  // Offset second sample by 1 pixel times the golden ratio
                  // TODO: try to get better results by more random sampling
                  // float3 st = float3(ShadowMap_TexelSize.xy, 0) * 1.618;
                  
                  // float2 realUV = samplePos * ShadowMap_TexelSize.zw;
                  // float2 ratio = frac(realUV);
                  // float2 uv = floor(realUV) / ShadowMap_TexelSize.zw;
                  // TODO: use Gather instead of sample to reduce sample by 2
                  // float TL = sampleLightmap(uv, height);
                  // float TR = sampleLightmap(uv + st.xz, height);
                  // float BL = sampleLightmap(uv + st.zy, height);
                  // float BR = sampleLightmap(uv + st.xy, height);

                  // Average the samples depending on pos
                  // float T = lerp(TL, TR, ratio.x);
                  // float B = lerp(BL, BR, ratio.x);
                  // float ret = lerp(T, B, ratio.y);
                }
                float ret =  sampleLightmap(samplePos, height);
                // Remap range to minimum light
                ret = lerp(darknessThreshold, 1, ret);

                // calculate how much of the simulation should be used
                // allows for a softer boundary with the fake samples
                float simulationInterpolation = pow(mDistanceFromCenter, 8);

                // Interpolate and return
                return lerp(ret, simulatedSample, simulationInterpolation);
            }
            
            // The previous lightmarching code, for result comparison
            // Could be used with the shadowmaps as a hinting mechanism for more dynamic shadow maps
            float4 old_lightmarch(float3 position)
            {
                float3 dirToLight = _WorldSpaceLightPos0;
                float dstInsideBox = (position.y - boundsMin.y);//rayBoxDst(boundsMin, boundsMax, position, 1/dirToLight).y;

                float stepSize = dstInsideBox/numStepsLight;
                position += dirToLight * stepSize * .5;
                float totalDensity = 0;
                float dstTravelled = 0;

                // TODO: check if branching is worth it
                for (int step = 0; step < numStepsLight; step ++) {
                    float density = sampleDensity(position, 0, 0).x;
                    totalDensity += max(0, density * stepSize);

                    //Variable stepping, using the noise like a signed-distance-function
                    //Works well if hard contrast is avoided
                    float dst = stepSize * max(abs(density), 1);
                    position += dirToLight * dst;
                    dstTravelled += dst;
                    //Try early returning if less than 0.01 is passing through
                    // if (totalDensity > -log(0.2) * lightAbsorptionTowardSun)
                    //     break;
                    //Due to variable stepping, skip if end is reached
                    if (dstTravelled >= dstInsideBox)
                        break;
                }
                float transmittance = beer(totalDensity * lightAbsorptionTowardSun);

                return lerp(darknessThreshold, 1, transmittance);
            }

            // Attempts at compressing ordered float4
            // currently unused and unworking
            float4 compressLightData(float4 data) {
                //data.gba = lerp(data.rgb, float3(1,1,1), data.gba);
                //data.gba = lerp(data.rgb, data.gba, data.gba)
                data.a = (data.a - data.b) / (1-data.b);
                data.b = (data.b - data.g) / (1-data.g);
                data.g = (data.g - data.r) / (1-data.r);
                return data;
            }
            float4 decompressLightData(float4 data) {
                float4 result;

                result.r = data.r;
                result.g = data.r + data.g * (1 - data.r);
                result.b = data.g + data.b * (1 - data.g);
                result.a = data.b + data.a * (1 - data.b);
                return result;
            }

            float4 shadowMarch(float3 position, float3 direction, float2 uv) {
                // quadratic remapping of the position, to increase resolution on center of shadowmap
                // could be much simpler if we could calculate `length(scaledPosition)` in advance using uvs
                // but it doesn't seem to work
                {
                    float3 lightDir = _WorldSpaceLightPos0;
                    // how much the top of the shadow volume is offset from the middle
                    float3 shadowMapStartOffset = (shadowMapNearPlane / lightDir.y) * lightDir.xyz;
                    shadowMapStartOffset.y = 0;
                    // the center of the top square of the shadow volume
                    float3 shadowMapStartPos = shadowMapPosition + shadowMapStartOffset;
                    // The position relative to that square
                    float3 centeredPosition = position - shadowMapStartPos;
                    // Remap to range [-1;1]
                    float2 scaledPosition = centeredPosition.xz / shadowMapHalfSize;
                    // Square the variations to bias them closer to 1
                    centeredPosition.xz = centeredPosition.xz * manhattanLength(scaledPosition);

                    position = centeredPosition + shadowMapStartPos;
                }

                // 'shadowMapAbsorptionLevels' represents the absorption levels for which a sun distance is stored
                float4 targets = shadowMapAbsorptionLevels;
                // 'targetDensity' is the associated total densities needed to reach them
                float4 targetDensity = float4(
                    reverse_beer(targets.x),
                    reverse_beer(targets.y),
                    reverse_beer(targets.z),
                    reverse_beer(targets.w));
                targetDensity /= lightAbsorptionTowardSun;
                // The distances (as a ratio of the container) that light has traveled
                // before reaching an absorption threshold
                float4 result = float4(1,1,1,1);
                // The current threshold
                int targetIt = 0;

                // The distance from the container and the distance to cross inside the container
                float2 boxParams = rayBoxDstVertical(shadowMapFarPlane, shadowMapNearPlane, position.y, 1/direction.y);
                float distanceToBox = boxParams.x;
                float distanceToTravel = boxParams.y;

                // adjust depth depending on shadow caster depth map
                // allows opaque geometry to create shadows
                // currently disabled here, in the shadow pipeline and the shadow camera
                // before adding back, test uv quadratic mapping

                // Get the depth value
                // float depth = getDepth(input.uv); // TODO add uv quadratic effect
                // depth -= distanceToBox;
                // if (depth < 0)
                //     return float4(1,1,1,1);
                // float distanceToTravelWithDepth = min(distanceToTravel, depth);

                // Numbers of measurements of the cloud density
                float stepSize = distanceToTravel / numStepsLight;

                // Adjust the position inside the container, and add half a step
                position += direction * (stepSize * .5 + distanceToBox);

                float totalDensity = 0;
                float dstTravelled = 0;

                // TODO: consider adding step-back for more precision
                for (int i = 0; i < numStepsLight; i++) {
                // while (dstTravelled < distanceToTravelWithDepth) {

                    // Sample current density
                    float density = sampleDensity(position, 4, 0).x;
                    // Modify the stepsize for variable stepping (less artifacts)
                    float dst = stepSize * max(abs(density), 1);

                    // Increment density counter
                    totalDensity += max(0, density * stepSize);

                    // If density reached current threshold
                    if (totalDensity >= targetDensity.x)
                    {
                        // Calculate the current ratio of the distance to cross:
                        float res = (dstTravelled + stepSize * 4) / distanceToTravel;

                        // Tries to calculate the approximate distance after which threshold was met
                        // minimal difference, so disabled for now
                        // float approxDistance = dstTravelled - lastStepSize * (totalDensity - targetDensity.x) / (addedDensity);
                        // float res = approxDistance / distanceToTravel;

                        // store that ratio
                        result.x = res;
                        // rotate both result and threshold for the next position
                        result.xyzw = result.yzwx;
                        targetDensity.xyzw = targetDensity.yzwx;
                        targetIt++;
                        // If threshold was the last one, break
                        // could be done by checking (result.x != 1), but targetIt is needed for rotating the result buffer
                        if (targetIt >= 4)
                            break;
                    }
                    // Variable stepping, using the noise like a signed-distance-function
                    // Works well if hard contrast is avoided
                    position += direction * dst;
                    // Advance through volume
                    dstTravelled += dst;
                }
                // If the last thresholds were never reach, set them to either 1, or to the ratio of the depth
                // Currently uneeded, as light never reaches past the end of the container
                for (;targetIt < 4; targetIt++)
                    result.xyzw = result.yzwx; //(result * float4(0,1,1,1) + float4(dstTravelled / distanceToTravel, 0,0,0)).yzwx;

                // Inverse the result, for use in the lightmarch function
                return 1 - saturate(result);
            }

            float cinematicGradient(float i, uniform float power)
            {
                if (i > 0.5)
                {
                    i -= 0.5;
                    i = pow(i * 2, 1 / power) / 2 + 0.5;
                }
                else if (i)
                    i = pow(i * 2, power) / 2;
                return (i);
            }

            float cinematicGradient(float i, uniform float power, uniform float middle)
            {
                if (i > middle)
                {
                    i -= middle;
                    i = pow(i / (1 - middle), 1 / power) * (1 - middle);
                    i += middle;
                }
                else if (i)
                    i = pow(i / middle, power) * middle;
                return (i);
            }

             
            float LinearEyeDepth( float rawdepth )
            {
                float x, y, z, w;
            #if SHADER_API_GLES3 // insted of UNITY_REVERSED_Z
                x = -1.0 + _ProjectionParams.y/ _ProjectionParams.z;
                y = 1;
                z = x / _ProjectionParams.y;
                w = 1 / _ProjectionParams.y;
            #else
                x = 1.0 - _ProjectionParams.y/ _ProjectionParams.z;
                y = _ProjectionParams.y / _ProjectionParams.z;
                z = x / _ProjectionParams.y;
                w = y / _ProjectionParams.y;
            #endif
             
              return 1.0 / (z * rawdepth + w);
            }

            float getDepth(float2 uv)
            {
                float nonlin_depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(uv));

                if (unity_OrthoParams.w)
                {
                    #ifdef UNITY_REVERSED_Z
                    return lerp(_ProjectionParams.z, _ProjectionParams.y, nonlin_depth) - _ProjectionParams.y;
                    #else
                    return lerp(_ProjectionParams.y, _ProjectionParams.z, nonlin_depth) - _ProjectionParams.y;
                    #endif
                }
                else
                    return LinearEyeDepth(nonlin_depth);
            }
            
            // Retrieve 4 points in the depth texture
            float4 getDepthMultiSample(float2 uv)
            {
                float4 nonlin_depth = _CameraDepthTexture.Gather(
                    sampler_CameraDepthTexture,
                    UnityStereoTransformScreenSpaceTex(uv),
                    float2 (0, 0));

                if (unity_OrthoParams.w)
                {
                    #ifdef UNITY_REVERSED_Z
                    return lerp(_ProjectionParams.z, _ProjectionParams.y, nonlin_depth) - _ProjectionParams.y;
                    #else
                    return lerp(_ProjectionParams.y, _ProjectionParams.z, nonlin_depth) - _ProjectionParams.y;
                    #endif
                }

                return float4(
                    LinearEyeDepth(nonlin_depth.x),
                    LinearEyeDepth(nonlin_depth.y),
                    LinearEyeDepth(nonlin_depth.z),
                    LinearEyeDepth(nonlin_depth.w)
                );
            }

            float3 getSunColor(float3 color, float3 rayDir, float transmittance)
            {
                // Phase function makes clouds brighter around sun
                float3 lightDir = _WorldSpaceLightPos0;
                float cosAngle = dot(rayDir, lightDir);
                float phaseVal = phase(cosAngle);

                // Add the sun
                float focusedEyeCos = pow(saturate(cosAngle), 0.9);
                float sun = saturate(hg(focusedEyeCos, .995)) * transmittance;

                return lerp(color * phaseVal, normalize(_LightColor0.rgb) * 4, sun);
            }

            float3 getCloudColor(float lightEnergy, float hazeRatio, float depthRatio)
            {
                // Apply to both cloud colors
                // float3 colAf = lerp(colA, colC, hazeRatio);
                // float3 colBf = lerp(colB, colC, hazeRatio);
                // Chose which gradient to apply
                // TODO: use a better way to lerp through these
                float3 cloud;
                if (lightEnergy < 0.5)
                    cloud = lerp(colB, colA, lightEnergy * 2);
                else
                    cloud = lerp(colA, _LightColor0.rgb, (lightEnergy - 0.5) * 2);
                cloud = lerp(cloud, colD, saturate(depthRatio));
                return lerp(cloud, colC, saturate(hazeRatio));
            }

            float4 rayMarch(float3 rayPos, float3 rayDir, float2 uv);

            float luminance(float3 color)
            {
                return dot(color, float3(0.299f, 0.587f, 0.114f));
            }

            float4 frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                // Create ray
                float3 rayPos = input.worldPos;
                float3 rayDir = input.viewVector;
                float2 uv = input.uv;

                // If using an ortho camera for shadow mode
                if (unity_OrthoParams.w)
                    return shadowMarch(rayPos, rayDir, uv);

                // If one of four corner pixels
                float2 pixelSize = 1 / _ScreenParams.xy;
                if ((uv.x <= pixelSize.x || uv.x >= 1 - pixelSize.x) &&
                    (uv.y <= pixelSize.y || uv.y >= 1 - pixelSize.y))
                {
                    // Store data for gameplay
                    float playerDensity = sampleDensity(playerPosition, 0, 0).x;
                    float playerLight = lightmarch(playerPosition);
                    return float4(
                      playerDensity / 100 + 0.5,
                      playerLight,
                      0.5, 0.5);
                }

                return rayMarch(rayPos, rayDir, uv);
            }

            float4 rayMarch(float3 rayPos, float3 rayDir, float2 uv) {

                // Skybox and plane
                float3 backgroundCol = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv).rgb;

                // Get the depth value(s)  
                float depth;
            #ifdef OUTLINE_ON
                {
                    float4 depth4 = getDepthMultiSample(uv);
                    float divergence = dot(depth4, float4(1,1,1,1)) / (length(depth4));
                    depth = min(min(depth4.x, depth4.y), min(depth4.x, depth4.y));

                    backgroundCol = lerp(
                        backgroundCol,
                        outlineColor,
                        saturate((outlineColor - divergence) * outlineStrength)
                    );
                }
            #else
                depth = getDepth(uv);
            #endif

                // If in editor, do not run
                // Incorrect params in editor tend to crash it
                if (!enabled) return float4(backgroundCol, 1);

                // Normalize ray because of perspective interpolation
                float distancePerspectiveModifier = length(rayDir);
                rayDir = rayDir / distancePerspectiveModifier;

                // If an object was drawn except the skybox
                // Usually the plane
                // TODO: add check that the object is inside the skybox ?
                bool hiddenByObject = abs(depth - _ProjectionParams.z) > 100;
                // Normalize depth the same way
                depth *= distancePerspectiveModifier;

                float2 rayToContainerInfo = rayBoxDst(boundsMin, boundsMax, rayPos, 1/rayDir);
                float dstToBox = rayToContainerInfo.x;
                float dstInsideBox = rayToContainerInfo.y;

                
                // Random offset to the start and speed of raymarching
                // Avoids some banding
                float noiseSample = NoiseTex.SampleLevel(samplerNoiseTex, uv * 10 + _Time.xy * 97 , 0);
                float startNoiseOffset = (noiseSample - 0.5)
                    * firstStepNoiseMultiplier * 3;
                float stepsizeNoiseComponent = noiseSample
                    * stepSizeRender * stepSizeNoiseRatio;

                // point of intersection with the cloud container
                float3 entryPoint = rayPos + rayDir * (dstToBox + startNoiseOffset);
                float stepSize = stepSizeRender + stepsizeNoiseComponent;

                // Adds an empty sphere around the camera
                // float dstTravelled = max(400 - dstToBox, 0);
                float dstTravelled = 0;
                float dstLimit = min(depth-dstToBox, dstInsideBox);
                float averageDst = dstToBox;

                // March through volume:
                float transmittance = 1;
                float lightEnergy = 0;
                float hazeRatio = 0;

                // max render distance = lodMinDistance + pow(lodLevelMagnitude, 5)
                // but container limits the raycasting anyway
                for (int i = 1; i < 7; i++)
                {
                    float lodDst = pow(max(lodLevelMagnitude, 0), i);
                    if (i == 5) lodDst += testParams.x;
                    if (i == 6) lodDst += testParams.y;
                    float lodMaxDistance = lodMinDistance + lodDst;
                    float localMax = min(dstToBox + dstLimit, lodMaxDistance) - dstToBox;
                    float start = dstTravelled;
                    while (dstTravelled < localMax) {

                        float loopRatioLinear = (dstTravelled - start) / (localMax - start);
                        float loopRatio = loopRatioLinear * loopRatioLinear;
                        loopRatio *= loopRatio;

                        rayPos = entryPoint + rayDir * dstTravelled;
                        float2 densities = sampleDensity(rayPos, i, loopRatio);
                        float density = densities.x;
                        float haze = densities.y / hazeTransmittanceFactor;
                        density = min(maxDensity, density);

                        // Calculate the total density the ray has hit on this step
                        float realDensity = max(max(density,haze), 0) * stepSize - psychedelicEffect;
                        // Calculate how much the ray should move forward depending on density
                        float realStepSize = max(abs(density), 0.05) * stepSize;
                        // Calculate the amount of light at this position

                        // Calculate the absorption based on density and distance moved
                        float beerRes = beer(realDensity * lightAbsorptionThroughCloud);
                        float oldTransmittance = transmittance;
                        // update transmittance
                        transmittance *= beerRes;

                        // Calculate how much of the loss in transmittance is due to haze
                        hazeRatio += max(haze - density, 0) * (oldTransmittance - transmittance);
                        
                        // Calculate the amount of light emitted from that area
                        float lightTransmittance = lightmarch(rayPos) * beerRes;
                        // Multiply by density of the area and visibility of the pixel
                        lightTransmittance *= realDensity * transmittance;
                        // Add to light energy accumulator
                        lightEnergy += lightTransmittance;

                        // Move forward
                        dstTravelled += realStepSize;
                        averageDst += realStepSize * transmittance;

                        // Exit early if T is close to zero as further samples won't affect the result much
                        if (transmittance < minTransmittance) {
                            break;
                        }
                    }
                    // Truly exit the loop
                    if (transmittance < minTransmittance) {
                        break;
                    }
                }
                if (depth > dstTravelled)
                    hiddenByObject = false;

                // Correct haze ratio
                // Series of correction allows independent modifications of haze color, strength and distance
                hazeRatio *= hazeTransmittanceFactor / hazeColorFactor / lightAbsorptionThroughCloud;
                // Correct transmittance calculations for full range
                transmittance = saturate(remap01(transmittance, minTransmittance, 1));
                // Control how much light reaches skybox or object
                // Basically make entire shader less transparent
                transmittance = pow(transmittance,
                    hiddenByObject ? cloudTransmittancePower : atmosphereTransmittancePower);
                // Correct light energy calculations
                // Allows independent change lightPower and light absorption
                lightEnergy *= lightPower * lightAbsorptionThroughCloud;
                // When absorption (1 - transmittance) is low, less light energy has accumulated
                // This value accounts for that
                float lowAbsorptionLightBalance = transmittance == 1 ? 1 : 1 / (1-transmittance);
                lightEnergy *= lowAbsorptionLightBalance;


                float currentDepth;
                if (dstInsideBox > 0)
                    currentDepth = dstToBox + dstTravelled;
                else
                    currentDepth = depth;

                // Pull colors towards haze color by linear distance                
                float depthRatio = saturate(averageDst / hazeColorFactorLinear);


                // Add shading to non-cloud objects
                // Could be done better by decoding normals
                // if (hiddenByObject)
                //     backgroundCol *= lerp(lightmarch(rayPos + rayDir * currentDepth), 1, 0.9);
                // Code above doesn't work for obscure reasons

                // Add clouds
                // Get the cloud color depending on hazeAmount and adjusted light energy
                float3 col = getCloudColor(lightEnergy, hazeRatio, depthRatio);

                float l = luminance(backgroundCol);
                float hdrFactor = remapClamped(l,
                    hdrMinSourceValue, hdrMaxSourceValue,
                    1, 1 / hdrTransmittancePower);

                transmittance = pow(transmittance, hdrFactor);                

                // Add background or plane/objects
                col = lerp(col, backgroundCol, transmittance);

                // col + (backgroundCol - col ) * transmittance

                // if (luminance(backgroundCol) > 1)
                //     col += backgroundCol * pow(transmittance, 0.1);
                // col = backgroundCol * transmittance + col * (1 - transmittance);

                // Add sun and sun glow
                // TODO: fix failure when outside of bounding box
                // ie. sun appears in front of plane/whales
                if (hiddenByObject == false)
                    col = getSunColor(col, rayDir, transmittance);

                return float4(col,0);
            }

            ENDHLSL
        }
    }
}
