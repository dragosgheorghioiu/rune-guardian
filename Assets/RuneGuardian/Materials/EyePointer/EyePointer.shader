Shader "Custom/EyePointer"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _Radius("Radius", Range(0, 0.25)) = 0.1
        _Angle("Angle", Range(0, 360)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        ZTest Always
     	ZWrite Off
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _Color;
            float _Radius;
            float _Angle;

            v2f vert(appdata_t v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            half4 frag(v2f i) : SV_Target
            {
                // Center the UV coordinates (0.5, 0.5 is the center of the quad)
                float2 centeredUV = i.uv - float2(0.5, 0.5);

                // Calculate the distance from the center
                float dist = length(centeredUV);
                float alpha = 1.0;

                if (dist > _Radius && dist < 0.5) {
                    // if (_Angle == 0) {
                    //     alpha = 0.0;
                    // }

                    // if (_Angle > 0) {
                        const float Deg2Rad = (UNITY_PI * 2.0) / 360.0;
                        float radians = (_Angle - 180) * Deg2Rad;

                        float angleRad = atan2(centeredUV.x, centeredUV.y);

                        if (angleRad > radians) {
                            alpha = 0.0;

                            if (dist < _Radius + 0.1) {
                                alpha = 1 - smoothstep(_Radius, _Radius + 0.1, dist);
                            }
                        }

                        if (angleRad <= radians) {
                            if (dist > 0.4) {
                                alpha = 1 - smoothstep(0.4, 0.5, dist);
                            }
                        }
                    // }
                }

                // Smoothly transition to transparency outside the circle
                if (dist > 0.5) {
                    alpha = 0.0;
                }

                // Apply the color with transparency
                return float4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
