Shader "Custom/CloudShader" {
    Properties {
        _MainTex ("Cloud Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Alpha ("Transparency", Range(0,1)) = 0.5
        _ScrollSpeed ("Scroll Speed", Vector) = (0.1, 0.05, 0, 0)
        _NoiseScale ("Noise Scale", Range(0,1)) = 0.3
    }
    
    SubShader {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True" 
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float _Alpha;
            float4 _ScrollSpeed;
            float _NoiseScale;
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // Particle system provides unique color per particle
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uvNoise : TEXCOORD1;
                float4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            v2f vert(appdata v) {
                v2f o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.pos = UnityObjectToClipPos(v.vertex);
                
                // Animate UVs over time
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv += _ScrollSpeed.xy * _Time.y;
                
                o.uvNoise = TRANSFORM_TEX(v.uv, _NoiseTex);
                o.uvNoise += _ScrollSpeed.zw * _Time.y * 0.5;
                
                // Pass through vertex color from particle system
                o.color = v.color;
                
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                // Sample textures
                fixed4 cloudTex = tex2D(_MainTex, i.uv);
                fixed4 noiseTex = tex2D(_NoiseTex, i.uvNoise);
                
                // Mix cloud with noise for variation
                fixed cloudMask = cloudTex.r * (1.0 - _NoiseScale * (1.0 - noiseTex.r));
                
                // Use particle color directly for per-particle tinting
                fixed4 finalColor = i.color;
                
                // Apply alpha
                finalColor.a = cloudMask * _Alpha * i.color.a;
                
                return finalColor;
            }
            ENDCG
        }
    }
}