
Shader "Hidden/Clouds"
{

    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        // material property used exclusively to check if material cloning works
        NoiseTex ("noise", 3d) = "white" {}
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

            #include "UnityCG.cginc"
            #include "Assets/Scripts/Clouds/Shaders/CloudDebug.cginc"

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
                // Render settings
                float near = _ProjectionParams.y;
                float far = _ProjectionParams.z;
                float2 orthoSize = unity_OrthoParams.xy;

                v2f o;
                float3 pos = UnityObjectToClipPos(v.vertex);
                // TODO: cheaper way to calculate clip pos, but sometimes breaks
                // float3(v.uv, 0) * float3(2,-2,0) - float3(1,-1,0); 
                o.pos = float4(pos, 1);
                o.uv = v.uv;

                if (unity_OrthoParams.w)
                {
                    float3 viewVector = float4(0,0,1,0);
                    o.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));

                    float4 worldPos = float4(float2(pos.x, -pos.y) * orthoSize, near, 1);
                    o.worldPos = mul(unity_CameraToWorld, float4(worldPos));
                }
                else
                {
                    float3 viewVector = mul(unity_CameraInvProjection, float4(float2(pos.x, -pos.y), 1, -1));
                    o.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));

                    float3 worldPos = mul(unity_CameraInvProjection, float4(float2(pos.x, -pos.y), near, -1));
                    o.worldPos = mul(unity_CameraToWorld, float4(worldPos,1));
                }

                return o;
            }

            // Textures
            // The main cloud texture
            Texture3D<float4> NoiseTex;
            // Whisps and detailing
            Texture3D<float4> DetailNoiseTex;
            // Larger scale 2D texture, currently unused
            Texture2D<float4> WeatherMap;
            // 1D texture to give the 'thunderhead''vibe
            Texture2D<float> AltitudeMap;

            SamplerState samplerNoiseTex;
            SamplerState samplerDetailNoiseTex;
            SamplerState samplerWeatherMap;
            SamplerState samplerAltitudeMap;

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            // Shape settings
            // TODO: reorganize parameters (eg. phase into light setting)
            float4 params;
            int3 mapSize;
            // Pretty self-explanatory noise combination parameters
            float densityMultiplier;
            float densityOffset;
            float scale;
            float detailNoiseScale;
            float detailNoiseWeight;
            // Weights to balance the 4 channels of the detail shader
            float3 detailWeights;
            float4 shapeNoiseWeights;
            // Used for silverlining while looking at the sun, currently broken
            float4 phaseParams;
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
            int debugViewMode; // 0 = off; 1 = shape tex; 2 = detail tex; 3 = weathermap, 4 = altitudemap
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

            float sampleDensity(float3 rayPos, bool cheap) {
                // Constants:
                const int mipLevel = 2;
                const float baseScale = 1/1000.0;

                // Calculate texture sample positions
                float time = _Time.x * timeScale;
                float3 size = boundsMax - boundsMin;
                float3 boundsCentre = (boundsMin+boundsMax) * .5;
                float3 uvw = (size * .5 + rayPos) * baseScale * scale;
                float3 shapeSamplePos = uvw + float3(time,time*0.1,time*0.2) * baseSpeed;

                // Calculate height gradient from weather map
                // Currently fully disabled
                //float2 weatherUV = (size.xz * .5 + (rayPos.xz-boundsCentre.xz)) / max(size.x,size.z);
                //float weatherMap = WeatherMap.SampleLevel(samplerWeatherMap, weatherUV, 1).x * 100;

                // Sets a gradient tapering off at the top and bottom, avoiding ugly flat spots (which tend to look buggy)
                float gMin = 0.1;//remap(weatherMap.x,0,1,0.1,0.5);
                float gMax = 0.9;//remap(weatherMap.x,0,1,gMin,0.9);
                float heightPercent = (rayPos.y - boundsMin.y) / size.y;
                float heightGradient = pow(saturate(remap(heightPercent, 0.0, gMin, 0, 1)) * saturate(remap(heightPercent, 1, gMax, 0, 1)), 0.17);


                // Calculate meta shape density
                // Duplicated code to create a meta layer of clouds
                // TODO: Fully seperate from normal noise settings
                //float3 shapeSamplePosMeta = shapeSamplePos / 10;
                //shapeSamplePosMeta.y /= 1.5;
                float3 shapeSamplePosMeta = uvw;
                shapeSamplePosMeta /= 10;
                shapeSamplePosMeta.y /= 1.5;


                float4 shapeNoiseMeta = NoiseTex.SampleLevel(samplerNoiseTex, shapeSamplePosMeta , mipLevel);
                float4 normalizedShapeWeightsMeta = shapeNoiseWeights / dot(shapeNoiseWeights, 1);
                float shapeFBMMeta = dot(shapeNoiseMeta, normalizedShapeWeightsMeta) * heightGradient;
                float baseShapeDensityMeta = (shapeFBMMeta + densityOffset * .1 - 0.1) * 15;

                // Add altitude density
                // TODO: standardize height gradient inside density
                baseShapeDensityMeta += altitudeDensity(heightPercent) * heightGradient;

                // NOTE: performance factor
                //if (cheap)
                //    return baseShapeDensityMeta * heightGradient;

                // Attempt at writing a shockwave around the plane
                // float dist = length(rayPos - playerPosition);
                // baseShapeDensityMeta -= sin(dist / 1000) * 1000 / pow(dist / 10, 3);

                // Try early returning, might be ignored by compiler since forking is hard on GPU
                if (baseShapeDensityMeta < -1)
                    return baseShapeDensityMeta * heightGradient / 2;

                // Calculate base shape density
                float4 shapeNoise = NoiseTex.SampleLevel(samplerNoiseTex, shapeSamplePos, mipLevel);
                float4 normalizedShapeWeights = shapeNoiseWeights / dot(shapeNoiseWeights, 1);
                float shapeFBM = dot(shapeNoise, normalizedShapeWeights) * heightGradient;
                float baseShapeDensity = baseShapeDensityMeta * heightGradient + shapeFBM + densityOffset * .1;

                // Save sampling from detail tex if shape density <= 0
                if (baseShapeDensity > 0 && !cheap) {
                    // Sample detail noise
                    float3 detailSamplePos = uvw*detailNoiseScale + float3(time*.4,-time,time*0.1)*detailSpeed;
                    float4 detailNoise = DetailNoiseTex.SampleLevel(samplerDetailNoiseTex, detailSamplePos, mipLevel);
                    float3 normalizedDetailWeights = detailWeights / dot(detailWeights, 1);
                    float detailFBM = dot(detailNoise, normalizedDetailWeights);

                    // Subtract detail noise from base shape (weighted by inverse density so that edges get eroded more than centre)
                    float oneMinusShape = 1 - shapeFBM;
                    float detailErodeWeight = oneMinusShape * oneMinusShape * oneMinusShape;
                    float cloudDensity = baseShapeDensity - (1-detailFBM) * detailErodeWeight * detailNoiseWeight;

                    return cloudDensity * densityMultiplier * 0.1;
                } 
                return baseShapeDensity * heightGradient / 2;
            }

            // Calculate proportion of light that reaches the given point from the lightsource
            float lightmarch(float3 position) {
                float3 dirToLight = _WorldSpaceLightPos0.xyz;
                float dstInsideBox = rayBoxDst(boundsMin, boundsMax, position, 1/dirToLight).y;

                float stepSize = dstInsideBox/numStepsLight;
                position += dirToLight * stepSize * .5;
                float totalDensity = 0;
                float dstTravelled = 0;

                // TODO: check if branching is worth it
                for (int step = 0; step < numStepsLight; step ++) {
                    float density = sampleDensity(position, true);
                    totalDensity += max(0, density * stepSize);

                    //Variable stepping, using the noise like a signed-distance-function
                    //Works well if hard contrast is avoided
                    float dst = stepSize * max(abs(density), 1);
                    position += dirToLight * dst;
                    dstTravelled += dst;
                    //Try early returning if less than 0.01 is passing through
                    if (totalDensity > -log(0.2) * lightAbsorptionTowardSun)
                        break;
                    //Due to variable stepping, skip if end is reached
                    if (dstTravelled >= dstInsideBox)
                        break;
                }
                float transmittance = beer(totalDensity * lightAbsorptionTowardSun);

                return lerp(darknessThreshold, 1, transmittance);
            }

            // Attempts at compressing ordered float4
            fixed4 compressLightData(float4 data)
            {
                //data.gba = lerp(data.rgb, float3(1,1,1), data.gba);
                //data.gba = lerp(data.rgb, data.gba, data.gba)
                data.a = (data.a - data.b) / (1-data.b);
                data.b = (data.b - data.g) / (1-data.g);
                data.g = (data.g - data.r) / (1-data.r);
                return data;
            }
            float4 decompressLightData(fixed4 data)
            {
                float4 result;

                result.r = data.r;
                result.g = data.r + data.g * (1 - data.r);
                result.b = data.g + data.b * (1 - data.g);
                result.a = data.b + data.a * (1 - data.b);
                return result;
            }

            fixed4 shadowMarch(float3 position, float3 direction, float depth) {

                // The absorption levels for which a sun distance is stored
                float4 targets = float4(0.99, 0.7, 0.1, 0.01);
                // The associated total densities needed to reach tem
                float4 targetDensity = float4(
                    reverse_beer(targets.x),
                    reverse_beer(targets.y),
                    reverse_beer(targets.z),
                    reverse_beer(targets.w));
                targetDensity *= lightAbsorptionTowardSun;
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
                depth -= distanceToBox;
                if (depth < 0)
                    return fixed4(0,0,0,0);

                // Numbers of measurements of the cloud density
                float stepSize = distanceToTravelWithDepth / numStepsLight;

                // Adjust the position inside the container, and add half a step
                position += direction * (stepSize * .5 + distanceToBox);

                float totalDensity = 0;
                float dstTravelled = 0;

                // TODO: consider adding step-back for more precision
                while (dstTravelled < distanceToTravelWithDepth) {

                    // Sample current density
                    float density = sampleDensity(position, false);
                    // Modify the stepsize for variable stepping (less artifacts)
                    float dst = stepSize * max(abs(density), 0.2);

                    // Increment density counter
                    totalDensity += max(0, density * dst);

                    // If density reached current threshold
                    if (totalDensity >= targetDensity.x)
                    {
                        // Calculate the current ratio of the distance to cross:
                        float res = dstTravelled / distanceToTravel;

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
                for (;targetIt < 4; targetIt++)
                    result.xyzw = (result * float4(0,1,1,1) + float4(dstTravelled / distanceToTravel, 0,0,0)).yzwx;
                return saturate(result);
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
                else if (debugViewMode == 3) {
                    channels = WeatherMap.SampleLevel(samplerWeatherMap, samplePos.xy, 0);
                } else if (debugViewMode == 4) {
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
                if (unity_OrthoParams.w && UNITY_REVERSED_Z)
                    return lerp(_ProjectionParams.z, _ProjectionParams.y, nonlin_depth) - _ProjectionParams.y;
                else if (unity_OrthoParams.w)
                    return lerp(_ProjectionParams.y, _ProjectionParams.z, nonlin_depth) - _ProjectionParams.y;
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

            fixed4 frag (v2f i) : SV_Target
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
                float distancePerspectiveModifier = length(i.viewVector);
                float3 rayPos = i.worldPos;
                float3 rayDir = i.viewVector / distancePerspectiveModifier;

                // Depth and cloud container intersection info:
                float depth = getDepth(i.uv) * distancePerspectiveModifier;

                float2 rayToContainerInfo = rayBoxDst(boundsMin, boundsMax, rayPos, 1/rayDir);
                float dstToBox = rayToContainerInfo.x;
                float dstInsideBox = rayToContainerInfo.y;

                // point of intersection with the cloud container
                float3 entryPoint = rayPos + rayDir * dstToBox;

                // random starting offset (makes low-res results noisy rather than jagged/glitchy, which is nicer)
                // removed, as signed distance function approach works better
                //float randomOffset = BlueNoise.SampleLevel(samplerBlueNoise, squareUV(i.uv*3), 0);
                //randomOffset *= rayOffsetStrength;

                float dstTravelled = 0;//randomOffset;
                float dstLimit = min(depth-dstToBox, dstInsideBox);

                float stepSize = stepSizeRender * distancePerspectiveModifier;

                // March through volume:
                float transmittance = 1;
                float lightEnergy = 0;

                while (dstTravelled < dstLimit) {
                    rayPos = entryPoint + rayDir * dstTravelled;
                    float density = sampleDensity(rayPos, false);

                     if (density > 0) {
                        float lightTransmittance = lightmarch(rayPos);
                        transmittance *= exp(-density * stepSize * lightAbsorptionThroughCloud);
                        lightEnergy += density * stepSize * transmittance * lightTransmittance;

                        // Exit early if T is close to zero as further samples won't affect the result much
                        if (transmittance < 0.01) {
                            transmittance -= 0.01;
                            break;
                        }
                    }
                    dstTravelled += stepSize * max(abs(density), 0.05) * 2;
                }

                float currentDepth;
                if (dstInsideBox > 0)
                    currentDepth = dstToBox + dstTravelled;
                else
                    currentDepth = depth;

                // Skybox and plane
                fixed3 backgroundCol = tex2D(_MainTex,i.uv);

                // Add shading to non-cloud objects
                // Could be done better by decoding normals
                // if (hiddenByObject)
                //     backgroundCol *= lerp(lightmarch(rayPos + rayDir * currentDepth), 1, 0.5);

                // Increase light energy contrast
                // TODO: make power a parameter
                lightEnergy *= 0.5;
                lightEnergy = cinematicGradient(lightEnergy, 2);
                transmittance = sqrt(saturate(transmittance));
                bool hiddenByObject = abs(dstLimit - dstInsideBox) > 1;

                // Add clouds
                fixed3 col = getCloudColor(currentDepth, lightEnergy);

                // Add background or plane/objects
                col = lerp(col, backgroundCol, transmittance);

                // Add sun and sun glow
                if (hiddenByObject == false)
                    col = getSunColor(col, rayDir, transmittance);

                return fixed4(col,0);
            }

            ENDCG
        }
    }
}