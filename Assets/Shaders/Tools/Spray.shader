Shader "Custom/FakeMetal_Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorTex ("Color Mask", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0,1)) = 1
        _Roughness ("Roughness", Range(0,1)) = 0.2
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
            #pragma multi_compile_instancing
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                fixed4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _ColorTex;
            samplerCUBE _Cube;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Metallic;
            float _Roughness;

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float3 N = normalize(i.worldNormal);
                float3 V = normalize(UnityWorldSpaceViewDir(i.worldPos));

                float3 R = reflect(-V, N);

                float mip = _Roughness * 6.0;
                float3 env = texCUBElod(_Cube, float4(R, mip)).rgb;

                float3 albedo = tex2D(_MainTex, i.uv).rgb;
                float colMask = tex2D(_ColorTex, i.uv).r;

                albedo = lerp(albedo, _Color.rgb, colMask);

                if (i.color.r > 0.5) albedo = _Color.rgb;
                else if (i.color.g > 0.5) albedo = 0.3;

                float3 diffuse = albedo;
                float3 metallicPart = env * albedo;

                float3 result = lerp(diffuse, metallicPart, _Metallic);

                return float4(result, 1);
            }

            ENDHLSL
        }
    }
}