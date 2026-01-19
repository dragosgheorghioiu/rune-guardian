Shader "Custom/PortalReveal"
{
 Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)

        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Range(0,2)) = 1

        _Metallic ("Metallic", Range(0,1)) = 0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5

        _DepthCutoff ("Depth Write Cutoff (0..1)", Range(0,1)) = 0.2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 300
        Cull Back

        Pass
        {
            Tags { "LightMode"="Always" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float3 _PortalPlanePos;
            float3 _PortalPlaneNormal;
            float _PortalFadeWidth;
            float _DepthCutoff;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos  : SV_POSITION;
                float3 wpos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos  = UnityObjectToClipPos(v.vertex);
                o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float d = dot(i.wpos - _PortalPlanePos, normalize(_PortalPlaneNormal));

                if (d < -_PortalFadeWidth) discard;

                float a = saturate((d + _PortalFadeWidth) / max(_PortalFadeWidth, 1e-5));

                if (a < _DepthCutoff) discard;

                return 0;
            }
            ENDCG
        }


        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual

        CGPROGRAM
        #pragma target 3.0
        #pragma surface surf Standard fullforwardshadows alpha:fade
        #include "UnityStandardUtils.cginc"

        sampler2D _MainTex;
        fixed4 _Color;

        sampler2D _BumpMap;
        half _BumpScale;

        half _Metallic;
        half _Smoothness;

        float3 _PortalPlanePos;
        float3 _PortalPlaneNormal;
        float _PortalFadeWidth;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 n = normalize(_PortalPlaneNormal);
            float d = dot(IN.worldPos - _PortalPlanePos, n);

            if (d < -_PortalFadeWidth)
                clip(-1);

            float a = saturate((d + _PortalFadeWidth) / max(_PortalFadeWidth, 1e-5));

            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_BumpMap), _BumpScale);
            o.Alpha = c.a * a;
        }
        ENDCG
    }

    FallBack "Standard"
}
