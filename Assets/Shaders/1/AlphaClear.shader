Shader "Grtaffiti/AlphaClear"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			ZTest Off
			Blend One Zero

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			Texture2D _MainTex;
			float4 _Pos[8];
			float4 _Rot[8];
			float _Dist[8];

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o =(v2f)0;

				//current spray data
				int id = round(v.color.r*255);
				float3 pos = _Pos[id].xyz;
				float2 rot = _Rot[id].xy;
				float dist = _Dist[id];
				
				float3 ver = v.vertex.xyz/2.08*(dist+0.05);	

				//exceptions
				if(pos.z>0 || pos.z<-length(ver))
				{
					v.vertex.xyz = 0;
					o.pos = UnityObjectToClipPos(v.vertex);
					return o;
				}

				//rotation x
				float a=atan2(ver.y,ver.z) - rot.x;
				float l=length(ver.yz);
				ver.yz = float2(sin(a),cos(a))*l;

				//rotation y
				a=atan2(ver.x,ver.z) + rot.y;
				l=length(ver.xz);
				ver.xz = float2(sin(a),cos(a))*l;

				//position
				ver+=pos.xyz;
				
				//vertex is out of canvas
				bool b=ver.z>0;

				if(b)
				{
					ver = pos.xyz + (ver-pos.xyz)/(ver.z-pos.z)*(-pos.z);
				}
				else
				{
					float3 offset = ver-pos.xyz;
					l=length(offset);
					offset.z = cos(asin(-pos.z/l))*l;
					offset.xy = offset.xy/length(offset.xy)*offset.z;
					ver = pos.xyz + offset;
				}
				ver.z=0;
				v.vertex.xyz = ver;

				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				uint width, height;
				_MainTex.GetDimensions(width, height);
				float2 uv = i.pos.xy / _ScreenParams.xy;
				int2 pixelCoord = int2(uv.x * width, uv.y * height);
				float4 col = _MainTex.Load(int3(pixelCoord, 0));
				if(col.a == 0) clip(-1);
				col.a=0;
				return col;
			}
			ENDCG
		}
	}
}