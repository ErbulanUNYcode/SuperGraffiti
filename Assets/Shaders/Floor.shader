Shader "Unlit/Floor"
{
    Properties
    {
        _GridSize ("Grid Size", Float) = 0.3
        _LineWidth ("Line Width", Range(0.001, 0.2)) = 0.02
        _FadeDistance ("Fade Distance", Float) = 3.0
        _Color ("Grid Color", Color) = (1,1,1,0.5)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float _GridSize;
            float _LineWidth;
            float _FadeDistance;
            fixed4 _Color;

            v2f vert(appdata v)
            {
                v2f o;

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

                o.worldPos = worldPos.xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 gridUV = i.worldPos.xz / _GridSize;

                float2 f = frac(gridUV);

                float lineX =
                    step(f.x, _LineWidth) +
                    step(1.0 - f.x, _LineWidth);

                float lineZ =
                    step(f.y, _LineWidth) +
                    step(1.0 - f.y, _LineWidth);

                float grid = saturate(lineX + lineZ);

                if (grid <= 0.0)
                    discard;

                float dist = distance(_WorldSpaceCameraPos, i.worldPos);

                float fade = saturate(1.0 - dist / _FadeDistance);

                fixed4 col = _Color;
                col.a *= fade;

                return col;
            }
            ENDCG
        }
    }
}