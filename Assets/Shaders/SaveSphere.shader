Shader "Custom/SaveSphere"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _IconTex ("Icon", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            ZWrite On
            ZTest LEqual
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _IconTex;

            struct appdata
            {
                float4 vertex : POSITION;
            };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 objClip : TEXCOORD0;
                nointerpolation float fov : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.objClip = UnityObjectToClipPos(float4(0,0,0,1));
                o.fov = 1/abs(UNITY_MATRIX_P._m11);
                return o;
            }

            fixed4 frag (v2f i, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                float2 screenUV = i.pos.xy / _ScreenParams.xy;
            #if (defined(SHADER_API_D3D11) || defined(SHADER_API_VULKAN)) && defined(UNITY_SINGLE_PASS_STEREO)
                screenUV.x= frac(screenUV.x);
            #endif

            #if UNITY_UV_STARTS_AT_TOP
                screenUV.y = screenUV.y;
            #endif

                if(isFrontFace)
                {
                    float2 centerUV = (i.objClip.xy / i.objClip.w) * 0.5 + 0.5;

                #if UNITY_UV_STARTS_AT_TOP
                    centerUV.y = 1.0 - centerUV.y;
                #endif

                    float2 uv = (screenUV - centerUV);

                    uv.x *= _ScreenParams.x / _ScreenParams.y;

                    uv *= i.objClip.w * 5.0*i.fov;

                    uv += 0.5;

                    return tex2D(_IconTex, uv);
                }

                return float4(tex2D(_MainTex, screenUV).rgb,1);
            }
            ENDCG
        }
    }
}