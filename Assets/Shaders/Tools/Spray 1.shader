Shader "Custom/FakeMetal_Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 1
        _Cube ("Reflection Cubemap", CUBE) = "" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            sampler2D _MainTex;
            samplerCUBE _Cube;
            float4 _MainTex_ST;
            float _Metallic;

            v2f vert (appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 N = normalize(i.worldNormal);
                float3 V = normalize(UnityWorldSpaceViewDir(i.worldPos));
                float3 R = reflect(-V, N);

                float3 albedo = tex2D(_MainTex, i.uv).rgb;
                float3 env = texCUBElod(_Cube, float4(R, 0)).rgb;

                float3 result = lerp(min(albedo,1), env, (_Metallic*2+albedo)/3);

                return float4(result, 1);
            }

            ENDHLSL
        }
    }
}