Shader "Unlit/ColorPickerView"
{
	Properties
	{
		_MyPos ("My Position", Vector) = (0,0,0,0)
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry" }
		LOD 100

		Pass
		{
			ZTest LEqual
			Blend One Zero

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 diffAll : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			float4 _MyPos;


			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				o.diffAll = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.diffAll -= _MyPos.xyz;

				float d = length(o.diffAll.xz);
				float a = atan2(o.diffAll.x, o.diffAll.z) - _MyPos.w;
				o.diffAll.xz = float2(sin(a), cos(a)) * d;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				if(i.diffAll.x<-0.01 || i.diffAll.x>0.26 || i.diffAll.y<-0.01 || i.diffAll.y>0.26 || i.diffAll.z<-0.01 || i.diffAll.z>0.26) clip(-1);
				return fixed4(i.diffAll*i.diffAll*16,1);
			}
			ENDCG
		}
	}
}