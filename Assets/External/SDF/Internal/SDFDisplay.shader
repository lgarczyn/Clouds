Shader "Sprites/SDFDisplay" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}

        [HDR]_FaceColor("Face Color", Color) = (1,1,1,1)
        _FaceDilate("Face Dilate", Range(0,1)) = 0.5

        [Header(Outline)]
        [Toggle(OUTLINE_ON)] _EnableOutline("Enable Outline", Float) = 0
        [HDR]_OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth("Outline Thickness", Range(0,1)) = 0
        [Toggle(DYNAMIC_OUTLINE)] _DynamicOutline("Dynamic Outline", Float) = 0

        [Header(Underlay)]
        [Toggle(UNDERLAY_ON)]  _EnableUnderlay("Enable Underlay", Float) = 0
        [HDR]_UnderlayColor("Border Color", Color) = (0,0,0,.5)
        _UnderlayOffsetX("Border OffsetX", Range(-1,1)) = 0
        _UnderlayOffsetY("Border OffsetY", Range(-1,1)) = 0
        _UnderlayDilate("Border Dilate", Range(-1,1)) = 0
        _UnderlaySoftness("Border Softness", Range(0,1)) = 0

        [Header(Other)]
        _GradientScale("Gradient Scale", float) = 20
        [Toggle(TEXCOLOR_ON)] _UseTextureColor("Use Texture Color", Float) = 0
    }
    SubShader {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Pass {

            Cull Off
            Lighting Off
            ZWrite Off
            Blend One OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #pragma shader_feature __ OUTLINE_ON
            #pragma shader_feature __ DYNAMIC_OUTLINE
            #pragma shader_feature __ TEXCOLOR_ON
            #pragma shader_feature __ UNDERLAY_ON

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            half4 _FaceColor;
            fixed _FaceDilate;
            half _GradientScale;
#if defined(OUTLINE_ON)
            fixed _OutlineSoftness;
            fixed _OutlineWidth;
            half4 _OutlineColor;
#endif
#if defined(UNDERLAY_ON)
            fixed _UnderlayOffsetX;
            fixed _UnderlayOffsetY;
            fixed _UnderlayDilate;
            fixed _UnderlaySoftness;
            half4 _UnderlayColor;
#endif

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target{
                half bias = _FaceDilate;

                // Compute density value and color, using texture if needed
#ifdef TEXCOLOR_ON
                fixed4 smp = tex2D(_MainTex, i.uv);
                float d = smp.a;
                fixed4 color = _FaceColor * half4(smp.rgb, 1);
#else
                fixed4 d = tex2D(_MainTex, i.uv).a;
                fixed4 color = _FaceColor;
#endif
                // Compute result color
                half4 output;
                

                // Calculate color with outline
#ifdef OUTLINE_ON
                {
                    half outlineFade = fwidth(i.uv * _GradientScale);
                    half realWidth = _OutlineWidth;
    #ifdef DYNAMIC_OUTLINE
                    realWidth *= outlineFade * 10;
    #endif
                    half ol_from = min(1, bias + (realWidth + outlineFade) / 2);
                    half ol_to = max(0, bias - (realWidth + outlineFade) / 2);
                    output = lerp(color, _OutlineColor, saturate((ol_from - d) / outlineFade));
                    output *= saturate((d - ol_to) / outlineFade);
                }
#else
                // calculate normal color
                {
                    half scale = 1.0 / (_GradientScale * fwidth(i.uv));
                    output = color * saturate((d - bias) * scale + 0.5);
                }
#endif

                // Append underlay (drop shadow)
#if defined(UNDERLAY_ON)
                {
                    half ul_from = max(0, bias - _UnderlayDilate - _UnderlaySoftness / 2);
                    half ul_to = min(1, bias - _UnderlayDilate + _UnderlaySoftness / 2);
                    float2 underlayUV = i.uv - float2(_UnderlayOffsetX, _UnderlayOffsetY);
                    d = tex2D(_MainTex, underlayUV).a;
                    output += float4(_UnderlayColor.rgb, 1) * (_UnderlayColor.a * (1 - output.a)) *
                        saturate((d - ul_from) / (ul_to - ul_from));
                }
#endif

                return output * i.color;
            }
            ENDCG
        }
    }
}

