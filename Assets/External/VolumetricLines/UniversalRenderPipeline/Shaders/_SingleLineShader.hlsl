#ifndef VOL_LINE_SINGLE_LINE_SHADER_URP_INC
#define VOL_LINE_SINGLE_LINE_SHADER_URP_INC
	
	// Property-variables declarations
	float _LineRadius;
	float4 _Color;

	// Vertex shader input attributes
	struct a2v
	{
		float4 vertex : POSITION;
		float3 otherPos : NORMAL; // object-space position of the other end
		half2 texcoord : TEXCOORD0;
		float2 offset : TEXCOORD1;
		// float4 color : COLOR;

		UNITY_VERTEX_INPUT_INSTANCE_ID
	};
	
	// Vertex out/fragment in data:
	struct v2f
	{
		float4 pos : SV_POSITION;
		half2 uv : TEXCOORD0;
		// float4 color : COLOR;

		UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
	};
	
	// Vertex shader
	v2f vert (a2v v)
	{
		v2f o;

		// Setup
		UNITY_SETUP_INSTANCE_ID(v);
		ZERO_INITIALIZE(v2f, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		// Pass on texture coordinates to fragment shader as they are:
		o.uv = v.texcoord;
		// o.color = v.color;

		// Extract average scale from matrix
		float lineScale = dot(float3(
			length(unity_ObjectToWorld._m00_m10_m20),
			length(unity_ObjectToWorld._m01_m11_m21),
			length(unity_ObjectToWorld._m02_m12_m22)
		), float3(1.0/3, 1.0/3, 1.0/3));

		// Scale to properly match Unity's world space units:
		// The `projScale` factor also handles different field of view values, which 
		// used to be handled via FOV_SCALING_OFF in previous versions of this asset.
		// Furthermore, `projScale` handles orthographic projection matrices gracefully.
		float projScale = unity_CameraProjection._m11;
		float scaledLineRadius = _LineRadius * lineScale * projScale;

		// Get aspect ratio
		float2 aspectRatio = float2(
			unity_CameraProjection._m11 / unity_CameraProjection._m00,
			1);

		// Transform current end of line		to homogeneous clip space:
		float4 csPos = TransformObjectToHClip(v.vertex.xyz);

		// Offset for our current vertex:
		float2 offset;

		// If transforming vertex of the middle part
		if (v.offset.x == 0) {

			// Transform opposite end of line to homogeneous clip space:
			float4 csPos_other = TransformObjectToHClip(v.otherPos);
			
			// The positions of the circles in clip space, corrected for aspect ratio
			float2 circleCenter1 = csPos.xy * aspectRatio / csPos.w;
			float2 circleCenter2 = csPos_other.xy * aspectRatio / csPos_other.w;

			// The projection from one end to another
			float2 lineDirProj = normalize(
				circleCenter1 - // screen-space pos of current end
				circleCenter2 // screen-space position of the other end
			) * scaledLineRadius; // account for scale

			// Some tangent math
			// see https://www.math-only-math.com/important-properties-of-direct-common-tangents.html

			// Radiuses, not accounting for line width
			float circleRadius1 = 1/csPos.w;
			float circleRadius2 = 1/csPos_other.w;
			// Difference between radiuses
			float radiusDiff = (circleRadius1-circleRadius2) * scaledLineRadius;

			// Distance between points
			float circleDist = length(circleCenter1-circleCenter2);

			// Angle between the centerline and the perpendicular to the tangent
			// float angleOfTangent = acos(radiusDiff / circleDist);
			// float xOffset = -cos(angleOfTangent)
			// float yOffset = sin(angleOfTangent)
			
			// Optimized version
			float xOffset = -radiusDiff / circleDist;
			float yOffset = v.offset.y * sqrt(1 - xOffset * xOffset);

			// Project the offset to face the camera
			offset = xOffset * lineDirProj +
				yOffset * float2(lineDirProj.y, -lineDirProj.x);
		} else {
			offset = v.offset * scaledLineRadius;
		}
		
		// Apply (aspect-ratio corrected) offset
		csPos.xy += offset / aspectRatio;
		o.pos = csPos;

		return o;
	}	
	
	// Fragment shader
	float4 frag(v2f i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		float2 uvCentered = i.uv - 0.5;
		float uvSquareLength = dot(uvCentered, uvCentered);
		float alpha = uvSquareLength <= 0.25;

		return float4(1, 1, 1, alpha) * _Color;
	}
	
#endif
