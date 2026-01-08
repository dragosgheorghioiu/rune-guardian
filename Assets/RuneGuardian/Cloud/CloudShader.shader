Shader "Custom/CloudShader"
{
    Properties
    {
        _MainTex ("Cloud Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _Alpha ("Transparency", Range(0,1)) = 0.5
        _ScrollSpeed ("Scroll Speed", Vector) = (0.1, 0.05, 0, 0)
        _NoiseScale ("Noise Scale", Range(0,1)) = 0.3
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float4 _Color;
            float _Alpha;
            float4 _ScrollSpeed;
            float _NoiseScale;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uvNoise : TEXCOORD1;
                float4 color : COLOR;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                
                // Animate UVs over time
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv += _ScrollSpeed.xy * _Time.y;
                
                o.uvNoise = TRANSFORM_TEX(v.uv, _NoiseTex);
                o.uvNoise += _ScrollSpeed.zw * _Time.y * 0.5;
                
                o.color = v.color;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Sample textures
                fixed4 cloudTex = tex2D(_MainTex, i.uv);
                fixed4 noiseTex = tex2D(_NoiseTex, i.uvNoise);
                
                // Mix cloud with noise for variation
                fixed cloudMask = cloudTex.r * (1.0 - _NoiseScale * (1.0 - noiseTex.r));
                
                // Apply color and alpha
                fixed4 col = _Color;
                col.a = cloudMask * _Alpha * i.color.a;
                
                return col;
            }
            ENDCG
        }
    }
}