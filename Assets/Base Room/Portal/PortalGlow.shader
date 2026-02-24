// Glowy portal surface shader for URP + VR stereo instancing
// Effects: animated swirl, fresnel edge glow, pulsing emission, transparency
Shader "Custom/PortalGlow"
{
    Properties
    {
        [HDR] _CoreColor("Core Color", Color) = (0.2, 0.5, 1.0, 0.8)
        [HDR] _EdgeColor("Edge Color", Color) = (0.5, 0.1, 1.0, 1.0)
        [HDR] _GlowColor("Glow Color", Color) = (0.3, 0.6, 1.5, 1.0)
        _SwirlSpeed("Swirl Speed", Range(0.1, 5.0)) = 1.0
        _SwirlDensity("Swirl Density", Range(1.0, 10.0)) = 4.0
        _DistortStrength("Distortion Strength", Range(0.0, 0.5)) = 0.15
        _FresnelPower("Fresnel Power", Range(0.5, 8.0)) = 2.5
        _PulseSpeed("Pulse Speed", Range(0.1, 4.0)) = 1.2
        _PulseAmount("Pulse Amount", Range(0.0, 1.0)) = 0.3
        _NoiseScale("Noise Scale", Range(1.0, 20.0)) = 6.0
        _CoreBrightness("Core Brightness", Range(0.5, 5.0)) = 2.0
        _Alpha("Overall Alpha", Range(0.0, 1.0)) = 0.9
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "PortalGlow"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile_instancing
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _CoreColor;
                half4 _EdgeColor;
                half4 _GlowColor;
                float _SwirlSpeed;
                float _SwirlDensity;
                float _DistortStrength;
                float _FresnelPower;
                float _PulseSpeed;
                float _PulseAmount;
                float _NoiseScale;
                float _CoreBrightness;
                float _Alpha;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Simple hash-based noise functions (no texture dependency)
            float2 hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            float hash21(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            // Value noise
            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f); // smoothstep

                float a = hash21(i + float2(0, 0));
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            // Fractal Brownian Motion (layered noise)
            float fbm(float2 p, int octaves)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                for (int i = 0; i < octaves; i++)
                {
                    value += amplitude * valueNoise(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }
                return value;
            }

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetWorldSpaceNormalizeViewDir(posInputs.positionWS);
                OUT.uv = IN.uv;

                return OUT;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 uv = i.uv;
                float time = _Time.y;

                // Center UVs for radial effects
                float2 centeredUV = uv * 2.0 - 1.0;
                float dist = length(centeredUV);
                float angle = atan2(centeredUV.y, centeredUV.x);

                // --- Swirl distortion ---
                float swirlAngle = angle + dist * _SwirlDensity - time * _SwirlSpeed;
                float2 swirlUV = float2(cos(swirlAngle), sin(swirlAngle)) * dist;
                swirlUV = swirlUV * 0.5 + 0.5;

                // --- Noise layers ---
                float noise1 = fbm(swirlUV * _NoiseScale + time * 0.3, 4);
                float noise2 = fbm(swirlUV * _NoiseScale * 1.5 - time * 0.2, 3);
                float combinedNoise = noise1 * 0.6 + noise2 * 0.4;

                // Distort the radial distance with noise
                float distortedDist = dist + (combinedNoise - 0.5) * _DistortStrength;

                // --- Core-to-edge gradient ---
                float coreMask = 1.0 - saturate(distortedDist * 1.2);
                coreMask = pow(coreMask, 1.5);

                float edgeMask = saturate(1.0 - abs(distortedDist - 0.7) * 4.0);

                // --- Fresnel (view-dependent edge glow) ---
                float3 normalWS = normalize(i.normalWS);
                float3 viewDirWS = normalize(i.viewDirWS);
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);

                // --- Pulse ---
                float pulse = 1.0 + sin(time * _PulseSpeed * 6.2832) * _PulseAmount;

                // --- Color composition ---
                half3 coreCol = _CoreColor.rgb * coreMask * _CoreBrightness;
                half3 edgeCol = _EdgeColor.rgb * edgeMask * 1.5;
                half3 glowCol = _GlowColor.rgb * fresnel * 1.2;
                half3 noiseCol = lerp(_CoreColor.rgb, _EdgeColor.rgb, combinedNoise) * combinedNoise * 0.5;

                half3 finalColor = (coreCol + edgeCol + glowCol + noiseCol) * pulse;

                // --- Alpha ---
                float coreAlpha = coreMask * 0.9;
                float edgeAlpha = edgeMask * 0.7;
                float fresnelAlpha = fresnel * 0.6;
                float noiseAlpha = combinedNoise * 0.3;
                float finalAlpha = saturate(coreAlpha + edgeAlpha + fresnelAlpha + noiseAlpha) * _Alpha;

                // Fade out at the very edge of the UV bounds
                float edgeFade = saturate((1.0 - dist) * 3.0);
                finalAlpha *= edgeFade;

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
