Shader "Custom/TerrainURP_4Textures_Fixed"
{
    Properties
    {
        _SandTex("Sand", 2D) = "white" {}
        _GrassTex("Grass", 2D) = "white" {}
        _RockTex("Rock", 2D) = "white" {}
        _SnowTex("Snow", 2D) = "white" {}
        _Tiling("Tiling", Float) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float4 color : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            sampler2D _SandTex;
            sampler2D _GrassTex;
            sampler2D _RockTex;
            sampler2D _SnowTex;

            float _Tiling;

            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.vertex.xz * _Tiling;
                o.color = v.color;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 weights = saturate(i.color);

                // 🔥 zabezpieczenie
                half sum = weights.r + weights.g + weights.b + weights.a + 0.0001;
                weights /= sum;

                half4 sand = tex2D(_SandTex, i.uv);
                half4 grass = tex2D(_GrassTex, i.uv);
                half4 rock = tex2D(_RockTex, i.uv);
                half4 snow = tex2D(_SnowTex, i.uv);

                half4 finalColor =
                    sand * weights.r +
                    grass * weights.g +
                    rock * weights.b +
                    snow * weights.a;

                return half4(finalColor.rgb, 1.0);
            }
            ENDHLSL
        }
    }
}