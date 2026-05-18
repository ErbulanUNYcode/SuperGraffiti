Shader "Unlit/ColorPickerNeon"
{
	Properties
	{
		_MyPos ("My Position", Vector) = (0,0,0,0)
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry" }

		Pass
		{
			ZWrite Off
			Blend One One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma multi_compile _ UNITY_SINGLE_PASS_STEREO

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
				float4 color : COLOR;
				float3 diffAll : TEXCOORD3;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			float4 _MyPos;

			v2f vert(appdata v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = v.color;

				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.worldNormal = UnityObjectToWorldNormal(v.normal);

				o.diffAll = o.worldPos - _MyPos.xyz;

				float d = length(o.diffAll.xz);
				float a = atan2(o.diffAll.x, o.diffAll.z) - _MyPos.w;
				o.diffAll.xz = float2(sin(a), cos(a)) * d;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				float3 c = i.color.rgb;
				float3 baseCol = i.diffAll * i.diffAll * 11.1;

				if (c.r > 0.99 && c.g < 0.01 && c.b < 0.01)
				{
					float v = i.uv.y;
					float t = saturate(abs(v - 0.5) * 2.0);

					float3 col = lerp(float3(1,1,1), baseCol, t);
					float alpha = 1.0 - t;

					return float4(col * alpha, 1);
				}

				float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
				float3 n = normalize(i.worldNormal);

				float facing = saturate(dot(n, viewDir));
				float t2 = 1.0 - facing * facing;

				float3 col = lerp(float3(1,1,1), baseCol, t2);
				float alpha2 = 1.0 - t2;

				return float4(col * alpha2, 1);
			}

			ENDCG
		}
	}
}