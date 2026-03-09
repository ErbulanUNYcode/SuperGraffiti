Shader "Unlit/ReRenderWithDraw"
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

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				linear float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			Texture2D _MainTex;


			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				uint width, height;
				_MainTex.GetDimensions(width, height);

				int2 pixelCoord = int2(i.uv.x * width, i.uv.y * height);
				float4 col = _MainTex.Load(int3(pixelCoord, 0));

				if(col.a == 0) clip(-1);
				col.a=0;
				return col;
			}
			ENDCG
		}
	}
}