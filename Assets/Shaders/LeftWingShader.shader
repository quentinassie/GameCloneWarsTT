Shader "Canvas/WindWingDeform"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _DeformMaskTex("Deform Mask", 2D) = "white" {}
        _Speed("Speed", Float) = 1.5
        _Frequency("Frequency", Float) = 3.0
        _Amplitude("Amplitude", Float) = 0.1
        _PulseStrength("Pulse Strength", Float) = 1.0
        _TintColor("Tint Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uvOrig : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _DeformMaskTex;
            float4 _MainTex_ST;
            float4 _DeformMaskTex_ST;
            float _Speed;
            float _Frequency;
            float _Amplitude;
            float _PulseStrength;
            float4 _TintColor;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                float2 uvOrig = TRANSFORM_TEX(IN.uv, _MainTex);
                float2 maskUV = TRANSFORM_TEX(IN.uv, _DeformMaskTex);
                float deformStrength = tex2Dlod(_DeformMaskTex, float4(maskUV, 0, 0)).r;

                float pulse = exp(-pow(frac(_Time.y * _Speed) * 4.0 - 2.0, 2.0));
                float wave = sin(uvOrig.y * _Frequency);
                float decay = pow(1.0 - uvOrig.y, 2.0);

                float2 offset = float2(
                    wave * _Amplitude * pulse * deformStrength * decay * _PulseStrength,
                    wave * _Amplitude * 0.3 * pulse * deformStrength * decay * _PulseStrength
                );

                float4 worldPos = IN.vertex;
                worldPos.xy += offset;

                OUT.vertex = TransformObjectToHClip(worldPos);
                OUT.uv = uvOrig;
                OUT.uvOrig = uvOrig;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 color = tex2D(_MainTex, IN.uv);
                clip(color.a - 0.01);
                return color * _TintColor;
            }
            ENDHLSL
        }
    }
}
