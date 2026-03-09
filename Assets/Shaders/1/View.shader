Shader "Unlit/View"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry" }
		LOD 100

		Pass
		{
			ZWrite On
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
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv = i.uv;
				fixed4 col = tex2D(_MainTex, uv);
				float2 ofs = float2(9,16)/100000;
				float2 op=0;
				uv.x+=ofs.x;
				op.x=tex2D(_MainTex, uv).a;
				uv.x-=ofs.x*2;
				op.x-=tex2D(_MainTex, uv).a;
				uv+=ofs;
				op.y=tex2D(_MainTex, uv).a;
				uv.y-=ofs.y*2;
				op.y-=tex2D(_MainTex, uv).a;
				float mg = op.x*op.x + op.y*op.y;
				if(mg>0)
				{
					float3 fx = 0.5+normalize(0.5-col.rgb);
					col.rgb*=(1-mg);
					col.rgb+=fx*mg;
				}

				col.a=0;

				return col;
			}
			ENDCG
		}
	}
}
