Shader "Unlit/ReRenderWithDraw"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MyPos ("My Position", Vector) = (0,0,0,0)
		_MyRot ("My Rotation", Vector) = (0,0,0,0)
		_MyCol ("My Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{
			"RenderType"="Opaque"
			"Queue"="Background"
		}
		LOD 100

		Pass
		{
			ZWrite Off
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
				linear float2 uv : TEXCOORD0;
				linear float3 worldPos : TEXCOORD1;
				linear float3 diffAll : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			float4 _MyPos;
			float4 _MyRot;
			float4 _MyCol;
			float4 _Rand;
			Texture2D _MainTex;


			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.diffAll = o.worldPos;
				o.diffAll -= _MyPos;

				float d = length(o.diffAll.xz);
				float a = atan2(o.diffAll.x, o.diffAll.z) - _MyRot.y;
				o.diffAll.xz = float2(sin(a), cos(a)) * d;

				d = length(o.diffAll.yz);
				a = atan2(o.diffAll.y, o.diffAll.z) + _MyRot.x;
				o.diffAll.yz = float2(sin(a), cos(a)) * d;
				o.diffAll.z -= 0.07;

				d = length(o.diffAll.xy);
				a = atan2(o.diffAll.x, o.diffAll.y) + _Rand.a;
				o.diffAll.xy = float2(sin(a), cos(a)) * d;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				uint width, height;
				_MainTex.GetDimensions(width, height);

				int2 pixelCoord = int2(i.uv.x * width, i.uv.y * height);
				float4 col = _MainTex.Load(int3(pixelCoord, 0));

				if(_MyPos.z > 0) return col;//спрей за стеной
				if(i.diffAll.z<=0) return col;//спрей смотрит на стену задом



				float3 diffAll;
				diffAll.xy= i.diffAll.xy-frac(i.diffAll.xy*32+_Rand.xy)/32;

				float rad = dot(diffAll.xy, diffAll.xy);

				diffAll.z = i.diffAll.z-frac(i.diffAll.z*8+_Rand.z + diffAll.x*101 + diffAll.y*211)/8;
				diffAll.z /= 4;
				float zOffset2 = dot(diffAll, diffAll) / 4;

				if(zOffset2 > 1) return col;//спрей слишком далеко от поверхности

				if(diffAll.z * diffAll.z < rad) return col;//поверхность далеко от центра спрея
				
				i.diffAll = (i.diffAll-diffAll)*float3(64,64,16)-1;

				float l = dot(i.diffAll, i.diffAll);
				//if(l > 1) return col;//поверхности далеко от центра капли спрея

				diffAll.xy /= diffAll.z;

				float glow = (1 - dot(diffAll.xy, diffAll.xy)) * (1 - zOffset2);
				
				glow *= glow*(1-l);
				_MyCol *= _MyCol;
				glow *= _MyCol.a/8;
				col.rgb *= 1 - glow;
				col.rgb += glow * _MyCol.rgb;

				return col;
			}
			ENDCG
		}
	}
}