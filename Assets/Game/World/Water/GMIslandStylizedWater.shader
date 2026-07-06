Shader "RateOfDecay/URP/GM Island Stylized Water"
{
    Properties
    {
        _ShallowColor ("Shallow Color", Color) = (0.12, 0.72, 0.88, 0.62)
        _DeepColor ("Deep Color", Color) = (0.02, 0.18, 0.42, 0.78)
        _FoamColor ("Foam Color", Color) = (0.82, 0.96, 1.0, 0.9)
        _Alpha ("Alpha", Range(0, 1)) = 0.72
        _FresnelPower ("Fresnel Power", Range(0.5, 8)) = 3.5
        _FresnelStrength ("Fresnel Strength", Range(0, 1)) = 0.55
        _WaveHeight ("Wave Height", Range(0, 1)) = 0.14
        _WaveScaleA ("Wave Scale A", Range(0.01, 2)) = 0.18
        _WaveScaleB ("Wave Scale B", Range(0.01, 2)) = 0.43
        _WaveSpeedA ("Wave Speed A", Range(0, 5)) = 0.85
        _WaveSpeedB ("Wave Speed B", Range(0, 5)) = 1.35
        _RippleScale ("Ripple Scale", Range(0.1, 25)) = 7.5
        _RippleSpeed ("Ripple Speed", Range(0, 10)) = 2.1
        _RippleStrength ("Ripple Strength", Range(0, 1)) = 0.18
        _FoamAmount ("Foam Amount", Range(0, 1)) = 0.32
        _SpecularStrength ("Specular Strength", Range(0, 2)) = 0.55
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor;
                half4 _DeepColor;
                half4 _FoamColor;
                half _Alpha;
                half _FresnelPower;
                half _FresnelStrength;
                half _WaveHeight;
                half _WaveScaleA;
                half _WaveScaleB;
                half _WaveSpeedA;
                half _WaveSpeedB;
                half _RippleScale;
                half _RippleSpeed;
                half _RippleStrength;
                half _FoamAmount;
                half _SpecularStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float waveMask : TEXCOORD2;
            };

            float WaveValue(float2 p, float time)
            {
                float waveA = sin((p.x + p.y * 0.42) * _WaveScaleA + time * _WaveSpeedA);
                float waveB = sin((p.x * -0.34 + p.y) * _WaveScaleB + time * _WaveSpeedB);
                float ripple = sin((p.x * 1.31 - p.y * 0.77) * _RippleScale + time * _RippleSpeed) * _RippleStrength;
                return waveA * 0.55 + waveB * 0.35 + ripple;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                float3 positionWS = posInputs.positionWS;
                float time = _Time.y;
                float wave = WaveValue(positionWS.xz, time);

                positionWS.y += wave * _WaveHeight;
                output.positionWS = positionWS;
                output.positionCS = TransformWorldToHClip(positionWS);

                float waveX = WaveValue(positionWS.xz + float2(0.18, 0), time);
                float waveZ = WaveValue(positionWS.xz + float2(0, 0.18), time);
                float3 tangentX = normalize(float3(0.18, (waveX - wave) * _WaveHeight, 0));
                float3 tangentZ = normalize(float3(0, (waveZ - wave) * _WaveHeight, 0.18));
                output.normalWS = normalize(cross(tangentZ, tangentX));
                output.waveMask = saturate(wave * 0.5 + 0.5);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);

                half fresnel = pow(1.0h - saturate(dot(normalWS, viewDirWS)), _FresnelPower);
                half3 baseColor = lerp(_DeepColor.rgb, _ShallowColor.rgb, input.waveMask);

                half foamPattern = sin((input.positionWS.x * 0.9 + input.positionWS.z * 1.25) + _Time.y * 1.8);
                foamPattern += sin((input.positionWS.x * -1.7 + input.positionWS.z * 0.55) + _Time.y * 2.4);
                foamPattern = saturate((foamPattern * 0.5 + 0.5) - (1.0h - _FoamAmount));

                half3 halfDir = normalize(lightDir + viewDirWS);
                half spec = pow(saturate(dot(normalWS, halfDir)), 96.0h) * _SpecularStrength;

                half3 color = baseColor;
                color = lerp(color, _FoamColor.rgb, foamPattern * 0.45h);
                color += fresnel * _FresnelStrength * _FoamColor.rgb;
                color += spec * mainLight.color;

                half alpha = saturate(lerp(_DeepColor.a, _ShallowColor.a, input.waveMask) * _Alpha + fresnel * 0.08h);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
