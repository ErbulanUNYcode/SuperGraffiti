Shader "Unlit/ColorPicker_Outline"
{
	Properties
	{
		_MyCol ("My Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Overlay" }
		LOD 100

		Pass
		{
			ZTest Off
			Blend One Zero

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma multi_compile _ UNITY_SINGLE_PASS_STEREO

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float3 worldPos : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float4 vertex : SV_POSITION;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			float4 _MyCol;

			v2f vert(appdata v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.worldNormal = UnityObjectToWorldNormal(v.normal);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
				float fresnel = 1 - saturate(dot(normalize(i.worldNormal), viewDir));
				float3 col = fresnel < 0.4 
					? _MyCol.rgb*=_MyCol.rgb
					: (normalize(0.5 - _MyCol.rgb) + 0.5);

				return fixed4(col, 1);
			}
			ENDCG
		}
	}
}
