
Shader "Clouds"
{

    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        // material properties used exclusively to check if material cloning works
        ShapeTex ("shape", 3D) = "white" {}
        NoiseTex ("noise", 2D) = "white" {}
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
            Texture3D<float> ShapeTex;
            // 1D texture to give the 'thunderhead''vibe
            Texture2D<float> AltitudeAtlas;
            // 2D texture to fix banding
            Texture2D<float> NoiseTex;
            // 2D texture containing the heights of 4 absorption levels
            Texture2D<float4> ShadowMap;
            float4 ShadowMap_TexelSize;

            static const float4 shadowMapAbsorptionLevels = float4(0.6, 0.4, 0.2, 0.01);
            float shadowMapSize;

            SamplerState samplerShapeTex;
            SamplerState samplerNoiseTex;
            SamplerState samplerAltitudeAtlas;
            SamplerState samplerShadowMapPointRepeat;

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            float4 testParams;

            // Shape settings
            // TODO: reorganize parameters (eg. phase into light setting)
            int3 mapSize;
            // Pretty self-explanatory noise combination parameters
            float minTransmittance;

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
            // LOD Settings
            float lodLevelMagnitude;
            float lodMinDistance;
            // Private parameters to recreate altitudeAtlas's range
            float altitudeOffset;
            float altitudeMultiplier;
            float altitudeValueOffset;
            float altitudeValueMultiplier;

            // March settings
            int numStepsLight;
            float stepSizeRender;
            float firstStepNoiseMultiplier;
            // Two opposite corners of the cloud container
            float3 boundsMin;
            float3 boundsMax;

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

            // Player settings
            float3 playerPosition;

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
                return exp(-d);
            }

            float reverse_beer(float d) {
                return -log(d);
            }

            float remap01(float v, float low, float high) {
                return (v-low)/(high-low);
            }

            float altitudeDensity(float height)
            {
                float texturePos = (height + altitudeOffset) * altitudeMultiplier;
                float textureValue = AltitudeAtlas.SampleLevel(samplerAltitudeAtlas, texturePos, 0);
                return textureValue * altitudeValueMultiplier + altitudeValueOffset;
            }

            float sampleDensity(float3 rayPos, uniform int optimisation, float optiInterpolation) {
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

                float density = altitudeDensity(rayPos.y);

                if (optimisation > 5)
                    return density;

                float3 wind = windDirection * (_Time.x * timeScale);

                float prevDensity;

                {
                    float3 samplePos = uvw.zxy / scale3 + wind * speed3;
                    float value = ShapeTex.SampleLevel(samplerShapeTex, samplePos, 0);
                    prevDensity = density;
                    density += (value - 0.5) * scale3 * weight3;
                }

                if (optimisation > 4)
                    return lerp(density, prevDensity, optiInterpolation);

                {
                    float3 samplePos = uvw / scale2 + wind * speed2;
                    float value = ShapeTex.SampleLevel(samplerShapeTex, samplePos, 0);
                    prevDensity = density;
                    density += (value - 0.5) * scale2 * weight2;
                }

                if (optimisation > 3)
                    return lerp(density, prevDensity, optiInterpolation);

                {
                    float3 samplePos = uvw / scale1 + wind * speed1;
                    float value = ShapeTex.SampleLevel(samplerShapeTex, samplePos, 0);

                    prevDensity = density;
                    density += (value - 0.5) * scale1 * weight1;
                }

                if (optimisation > 2)
                    return lerp(density, prevDensity, optiInterpolation);

                {
                    float3 samplePos = uvw / scale0 + time * wind * speed0;
                    float value = ShapeTex.SampleLevel(samplerShapeTex, samplePos, 0);

                    prevDensity = density;
                    density -= (value - 0.5) * scale0 * weight0;
                }

                return lerp(density, prevDensity, optiInterpolation);
            }

            float sampleLightmap(float2 uv, float height)
            {
                return 0.5;
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
                // TODO lookup SampleCMP

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
                // TODO: use more recent tools:
                // https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@11.0/manual/writing-shaders-urp-reconstruct-world-position.html
                // https://alexanderameye.github.io/notes/realtime-caustics/
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
                float focusedEyeCos = pow(saturate(cosAngle), 0.9);
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
                // Create ray
                float3 rayPos = i.worldPos;
                float3 rayDir = i.viewVector;

                // Get the depth value
                float depth = getDepth(i.uv);

                // If using an ortho camera for shadow mode
                if (unity_OrthoParams.w)
                    return shadowMarch(rayPos, rayDir, depth);

                // If one of four corner pixels
                float2 pixelSize = 1 / _ScreenParams.xy;
                if ((i.uv.x <= pixelSize.x || i.uv.x >= 1 - pixelSize.x) &&
                    (i.uv.y <= pixelSize.y || i.uv.y >= 1 - pixelSize.y))
                {
                    // Store data for gameplay
                    float playerDensity = sampleDensity(playerPosition, 0, 0);
                    float playerLight = lightmarch(playerPosition);
                    return float4(playerDensity, playerLight, 0.5, 0.5);
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
                bool hiddenByObject = abs(depth - _ProjectionParams.z) > 100;
                // Normalize depth the same way
                depth *= distancePerspectiveModifier;

                // Random offset to the start of raymarching
                // Avoids some banding
                // TODO: map texture 1:1 to actual pixels
                float startNoiseOffset = (NoiseTex.SampleLevel(samplerNoiseTex, uv * 10 + _Time.xy * 97 , 0) - 0.5)
                    * firstStepNoiseMultiplier * 3;

                float2 rayToContainerInfo = rayBoxDst(boundsMin, boundsMax, rayPos, 1/rayDir);
                float dstToBox = rayToContainerInfo.x;
                float dstInsideBox = rayToContainerInfo.y;

                // point of intersection with the cloud container
                float3 entryPoint = rayPos + rayDir * (dstToBox + startNoiseOffset);

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
                for (int i = 1; i < 7; i++)
                {
                    float lodMaxDistance = lodMinDistance + pow(lodLevelMagnitude, i);
                    float localMax = min(dstToBox + dstLimit, lodMaxDistance) - dstToBox;
                    float start = dstTravelled;
                    while (dstTravelled < localMax) {

                        float loopRatioLinear = (dstTravelled - start) / (localMax - start);
                        float loopRatio = loopRatioLinear;// * loopRatioLinear;
                        // loopRatio *= loopRatio;

                        rayPos = entryPoint + rayDir * dstTravelled;
                        float density = sampleDensity(rayPos, i, loopRatio);

                        // TODO: fix banding when setting godrays
                        float real_density = max(density, godRaysIntensity);
                        float lightTransmittance = lightmarch(rayPos);

                        // some day I'll figure out WTH I meant with this
                        // clearly I'm just multiplying the transmitance by sqrt of beer
                        // buut doing that gives a completely different result
                        float beerRes = beer(real_density * stepSize * lightAbsorptionThroughCloud);
                        transmittance *= beerRes;
                        lightEnergy += real_density * stepSize * transmittance * lightTransmittance * 0.5;
                        transmittance /= sqrt(beerRes);

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
                if (depth > dstTravelled)
                    hiddenByObject = false;

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
                // if (hiddenByObject)
                //     backgroundCol *= lerp(lightmarch(rayPos + rayDir * currentDepth), 1, 0.9);
                // Code above doesn't work for obscure reasons

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
                // TODO: fix failure when outside of bounding box
                // ie. sun appears in front of plane/whales
                if (hiddenByObject == false)
                    col = getSunColor(col, rayDir, transmittance);

                return float4(col,0);
            }

            ENDCG
        }
    }
}
