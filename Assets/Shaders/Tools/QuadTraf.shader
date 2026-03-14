Shader "Graffiti/QuadTraf"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			Blend One Zero
			Cull Off
			ZTest On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			int _VertCount;
			float _Round;
			bool _Reverse;

			// 5 float4 = 10 вершин
			float4 _Vertices[5];

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				nointerpolation float microRad : TEXCOORD1;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.microRad = length((_Vertices[0].xy+_Vertices[0].zw)/2*0.8);
				return o;
			}

			float2 GetVertex(int index)
			{
				int i = index >> 1;
				float4 v = _Vertices[i];

				if ((index & 1) == 0)
					return v.xy;
				else
					return v.zw;
			}

			float4 frag (v2f i) : SV_Target
			{
				// центрируем UV
				float2 p = i.uv * 2 - 1;

				// угол точки
				float tau = 6.28318530718;
				float angle = atan2(p.x, p.y)-tau/2/_VertCount;
				if (angle < 0) angle += tau;

				float sectionAngle = tau / _VertCount;

				int section = angle / sectionAngle;

				int aIndex = section;
				int bIndex = (section + 1) % _VertCount;

				float2 a = GetVertex(aIndex)*0.8;
				float2 b = GetVertex(bIndex)*0.8;

				float2 edge = b - a;
				float2 ap = p - a;

				float cross = edge.y * ap.x - edge.x * ap.y;

				float inside = step(0, cross);

				a*=(1-_Round);
				b*=(1-_Round);
				//между a и b
				float2 ab = b - a;
				ap = p - a;
				float2 offset = (a-p)/i.microRad/_Round;
				float sqMagA = dot(offset, offset);
				offset = (b-p)/i.microRad/_Round;
				float sqMagB = dot(offset, offset);
				inside = (dot(ap, ab) >= 0.0) && (dot(ap, ab) <= dot(ab, ab))?inside:((sqMagA<sqMagB?sqMagA:sqMagB)<1);
				if(_Reverse) inside = 1-inside;
				clip(inside-0.5);
				return float4(0.3,0.2,0.1,1);
			}
			ENDCG
		}
	}
}