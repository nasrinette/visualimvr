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

        // edge glow around (explanation)
        // _EdgeGlow ("Edge Glow", Range(0,1)) = 0
        // _GlowWidth ("Glow Width", Range(0,0.2)) = 0.02
        
        // arrows (explanation)
        _ShowArrows ("Show Arrows", Range(0,1)) = 0 // either yes or no
        _ArrowStrength ("Arrow Strength", Range(0,1)) = 0.8
        _ArrowAngleWidth ("Arrow Angle Width (rad)", Range(0.01,1.0)) = 0.25
        _ArrowLength ("Arrow Length", Range(0.01,0.3)) = 0.08
        _ArrowPulseAmp ("Arrow Pulse Amp", Range(0,0.05)) = 0.01
        _ArrowPulseSpeed ("Arrow Pulse Speed", Range(0,10)) = 3
        // _EdgeGlowColor ("Edge Glow Color", Color) = (1,0.8,0.2,1)
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

            // float _EdgeGlow, _GlowWidth;
            float _ShowArrows, _ArrowStrength, _ArrowAngleWidth, _ArrowLength;
            float _ArrowPulseAmp, _ArrowPulseSpeed;
            // float4 _EdgeGlowColor;

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
            
            #define PI 3.14159265
            float WrapAngle(float a)
            {
                // Wrap to [-PI, PI]
                // I found this function online
                a = fmod(a + PI, 2.0 * PI);
                if (a < 0) a += 2.0 * PI;
                return a - PI;
            }

            float AngleDist(float a, float b)
            {
                return abs(WrapAngle(a - b));
            }

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
                
                // FOR OUTLINE 
              

                // edgeBand = 1 near dist==r, 0 away
                // float edgeBand = 1.0 - smoothstep(0.0, _GlowWidth, abs(d - _Radius));
                // col.rgb += edgeBand * _EdgeGlow * _EdgeGlowColor.rgb;



                // FOR ARROWS
                float2 p = uv - _CenterUV.xy;
                float ang  = atan2(p.y, p.x); // [-PI, PI]
                
                float pulse = sin(_Time.y * _ArrowPulseSpeed) * _ArrowPulseAmp; // ocillate between -1 and 1

                // how far outward from the tunnel edge 
                // 0 at edge, 1 at arrow tip (outward)
                float u = (d - (_Radius + pulse)) / _ArrowLength; 

               
                // angular distance from arrow centers
                float angDistRight = AngleDist(ang, 0.0);
                float angDistLeft  = AngleDist(ang, PI);

                // turn angular distance into [0..1] local coordinate
                float vRight = angDistRight / _ArrowAngleWidth;
                float vLeft  = angDistLeft  / _ArrowAngleWidth;

                // step(a, b): returns 0 if b < a; returns 1 if b >= a
                float inRadial = step(0.0, u) * step(u, 1.0); // only 1 when 0 <= u <= 1


                // Triangle profile: at u=0 (base), allow v up to around 1; at u=1 (tip), v must be 0
                // So "inside triangle" if v <= (1-u)
                float triangleRight = step(vRight, 1.0 - u) * inRadial;
                float triangleLeft  = step(vLeft,  1.0 - u) * inRadial;

                float arrows = (triangleRight + triangleLeft) ;
                
                col.rgb += (arrows * _ShowArrows) * _ArrowStrength;

                return col;
            }
            ENDHLSL
        }
    }
}