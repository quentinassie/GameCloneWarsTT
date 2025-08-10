Shader "Custom/HoloRippleSafe"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _WaveSpeed ("Wave Speed", Float) = 0.5
        _WaveFrequency ("Wave Frequency", Float) = 15.0
        _WaveAmplitude ("Wave Amplitude", Float) = 0.01
        _PulseSpeed ("Pulse Speed", Float) = 1.0
        _PulseStrength ("Pulse Strength", Range(0,1)) = 0.2
        _ColorTint ("Color Tint", Color) = (0.3, 0.6, 1.0, 1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _WaveSpeed;
            float _WaveFrequency;
            float _WaveAmplitude;
            float _PulseSpeed;
            float _PulseStrength;
            float4 _ColorTint;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 center = float2(0.5, 0.5);

                float baseAlpha = tex2D(_MainTex, uv).a;
                if (baseAlpha < 0.01)
                    discard;

                float dist = distance(uv, center);
                float ripple = sin(dist * _WaveFrequency - _Time.y * _WaveSpeed) * _WaveAmplitude - 0.01;
                float2 offset = normalize(uv - center) * ripple;
                float2 distortedUV = uv + offset;

                fixed4 col = tex2D(_MainTex, distortedUV) * _ColorTint;

                float pulse = 1.0 - (_PulseStrength * abs(sin(_Time.y * _PulseSpeed)));

                // Appliquer alpha final = alpha texture * alpha couleur * pulsation
                col.a = baseAlpha * _ColorTint.a * pulse;

                return col;
            }
            ENDCG
        }
    }
}
