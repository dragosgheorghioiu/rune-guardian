Shader "Custom/ScrollingTexture"
{
    Properties
    {
        _MainTex ("Texture2D", 2D) = "white" {}
        _Speed ("Speed", Float) = 1.0
        _Direction ("Direction", Vector) = (1.0, 0.0, 0.0, 0.0)
        _UVOffset ("UVOffset", Float) = 0.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Speed;
            float2 _Direction;
            float _UVOffset;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate scrolling offset based on time, speed, and direction
                float2 scrollOffset = _Direction * _Speed * _Time.y;
                
                // Add the UV offset parameter
                scrollOffset += _Direction * _UVOffset;
                
                // Apply scroll to UV coordinates
                float2 scrolledUV = i.uv + scrollOffset;
                
                // Sample the texture
                fixed4 col = tex2D(_MainTex, scrolledUV);
                
                return col;
            }
            ENDCG
        }
    }
}
