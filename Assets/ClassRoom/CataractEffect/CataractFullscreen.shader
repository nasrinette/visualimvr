// Cataract simulation fullscreen shader for URP + VR stereo instancing
// Effects: cloudy blur, light scatter, contrast loss, yellow tinting, veiling glare
Shader "Hidden/CataractFullscreen"
{
    Properties
    {
        _BlurRadius("Blur Radius", Float) = 0.003
        _ScatterStrength("Scatter Strength", Float) = 0.0
        _ContrastLoss("Contrast Loss", Float) = 0.15
        _YellowTint("Yellow Tint", Float) = 0.15
        _VeilingGlare("Veiling Glare", Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Overlay" }

        Pass
        {
            Name "CataractEffect"
            ZTest Always ZWrite Off Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile_instancing
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _BlurRadius;
            float _ScatterStrength;
            float _ContrastLoss;
            float _YellowTint;
            float _VeilingGlare;

            struct CataractAttributes
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct CataractVaryings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CataractVaryings Vert(CataractAttributes IN)
            {
                CataractVaryings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = GetFullScreenTriangleVertexPosition(IN.vertexID);
                OUT.uv = GetFullScreenTriangleTexCoord(IN.vertexID);
                return OUT;
            }

            // 8-sample Poisson disc for the cloudy lens blur
            static const float2 poissonDisc[8] =
            {
                float2(-0.326, -0.406),
                float2(-0.840, -0.074),
                float2(-0.696,  0.457),
                float2( 0.962, -0.195),
                float2( 0.473, -0.480),
                float2( 0.519,  0.767),
                float2( 0.185, -0.893),
                float2( 0.896,  0.412)
            };

            half4 Frag(CataractVaryings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 uv = i.uv;
                half4 original = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                // --- 1. Cloudy blur (Poisson disc) ---
                // Simulates the opaque, clouded lens scattering light irregularly
                half3 blurred = half3(0, 0, 0);
                for (int s = 0; s < 8; s++)
                {
                    float2 offset = poissonDisc[s] * _BlurRadius;
                    blurred += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + offset).rgb;
                }
                blurred /= 8.0;

                // Blend: when _BlurRadius is very small, stay close to original
                float blurBlend = saturate(_BlurRadius * 300.0);
                half3 col = lerp(original.rgb, blurred, blurBlend);

                // --- 2. Light scatter / glare ---
                // Bright surrounding pixels bleed into the current pixel,
                // simulating light diffracting through the clouded lens
                if (_ScatterStrength > 0.001)
                {
                    half3 scatter = half3(0, 0, 0);
                    float scatterRad = 0.03;
                    for (int d = 0; d < 6; d++)
                    {
                        float angle = d * 1.047198; // 2*PI / 6
                        float2 dir = float2(cos(angle), sin(angle));
                        half3 s1 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + dir * scatterRad).rgb;
                        // Weight by brightness: only bright areas scatter noticeably
                        float sLum = dot(s1, float3(0.299, 0.587, 0.114));
                        scatter += s1 * sLum;
                    }
                    scatter /= 6.0;
                    col += scatter * _ScatterStrength;
                }

                // --- 3. Contrast reduction ---
                // The clouded lens flattens the contrast range
                float lum = dot(col, float3(0.299, 0.587, 0.114));
                col = lerp(col, half3(0.5, 0.5, 0.5), _ContrastLoss);

                // --- 4. Yellow tinting + partial desaturation ---
                // Cataracts cause the lens to yellow/brown over time
                half3 gray = half3(lum, lum, lum);
                col = lerp(col, gray, _YellowTint * 0.5);
                col *= lerp(half3(1, 1, 1), half3(1.05, 0.98, 0.80), _YellowTint);

                // --- 5. Veiling glare ---
                // Overall milky haze proportional to how much light is in the scene
                float veilLum = dot(col, float3(0.299, 0.587, 0.114));
                half3 veil = half3(1.0, 0.98, 0.92) * veilLum;
                col = lerp(col, veil, _VeilingGlare);

                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
