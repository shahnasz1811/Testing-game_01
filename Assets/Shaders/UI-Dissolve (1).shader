// Hand-written UI/Canvas equivalent of Dissolve.shadergraph.
//
// Shader Graph's "Sprite Unlit" target can't be assigned to a uGUI Image, so this
// reproduces the exact same node graph in plain HLSL instead, property-for-property:
//
//   _MainTex, _DissolveAmount, _DissolveScale, _OutlineThickness, _OutlineColor
//
// Graph logic (traced from the .shadergraph's node/edge data):
//   noise          = SimpleNoise(uv, _DissolveScale)                 [Unity's built-in Simple Noise node]
//   innerEdge      = 1 - _DissolveAmount
//   outerEdge      = innerEdge + _OutlineThickness
//   cutoutMask     = step(noise, innerEdge)     // 1 where fully "solid"
//   outerMask      = step(noise, outerEdge)     // 1 where solid OR in the burn ring
//   edgeBand       = outerMask - cutoutMask     // 1 only in the burn ring
//   finalColor.rgb = baseColor.rgb - edgeBand + edgeBand * _OutlineColor.rgb
//   finalColor.a   = baseColor.a * outerMask
//
// Base UI plumbing (stencil/clip-rect/instancing) copied from Unity's built-in
// UI/Default shader so this still works correctly inside Masks/RectMask2D.
Shader "UI/Dissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Dissolve)]
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0
        _DissolveScale ("Dissolve Scale", Range(0,500)) = 30
        _OutlineThickness ("Outline Thickness", Range(0,1)) = 0.1
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)

        [Header(UI Plumbing Do Not Edit)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float4 _ClipRect;

            float _DissolveAmount;
            float _DissolveScale;
            float _OutlineThickness;
            fixed4 _OutlineColor;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            // ---- Simple Noise, matching Unity Shader Graph's "Simple Noise" node ----
            float SimpleNoise_RandomValue(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            float SimpleNoise_Interpolate(float a, float b, float t)
            {
                return (1.0 - t) * a + (t * b);
            }

            float SimpleNoise_ValueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);

                float r0 = SimpleNoise_RandomValue(i + float2(0.0, 0.0));
                float r1 = SimpleNoise_RandomValue(i + float2(1.0, 0.0));
                float r2 = SimpleNoise_RandomValue(i + float2(0.0, 1.0));
                float r3 = SimpleNoise_RandomValue(i + float2(1.0, 1.0));

                float bottom = SimpleNoise_Interpolate(r0, r1, f.x);
                float top    = SimpleNoise_Interpolate(r2, r3, f.x);
                return SimpleNoise_Interpolate(bottom, top, f.y);
            }

            // 3-octave sum, same freq/amp progression as the built-in node
            // (freq = 1,2,4 while amp = 0.125,0.25,0.5 - amplitude grows as
            // frequency grows, which is what gives this node its coarse,
            // blobby look rather than a fine, sandy one).
            float SimpleNoise(float2 uv, float scale)
            {
                float t = 0.0;

                float freq = pow(2.0, 0.0);
                float amp  = pow(0.5, 3.0 - 0.0);
                t += SimpleNoise_ValueNoise(float2(uv.x * scale / freq, uv.y * scale / freq)) * amp;

                freq = pow(2.0, 1.0);
                amp  = pow(0.5, 3.0 - 1.0);
                t += SimpleNoise_ValueNoise(float2(uv.x * scale / freq, uv.y * scale / freq)) * amp;

                freq = pow(2.0, 2.0);
                amp  = pow(0.5, 3.0 - 2.0);
                t += SimpleNoise_ValueNoise(float2(uv.x * scale / freq, uv.y * scale / freq)) * amp;

                return t;
            }
            // ---- end Simple Noise ----

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 baseTex = tex2D(_MainTex, IN.texcoord) * IN.color;

                float noise = SimpleNoise(IN.texcoord, _DissolveScale);

                float innerEdge = 1.0 - _DissolveAmount;
                float outerEdge = innerEdge + _OutlineThickness;

                float cutoutMask = step(noise, innerEdge);
                float outerMask  = step(noise, outerEdge);
                float edgeBand   = outerMask - cutoutMask;

                fixed3 finalRGB = baseTex.rgb - edgeBand + edgeBand * _OutlineColor.rgb;
                fixed  finalA   = baseTex.a * outerMask;

                fixed4 color = fixed4(finalRGB, finalA);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
