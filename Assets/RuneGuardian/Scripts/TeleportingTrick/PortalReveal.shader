Shader "URP/PortalReveal"
{
 Properties
    {
        _BaseMap ("Albedo", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1,1,1,1)
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Range(0,2)) = 1
        _Metallic ("Metallic", Range(0,1)) = 0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _PortalPlanePos ("Portal Plane Pos", Vector) = (0,0,0,0)
        _PortalPlaneNormal ("Portal Plane Normal", Vector) = (0,1,0,0)
        _PortalFadeWidth ("Portal Fade Width", Float) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 300
        Cull Back
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _BumpScale;
                float _Metallic;
                float _Smoothness;
                float3 _PortalPlanePos;
                float3 _PortalPlaneNormal;
                float _PortalFadeWidth;
            CBUFFER_END

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float d = dot(IN.worldPos - _PortalPlanePos, normalize(_PortalPlaneNormal));
                if (d < -_PortalFadeWidth) discard;
                float a = saturate((d + _PortalFadeWidth) / max(_PortalFadeWidth, 1e-5));
                half4 c = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                if (a < 0.01) discard;
                c.a *= a;
                return c;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Forward"
}
