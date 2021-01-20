// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color

Shader "KT/SphereShader" {
    Properties {
        _Color("Tint", Color) = (1,1,1,1)
    }
    SubShader {
        Tags { "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 150
 
        CGPROGRAM
        #pragma surface surf Lambert

        fixed4 _Color;

        struct Input {
            float3 worldPos;
        };

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.worldPos = v.vertex.xyz;
        }
 
        void surf(Input IN, inout SurfaceOutput o) {
            float3 c = mul(unity_ObjectToWorld, float4(IN.worldPos, 1)).rgb;
            float r = _SinTime / 4;
            float g = (clamp(c.g - 0.125, 0, 0.125) * 8);
            float b = (clamp(c.r - 0.125, 0, 0.125) * 8);
            o.Albedo = float3(r, g, b) * _Color;
            o.Alpha = 0;
        }
        ENDCG
    }
}