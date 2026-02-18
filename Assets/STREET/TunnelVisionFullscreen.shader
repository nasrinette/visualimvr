// i used https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@8.2/manual/writing-shaders-urp-basic-unlit-structure.html
// and https://docs.unity3d.com/2022.1/Documentation/Manual/SinglePassInstancing.html for VR part
Shader "Hidden/TunnelVisionFullscreen"
{
    Properties
    {
        _Radius("Radius", Float) = 0.18
        _Feather("Feather", Float) = 0.06
        _Darkness("Darkness", Float) = 0.75
        _CenterUV("CenterUV", Vector) = (0.5,0.5,0,0)

        _WarpStrength("WarpStrength", Float) = 0.02
        _EdgeWidth("EdgeWidth", Float) = 0.08
        _Strain("Strain", Float) = 0
        _Snap("Snap", Float) = 0

    }
    // contains shader code
    SubShader
    {
        // defines when and under which conditions a subshader block or pass is executed
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Overlay" }

        Pass
        {
            Name "TunnelVision"
            ZTest Always ZWrite Off Cull Off

            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            // This line defines the name of the vertex shader. 
            #pragma vertex Vert
            // This line defines the name of the fragment shader. 
            #pragma fragment Frag

            // todo idk
            #pragma multi_compile_instancing
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // values coming fro c#
            float _Radius, _Feather, _Darkness;
            float4 _CenterUV;
            float _WarpStrength, _EdgeWidth, _Strain, _Snap, _BlurStrength;
            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.

            struct TunnelVisionAttributes
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID // for vr
            };

            // from vertex shader to frag shader
            struct TunnelVisionVaryings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO // for vr
            };

            TunnelVisionVaryings Vert (TunnelVisionAttributes IN)
            {
                TunnelVisionVaryings OUT;

                UNITY_SETUP_INSTANCE_ID(IN); // init this vertex for stereo rendering
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT); // pass stereo info to frag shader

                OUT.positionHCS = GetFullScreenTriangleVertexPosition(IN.vertexID);
                OUT.uv          = GetFullScreenTriangleTexCoord(IN.vertexID);
                return OUT;
            }

            half4 Frag (TunnelVisionVaryings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); // use correct eye texture for this pixel

                // TODO
                float2 uv = i.uv; // where on the scree is the pixel
                
                float d = distance(uv, _CenterUV.xy); // dist of pixel from tunnel center

                float mask = smoothstep(_Radius - _Feather, _Radius, d);
                // if d < _Radius - _Feather, returns 0; if d > _Radius returns 1; 
                // or smooth interp between 0 and 1 if d in between

                // edge ring: 1 near boundary, 0 away
                float edge = 1.0 - saturate(abs(d - _Radius) / _EdgeWidth);
                edge = smoothstep(0.0, 1.0, edge);

                float2 dirToCenter = normalize(_CenterUV.xy - uv);

                // warp spikes when snapping (snap inward)
                float warpAmt = _WarpStrength * edge * (_Strain + 1.2 * _Snap);

                float2 warpedUV = uv + dirToCenter * warpAmt;

                // half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, warpedUV); // get what the world looks like at this pixel
                float blurAmt = _BlurStrength * edge * (_Strain + _Snap);

                float2 px = blurAmt * 0.02; // todo: tune this
                half4 c0 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, warpedUV);
                half4 c1 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, warpedUV + float2(px.x, 0));
                half4 c2 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, warpedUV - float2(px.x, 0));
                half4 c3 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, warpedUV + float2(0, px.y));
                half4 c4 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, warpedUV - float2(0, px.y));

                half4 col = (c0 + c1 + c2 + c3 + c4) / 5.0;

                //  apply darkness outside (keep center clear)
                // lerp does 1 * (1-mask) + (1-darkness) * mask
                // if the mask is 0, it means we are at the center, then lerp is 1, so we don't change the color of that pixel
                // if mask = 1, then we are outside, so pixel becomes very dark (darkness=1)
                col.rgb *= lerp(1.0, 1.0 - _Darkness, mask);

                return col;
            }
            ENDHLSL
        }
    }
}