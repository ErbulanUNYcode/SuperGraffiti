Shader "Unlit/ColorPickerView"
{
	Properties
	{
		_MyPos ("My Position", Vector) = (0,0,0,0)
		_MyScale ("My Scale", Float) = 0.25
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
				float3 diffAll : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			float4 _MyPos;
			float _MyScale;


			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				o.diffAll = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.diffAll -= _MyPos.xyz;

				float d = length(o.diffAll.xz);
				float a = atan2(o.diffAll.x, o.diffAll.z) - _MyPos.w;
				o.diffAll.xz = float2(sin(a), cos(a)) * d;
                o.diffAll /= _MyScale;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float s = 0.04;
				if(i.diffAll.x<-s || i.diffAll.x>1+s || i.diffAll.y<-s || i.diffAll.y>1+s || i.diffAll.z<-s || i.diffAll.z>1+s) clip(-1);
				return fixed4(i.diffAll*i.diffAll,1);
			}
			ENDCG
		}
	}
}