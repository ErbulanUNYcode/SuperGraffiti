Shader "Unlit/RedChannelCull"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle] _Reverse ("Reverse", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest"
        }

        Pass
        {
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            float _Reverse;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, IN.uv);

                float center = tex.r;

                if (_Reverse > 0.5)
                    center = 1 - center;

                int a = 0;

                for (int j = 0; j < 16; j++)
                {
                    float angle = (float(j) / 16.0) * 6.2831853;

                    float s, c;
                    sincos(angle, s, c);

                    float2 offset = float2(c*0.002, s*0.008);

                    float r = tex2D(_MainTex, IN.uv + offset).r;

                    if (_Reverse > 0.5)
                        r = 1 - r;

                    a += r > 0.5;
                }

                fixed4 col = fixed4(0.15,0.1, 0.05, 1);

                if (a != 16)
                    col = fixed4(0.15,0.1, 1, 1);

                clip(center - 0.5);

                return col;
            }
            ENDCG
        }
    }
}