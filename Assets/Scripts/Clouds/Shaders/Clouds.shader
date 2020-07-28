
Shader "Hidden/Clouds"
{

    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        // material properties used exclusively to check if material cloning works
        NoiseTex ("noise", 3D) = "white" {}
        ShadowMap ("shadows", 2D) = "black" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "UnityCG.cginc"
            #include "Assets/Scripts/Clouds/Shaders/CloudDebug.cginc"
            float4 _MainTex_TexelSize;

            // vertex input: position, UV
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
                float3 worldPos: TEXCOORD2;
            };

            // Undocumented VR matrices
            // float4x4 _LeftWorldFromView;
            // float4x4 _LeftViewFromScreen;
            // #ifdef VR_MODE
            // float4x4 _RightWorldFromView;
            // float4x4 _RightViewFromScreen;
            // #endif

            // Vertex shader that procedurally outputs a full screen triangle
            v2f vert(appdata v)
            {

                #if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0)
                    v.uv.y = 1-v.uv.y;
                #endif
                // Render settings
                float near = _ProjectionParams.y;
                float far = _ProjectionParams.z;
                float2 orthoSize = unity_OrthoParams.xy;

                v2f o;
                float3 pos = UnityObjectToClipPos(v.vertex);
                // TODO: cheaper way to calculate clip pos, but sometimes breaks
                // float3(v.uv, 0) * float3(2,-2,0) - float3(1,-1,0); 
                o.pos = float4(pos, 1);
                if (_ProjectionParams.x < 0)
                    pos.y = -pos.y;
                o.uv = v.uv;

                if (unity_OrthoParams.w)
                {
                    float3 viewVector = float4(0,0,1,0);
                    o.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                    // o.viewVector = -_WorldSpaceLightPos0;

                    float4 worldPos = float4(float2(pos.x, pos.y) * orthoSize, near, 1);
                    o.worldPos = mul(unity_CameraToWorld, float4(worldPos));
                }
                else
                {
                    float3 viewVector = mul(unity_CameraInvProjection, float4(float2(pos.x, pos.y), 1, -1));
                    o.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));

                    float3 worldPos = mul(unity_CameraInvProjection, float4(float2(pos.x, pos.y), near, -1));
                    o.worldPos = mul(unity_CameraToWorld, float4(worldPos,1));
                }

                return o;
            }

            // Textures
            // The main cloud texture
            Texture3D<float4> NoiseTex;
            // Whisps and detailing
            Texture3D<float4> DetailNoiseTex;
            // 1D texture to give the 'thunderhead''vibe
            Texture2D<float> AltitudeMap;
            // 2D texture containing the heights of 4 absorption levels
            Texture2D<float4> ShadowMap;
            float4 ShadowMap_TexelSize;

            static const float4 shadowMapAbsorptionLevels = float4(0.6, 0.4, 0.2, 0.01);
            float shadowMapSize;

            SamplerState samplerNoiseTex;
            SamplerState samplerDetailNoiseTex;
            SamplerState samplerAltitudeMap;
            SamplerState samplerShadowMapPointRepeat;

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            // Shape settings
            // TODO: reorganize parameters (eg. phase into light setting)
            float4 params;
            int3 mapSize;
            // Pretty self-explanatory noise combination parameters
            float densityMultiplier;
            float visualDensityMultiplier;
            float densityOffset;
            float minTransmittance;
            float scale;
            float detailNoiseScale;
            float detailNoiseWeight;
            // Weights to balance the 4 channels of the detail shader
            float3 detailWeights;
            float4 shapeNoiseWeights;
            // Used for silverlining while looking at the sun, currently broken
            float4 phaseParams;
            // Parameters for the altitude taper
            float densityTaperUpStrength;
            float densityTaperUpStart;
            float densityTaperDownStrength;
            float densityTaperDownStart;
            // LOD Settings
            float lodLevelMagnitude;
            float lodMinDistance;
            // Private parameters to recreate altitudeMap's range
            float altitudeOffset;
            float altitudeMultiplier;

            // March settings
            int numStepsLight;
            float stepSizeRender;
            float rayOffsetStrength;
            // Two opposite corners of the cloud container
            float3 boundsMin;
            float3 boundsMax;
            // Used for offseting the noise patterns, currently unused
            float3 shapeOffset;
            float3 detailOffset;

            // Light settings
            float lightAbsorptionTowardSun;
            float lightAbsorptionThroughCloud;
            float darknessThreshold;
            float godRaysIntensity;
            float4 _LightColor0;
            float4 colA;
            float4 colB;
            float4 colC;

            // Animation settings
            float timeScale;
            float baseSpeed;
            float detailSpeed;
            float3 playerPosition;

            // Debug settings:
            // Allow the 'picture in a picture' noise editing tool
            // TODO: Will need to be heavily changed to remove this shader from the camera
            int debugViewMode; // 0 = off; 1 = shape tex; 2 = detail tex; 4 = altitudemap
            int debugGreyscale;
            int debugShowAllChannels;
            float debugNoiseSliceDepth;
            float4 debugChannelWeight;
            float debugTileAmount;
            float viewerSize;

            // Maps a float from an interval to another, without bound checking
            float remap(float v, float minOld, float maxOld, float minNew, float maxNew) {
                return minNew + (v-minOld) * (maxNew - minNew) / (maxOld-minOld);
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
                return (1-g2) / (4*3.1415*pow(1+g2-2*g*(a), 1.5));
            }

            float phase(float a) {
                float blend = .5;
                float hgBlend = hg(a,phaseParams.x) * (1-blend) + hg(a,-phaseParams.y) * blend;
                return phaseParams.z + hgBlend*phaseParams.w;
            }

            float beer(float d) {
                float beer = exp(-d);
                return beer;
            }

            float reverse_beer(float d) {
                return -log(d);
            }

            float remap01(float v, float low, float high) {
                return (v-low)/(high-low);
            }

            float altitudeDensity(float heightPercent)
            {
                return sqrt(AltitudeMap.SampleLevel(samplerAltitudeMap, heightPercent, 0)) * altitudeMultiplier + altitudeOffset;
            }

            float sampleDensity(float3 rayPos, uniform int optimisation, float optiInterpolation) {
                // Constants:
                const int mipLevel = 2;
                const float baseScale = 1/1000.0;

                // Calculate texture sample positions
                float time = _Time.x * timeScale;
                float3 size = boundsMax - boundsMin;
                float3 boundsCentre = boundsMin + size * .5;
                float3 uvw = (float3(3200, 1400, 3200) / 2 + rayPos) * baseScale * scale;

                // Sets a gradient tapering off at the top and bottom, avoiding ugly flat spots (which tend to look buggy)
                float gMin = 0.2;
                float gMax = 0.8;
                float heightPercent = (rayPos.y - boundsMin.y) / size.y;

                float heightDensityOffset = min(
                    min(
                        (heightPercent - densityTaperDownStart) * densityTaperDownStrength,
                        (densityTaperUpStart - heightPercent) * densityTaperUpStrength
                    ), 0);

                // optimisation is set based on distance, and always uniform to avoid branching
                // later iterations of the loops have higher optimization values
                if (optimisation > 4)
                    return heightDensityOffset + 3;

                float altDensity = altitudeDensity(heightPercent) / 2 + heightDensityOffset;

                // Calculate meta shape density
                // Duplicated code to create a meta layer of clouds
                // TODO: Fully seperate from normal noise settings
                float3 shapeSamplePosMeta = uvw;
                shapeSamplePosMeta /= 10;
                shapeSamplePosMeta.y /= 3;
                // shapeSamplePosMeta.y += (shapeSamplePosMeta.x + shapeSamplePosMeta.y) / 1000;

                float4 shapeNoiseMeta = NoiseTex.SampleLevel(samplerNoiseTex, shapeSamplePosMeta , mipLevel);
                float4 normalizedShapeWeightsMeta = shapeNoiseWeights / dot(shapeNoiseWeights, 1);
                float shapeFBMMeta = dot(shapeNoiseMeta, normalizedShapeWeightsMeta);
                float baseShapeDensityMeta = (shapeFBMMeta + densityOffset * .1 - 0.1) * 15;

                // Add altitude density
                // TODO: standardize height gradient inside density
                baseShapeDensityMeta += altDensity;

                // optiInterpolation allows smooth lerping between optimization levels
                // it approaches 1 rapidly when reaching the end of the loop
                if (optimisation > 3)
                    return lerp(baseShapeDensityMeta, heightDensityOffset + 3, optiInterpolation);

                // Early returning if further calculations is unlikely to affect results
                // TODO: actually check if early returning is worth it
                if (baseShapeDensityMeta < -1 - densityOffset / 10)
                    return baseShapeDensityMeta;

                // Attempt at writing a shockwave around the plane
                // float dist = length(rayPos - playerPosition);
                // baseShapeDensityMeta -= sin(dist / 1000) * 1000 / pow(dist / 10, 3);

                // Try early returning, might be ignored by compiler since forking is hard on GPU
                // if (baseShapeDensityMeta < -1)
                //     return baseShapeDensityMeta;

                // Calculate base shape density
                float3 shapeSamplePos = uvw + float3(time,time*0.1,time*0.2) * baseSpeed;
                float4 shapeNoise = NoiseTex.SampleLevel(samplerNoiseTex, shapeSamplePos, mipLevel);
                float4 normalizedShapeWeights = shapeNoiseWeights / dot(shapeNoiseWeights, 1);
                float shapeFBM = dot(shapeNoise, normalizedShapeWeights);
                float baseShapeDensity = shapeFBM + densityOffset * .1;

                baseShapeDensity += baseShapeDensityMeta;

                if (optimisation > 2)
                    return lerp(baseShapeDensity, baseShapeDensityMeta, optiInterpolation);
                // if (baseShapeDensity > detailNoiseWeight || baseShapeDensity < 0)
                //     return (baseShapeDensity);

                // Sample detail noise
                float3 detailSamplePos = uvw*detailNoiseScale + float3(time*.4,-time,time*0.1)*detailSpeed;
                float4 detailNoise = DetailNoiseTex.SampleLevel(samplerDetailNoiseTex, detailSamplePos, mipLevel);
                float3 normalizedDetailWeights = detailWeights / dot(detailWeights, 1);
                float detailFBM = dot(detailNoise, normalizedDetailWeights);

                // Subtract detail noise from base shape (weighted by inverse density so that edges get eroded more than centre)
                float oneMinusShape = 1 - shapeFBM;
                float detailErodeWeight = oneMinusShape * oneMinusShape * oneMinusShape;
                float cloudDensity = baseShapeDensity - (1-detailFBM) * detailErodeWeight * detailNoiseWeight; 
                cloudDensity *= densityMultiplier / 2;
                // cloudDensity = baseShapeDensity - cloudDensity;

                return lerp(cloudDensity, baseShapeDensity, optiInterpolation);
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

            // Calculate proportion of light that reaches the given point from the lightsource
            float lightmarch(float3 position) {

                // The position inside the shadow map
                float2 samplePos =
                    (((position.y / -_WorldSpaceLightPos0.y) * _WorldSpaceLightPos0).xz + position.xz) / (shadowMapSize * 2) + 0.5;
                // The height inside the container, from 0 to 1
                float height = ((position.y - boundsMin.y) / (boundsMax.y - boundsMin.y) );
                // samplePos += sin(position.x + position.y) / 10000 + cos((samplePos.x - samplePos.y) * 10000) / 10000;

                if (height < 0)
                    return (0);
                if (height > 1)
                    return (1);

                // float ret_tm = sampleLightmap(samplePos, height);
                // return lerp(darknessThreshold, 1, ret_tm);
                
                float3 st = float3(1 / 512.0, 1 / 512.0, 0);

                // float2 ratio = (samplePos % ShadowMap_TexelSize.xy + ShadowMap_TexelSize.xy) % ShadowMap_TexelSize.xy;
                // float2 ratio = samplePos % ShadowMap_TexelSize.xy;

                float2 realUV = samplePos * 512;
                float2 ratio = frac(realUV);
                float2 uv = floor(realUV) / 512;

                // TODO: use Gather instead of sample to reduce sample by 2
                float TL = sampleLightmap(uv, height);
                float TR = sampleLightmap(uv + st.xz, height);
                float BL = sampleLightmap(uv + st.zy, height);
                float BR = sampleLightmap(uv + st.xy, height);

                float T = lerp(TL, TR, ratio.x);
                float B = lerp(BL, BR, ratio.x);

                float ret = lerp(T, B, ratio.y);

                return lerp(darknessThreshold, 1, ret);
            }
            
            // The previous lightmarching code, for result comparison
            // Could be used with the shadowmaps as a hinting mechanism for more dynamic shadow maps
            float4 old_lightmarch(float3 position)
            {
                float3 dirToLight = _WorldSpaceLightPos0.xyz;
                float dstInsideBox = (position.y - boundsMin.y);//rayBoxDst(boundsMin, boundsMax, position, 1/dirToLight).y;

                float stepSize = dstInsideBox/numStepsLight;
                position += dirToLight * stepSize * .5;
                float totalDensity = 0;
                float dstTravelled = 0;

                // TODO: check if branching is worth it
                for (int step = 0; step < numStepsLight; step ++) {
                    float density = sampleDensity(position, 0, 0);
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

            float4 shadowMarch(float3 position, float3 direction, float depth) {

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
                float2 boxParams = rayBoxDstVertical(boundsMin.y, boundsMax.y, position.y, 1/direction.y);
                float distanceToBox = boxParams.x;
                float distanceToTravel = boxParams.y;
                float distanceToTravelWithDepth = min(distanceToTravel, depth);

                //adjust depth
                // BUG: player can cast shadow on clouds above
                // might want to remove external shadows altogether for now
                depth -= distanceToBox;
                if (depth < 0)
                    return float4(1,1,1,1);

                // Numbers of measurements of the cloud density
                float stepSize = distanceToTravelWithDepth / numStepsLight;

                // Adjust the position inside the container, and add half a step
                position += direction * (stepSize * .5 + distanceToBox);

                float totalDensity = 0;
                float dstTravelled = 0;

                // TODO: consider adding step-back for more precision
                for (int i = 0; i < numStepsLight; i++) {
                // while (dstTravelled < distanceToTravelWithDepth) {

                    // Sample current density
                    float density = sampleDensity(position, 3, 0);
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

            float4 debugDrawNoise(float2 uv) {

                float4 channels = 0;
                float3 samplePos = float3(uv.x,uv.y, debugNoiseSliceDepth);

                if (debugViewMode == 1) {
                    channels = NoiseTex.SampleLevel(samplerNoiseTex, samplePos, 0);
                }
                else if (debugViewMode == 2) {
                    channels = DetailNoiseTex.SampleLevel(samplerDetailNoiseTex, samplePos, 0);
                }
                else if (debugViewMode == 4) {
                    channels = AltitudeMap.SampleLevel(samplerAltitudeMap, samplePos.yy, 0);
                }

                if (debugShowAllChannels) {
                    return channels;
                }
                else {
                    float4 maskedChannels = (channels*debugChannelWeight);
                    if (debugGreyscale || debugChannelWeight.w == 1) {
                        return dot(maskedChannels,1);
                    }
                    else {
                        return maskedChannels;
                    }
                }
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

            float getDepth(float2 uv)
            {
                float nonlin_depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(uv));

                // TODO: figure out why negative near planes break the depths
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

            fixed3 getSunColor(fixed3 color, float3 rayDir, float transmittance)
            {
                // Phase function makes clouds brighter around sun
                float cosAngle = dot(rayDir, _WorldSpaceLightPos0.xyz);
                float phaseVal = phase(cosAngle);

                // Add the sun
                float focusedEyeCos = pow(saturate(cosAngle), params.x);
                float sun = saturate(hg(focusedEyeCos, .995)) * transmittance;

                return lerp(color * phaseVal, normalize(_LightColor0) * 4, sun);
            }

            fixed3 getCloudColor(float currentDepth, float lightEnergy)
            {
                // Get a fog ratio
                float dstFog = 1-exp(-currentDepth * 8*.0002);
                // Apply to both cloud colors
                fixed3 colAf = lerp(colA, colC, dstFog);
                fixed3 colBf = lerp(colB, colC, dstFog);
                // Chose which gradient to apply
                // TODO: use a better way to lerp through these
                if (lightEnergy < 0.5)
                    return lerp(colBf, colAf, lightEnergy * 2);
                else
                    return lerp(colAf, _LightColor0, (lightEnergy - 0.5) * 2);
            }

            float4 rayMarch(float3 rayPos, float3 rayDir, float depth, float2 uv);

            float4 frag (v2f i) : SV_Target
            {
                #if DEBUG_MODE == 1
                if (debugViewMode != 0) {
                    float width = _ScreenParams.x;
                    float height =_ScreenParams.y;
                    float minDim = min(width, height);
                    float x = i.uv.x * width;
                    float y = (1-i.uv.y) * height;

                    if (x < minDim*viewerSize && y < minDim*viewerSize) {
                        return debugDrawNoise(float2(x/(minDim*viewerSize)*debugTileAmount, y/(minDim*viewerSize)*debugTileAmount));
                    }
                }
                #endif

                // Create ray
                float3 rayPos = i.worldPos;
                float3 rayDir = i.viewVector;

                // Get the depth value
                float depth = getDepth(i.uv);

                // If using an ortho camera for shadow mode
                if (unity_OrthoParams.w)
                    return shadowMarch(rayPos, rayDir, depth);

                // if (i.uv.x <= 0.5 / _ScreenParams.x && i.uv.y <= 0.5 * _ScreenParams.y) {
                //     float playerDensity = sampleDensity(playerPosition, 0, 0);
                //     float playerLight = lightmarch(playerPosition);
                //     return float4(playerDensity, playerLight, 0, 0);
                // }
                if (i.uv.x <= 50 / _ScreenParams.x && i.uv.y <= 50 / _ScreenParams.y) {
                    float playerDensity = sampleDensity(playerPosition, 0, 0) * 10 + 0.5;
                    return float4(playerDensity, playerDensity, playerDensity, 0);
                }
                if (i.uv.x <= 50 / _ScreenParams.x && i.uv.y <= 100 / _ScreenParams.y) {
                    float playerLight = lightmarch(playerPosition);
                    return float4(playerLight, playerLight, playerLight, 0);
                }
                


                return rayMarch(rayPos, rayDir, depth, i.uv);
            }

            float4 rayMarch(float3 rayPos, float3 rayDir, float depth, float2 uv) {

                // Normalize ray because of perspective interpolation
                float distancePerspectiveModifier = length(rayDir);
                rayDir = rayDir / distancePerspectiveModifier;

                // If an object was drawn except the skybox
                // Usually the plane
                // TODO: add check that the object is inside the skybox ?
                bool hiddenByObject = false; abs(depth - _ProjectionParams.z) > 10;
                // Normalize depth the same way
                depth *= distancePerspectiveModifier;

                float2 rayToContainerInfo = rayBoxDst(boundsMin, boundsMax, rayPos, 1/rayDir);
                float dstToBox = rayToContainerInfo.x;
                float dstInsideBox = rayToContainerInfo.y;

                // point of intersection with the cloud container
                float3 entryPoint = rayPos + rayDir * dstToBox;

                // Adds an empty sphere around the camera
                // float dstTravelled = max(400 - dstToBox, 0);
                float dstTravelled = 0;
                float avgDstTravelled = 0;
                float dstLimit = min(depth-dstToBox, dstInsideBox);

                float stepSize = stepSizeRender;

                // March through volume:
                float transmittance = 1;
                float lightEnergy = 0;

                // max render distance = lodMinDistance + pow(lodLevelMagnitude, 5)
                // but container limits the raycasting anyway
                for (int i = 1; i < 6; i++)
                {
                    float lodMaxDistance = lodMinDistance + pow(lodLevelMagnitude, i);
                    float localMax = min(dstToBox + dstLimit, lodMaxDistance) - dstToBox;
                    while (dstTravelled < localMax) {

                        float loopRatioLinear = (dstTravelled + dstToBox) / (lodMaxDistance);
                        float loopRatio = loopRatioLinear * loopRatioLinear;
                        loopRatio *= loopRatio;

                        rayPos = entryPoint + rayDir * dstTravelled;
                        float density = sampleDensity(rayPos, i, loopRatio);
                        float real_stepSize = clamp(stepSize * 500, 0, 1) * max(abs(density), 0.05);

                        float real_density = max(density, godRaysIntensity);
                        float lightTransmittance = lightmarch(rayPos);

                        transmittance *= beer(real_density * stepSize * lightAbsorptionThroughCloud);
                        lightEnergy += real_density * stepSize * transmittance * lightTransmittance * 0.5;
                        transmittance /= sqrt(beer(real_density * stepSize * lightAbsorptionThroughCloud));

                        dstTravelled += stepSize * max(abs(density), 0.05);
                        avgDstTravelled += stepSize * max(abs(density), 0.05) * transmittance;
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
                transmittance = saturate((transmittance - minTransmittance) / (1 - minTransmittance));

                float currentDepth;
                if (dstInsideBox > 0)
                    currentDepth = dstToBox + dstTravelled;
                else
                    currentDepth = depth;

                // Skybox and plane
                fixed3 backgroundCol = tex2D(_MainTex, uv);

                // Add shading to non-cloud objects
                // Could be done better by decoding normals
                if (hiddenByObject)
                    backgroundCol *= lerp(lightmarch(rayPos + rayDir * currentDepth), 1, 0.5);

                // Increase light energy contrast
                // TODO: make power a parameter
                lightEnergy *= 0.5;

                // Add clouds
                // When absorption (1 - transmittance) is low, less light energy has accumulated
                // This value accounts for that
                float lowAbsorptionLightBalance = transmittance == 1 ? 1 : 1 / (1-transmittance);
                // Get the cloud color depending on distance, and adjusted light energy
                fixed3 col = getCloudColor(avgDstTravelled, lightEnergy * lowAbsorptionLightBalance);

                // Add background or plane/objects
                col = lerp(col, backgroundCol, transmittance);

                // Add sun and sun glow
                if (hiddenByObject == false)
                    col = getSunColor(col, rayDir, transmittance);

                return float4(col,0);
            }

            ENDCG
        }
    }
}
