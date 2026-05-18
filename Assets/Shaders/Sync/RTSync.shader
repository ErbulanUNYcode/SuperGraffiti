Shader "CustomRenderTexture/RTSync"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Params ("xy=grid zw=index", Vector) = (4, 4, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Name "Update"
            HLSLPROGRAM

            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag

            #include "UnityCustomRenderTexture.cginc"

            sampler2D _MainTex;
            float4 _Params;

            float4 frag(v2f_customrendertexture i) : SV_Target
            {
                float2 grid = _Params.xy;
                float2 index = _Params.zw;

                float2 segSize = 1.0 / grid;

                float2 uv = index * segSize + i.localTexcoord * segSize;

                return float4(tex2D(_MainTex, uv).rgb, 1);
            }

            ENDHLSL
        }
    }
}