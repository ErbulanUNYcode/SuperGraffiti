Shader "Custom/VertexMove"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TrafTex0 ("TrafTex 0", 2D) = "white" {}
		_TrafTex1 ("TrafTex 1", 2D) = "white" {}
		_TrafTex2 ("TrafTex 2", 2D) = "white" {}
		_TrafTex3 ("TrafTex 3", 2D) = "white" {}
		_TrafTex4 ("TrafTex 4", 2D) = "white" {}
		_TrafTex5 ("TrafTex 5", 2D) = "white" {}
		_TrafTex6 ("TrafTex 6", 2D) = "white" {}
		_TrafTex7 ("TrafTex 7", 2D) = "white" {}
	}
	SubShader
	{
		Tags
		{
			"RenderType"="Opaque"
			"Queue"="Overlay"
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
			fixed4 _Col[8];
			sampler2D  _TrafTex0;
			sampler2D  _TrafTex1;
			sampler2D  _TrafTex2;
			sampler2D  _TrafTex3;
			sampler2D  _TrafTex4;
			sampler2D  _TrafTex5;
			sampler2D  _TrafTex6;
			sampler2D  _TrafTex7;
			int _CurrentR;
			int _CurrentL;

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				linear float3 dif1 : TEXCOORD1;
				linear float3 dif2 : TEXCOORD2;
				linear float3 dif3 : TEXCOORD3;
				linear float3 dif4 : TEXCOORD4;
				linear float3 dif5 : TEXCOORD5;
				linear float3 dif6 : TEXCOORD6;
				linear float3 dif7 : TEXCOORD7;
				linear float3 dif8 : TEXCOORD8;
			};

			v2f vert (appdata v)
			{
				v2f o =(v2f)0;
				int id = round(v.color.r*255);
				float3 pos = _Pos[id].xyz;
				float2 rot = _Rot[id].xy;
				if((_CurrentR!=id && _CurrentL!=id && _Col[id].a == 0) || pos.z>0 || pos.z<-2.12)
				{
					v.vertex.xyz = 0;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = float2(0,0);
					return o;
				}
				float3 ver = v.vertex.xyz;
				float a=atan2(ver.y,ver.z) - rot.x;
				float l=length(ver.yz);
				ver.yz = float2(sin(a),cos(a))*l;
				a=atan2(ver.x,ver.z) + rot.y;
				l=length(ver.xz);
				ver.xz = float2(sin(a),cos(a))*l;
				bool b=ver.z>0;
				ver+=pos.xyz;
				l=4.4944-pos.z*pos.z;
				float3 offset = ver - pos.xyz;
				if(b)
				{
					ver = pos - offset/offset.z*pos.z;
					offset = ver - pos.xyz;
					if(offset.x*offset.x + offset.y*offset.y > l)
					{
						a = atan2(offset.x,offset.y);
						ver.xy = float2(sin(a),cos(a))*sqrt(l) + pos.xy;
					}
				}
				else
				{
					a = atan2(offset.x,offset.y);
					ver.xy = float2(sin(a),cos(a))*sqrt(l)+ pos.xy;
				}
				ver.z=0;
				v.vertex.xyz = ver;

				o.pos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(o.pos);
				o.uv = screenPos.xy / screenPos.w;

				float3 worldPos = v.vertex.xyz;
				float3 difs[8];
				float d;
				float3 dif;

				for (int i = 0; i<8; i++)
				{
					dif = worldPos - _Pos[i].xyz;

					//y
					d = length(dif.xz);
					a = atan2(dif.x, dif.z) - _Rot[i].y;
					dif.xz = float2(sin(a), cos(a)) * d;

					//x
					d = length(dif.yz);
					a = atan2(dif.y, dif.z) + _Rot[i].x;
					dif.yz = float2(sin(a), cos(a)) * d;
					dif.z -= 0.07;
					dif.z/=4;
					//z
					/*d = length(dif.xy);
					a = atan2(dif.x, dif.y);
					dif.xy = float2(sin(a), cos(a)) * d;*/

					dif*=2;

					difs[i] = dif;
				}
				
				o.dif1 = difs[0];
				o.dif2 = difs[1];
				o.dif3 = difs[2];
				o.dif4 = difs[3];
				o.dif5 = difs[4];
				o.dif6 = difs[5];
				o.dif7 = difs[6];
				o.dif8 = difs[7];

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				uint width, height;
				_MainTex.GetDimensions(width, height);
				int2 pixelCoord = int2(i.uv.x * width, i.uv.y * height);
				float4 col = _MainTex.Load(int3(pixelCoord, 0));
				col.a=0;
				float3 dif;
				float4 c;
				float al,d,l,traf;
				float2 r;
				
				dif = i.dif1;
				if(dif.z > 0 && dif.z+0.05 > abs(dif.x) && dif.z+0.05 > abs(dif.y))
				{
					float traf = tex2D(_TrafTex0,dif.xy/dif.z/2+0.5).a;
					if(traf<1)
					{
						r = dif.xy/dif.z;
						d = dot(r,r);
						if(d < 1)//-1%
						{
							dif.xy /= 2;
							l = dot(dif,dif);
							if(l < 1)
							{
								c = _Col[0];
								c*=c;
								al = pow((1-l) * (1-d),4) * (1-traf);
								if((_CurrentR==0||_CurrentL==0) && al>0.001) col.a = 1;
								al*=c.a;
								col.rgb *= 1 - al;
								col.rgb += al * c.rgb;
							}
						}
					}
				}
				
				dif = i.dif2;
				if(dif.z > 0 && dif.z+0.05 > abs(dif.x) && dif.z+0.05 > abs(dif.y))
				{
					float traf = tex2D(_TrafTex1,dif.xy/dif.z/2+0.5).a;
					if(traf<1)
					{
						r = dif.xy/dif.z;
						d = dot(r,r);
						if(d < 1)//-1%
						{
							dif.xy /= 2;
							l = dot(dif,dif);
							if(l < 1)
							{
								c = _Col[1];
								c*=c;
								al = pow((1-l) * (1-d),4) * (1-traf);
								if((_CurrentR==1||_CurrentL==1) && al>0.001) col.a = 1;
								al*=c.a;
								col.rgb *= 1 - al;
								col.rgb += al * c.rgb;
							}
						}
					}
				}
				
				dif = i.dif3;
				if(dif.z > 0 && dif.z+0.05 > abs(dif.x) && dif.z+0.05 > abs(dif.y))
				{
					float traf = tex2D(_TrafTex2,dif.xy/dif.z/2+0.5).a;
					if(traf<1)
					{
						r = dif.xy/dif.z;
						d = dot(r,r);
						if(d < 1)//-1%
						{
							dif.xy /= 2;
							l = dot(dif,dif);
							if(l < 1)
							{
								c = _Col[2];
								c*=c;
								al = pow((1-l) * (1-d),4) * (1-traf);
								if((_CurrentR==2||_CurrentL==2) && al>0.001) col.a = 1;
								al*=c.a;
								col.rgb *= 1 - al;
								col.rgb += al * c.rgb;
							}
						}
					}
				}
				
				dif = i.dif4;
				if(dif.z > 0 && dif.z+0.05 > abs(dif.x) && dif.z+0.05 > abs(dif.y))
				{
					float traf = tex2D(_TrafTex3,dif.xy/dif.z/2+0.5).a;
					if(traf<1)
					{
						r = dif.xy/dif.z;
						d = dot(r,r);
						if(d < 1)//-1%
						{
							dif.xy /= 2;
							l = dot(dif,dif);
							if(l < 1)
							{
								c = _Col[3];
								c*=c;
								al = pow((1-l) * (1-d),4) * (1-traf);
								if((_CurrentR==3||_CurrentL==3) && al>0.001) col.a = 1;
								al*=c.a;
								col.rgb *= 1 - al;
								col.rgb += al * c.rgb;
							}
						}
					}
				}
				
				dif = i.dif5;
				if(dif.z > 0 && dif.z+0.05 > abs(dif.x) && dif.z+0.05 > abs(dif.y))
				{
					float traf = tex2D(_TrafTex4,dif.xy/dif.z/2+0.5).a;
					if(traf<1)
					{
						r = dif.xy/dif.z;
						d = dot(r,r);
						if(d < 1)//-1%
						{
							dif.xy /= 2;
							l = dot(dif,dif);
							if(l < 1)
							{
								c = _Col[4];
								c*=c;
								al = pow((1-l) * (1-d),4) * (1-traf);
								if((_CurrentR==4||_CurrentL==4) && al>0.001) col.a = 1;
								al*=c.a;
								col.rgb *= 1 - al;
								col.rgb += al * c.rgb;
							}
						}
					}
				}
				
				dif = i.dif6;
				if(dif.z > 0 && dif.z+0.05 > abs(dif.x) && dif.z+0.05 > abs(dif.y))
				{
					float traf = tex2D(_TrafTex5,dif.xy/dif.z/2+0.5).a;
					if(traf<1)
					{
						r = dif.xy/dif.z;
						d = dot(r,r);
						if(d < 1)//-1%
						{
							dif.xy /= 2;
							l = dot(dif,dif);
							if(l < 1)
							{
								c = _Col[5];
								c*=c;
								al = pow((1-l) * (1-d),4) * (1-traf);
								if((_CurrentR==5||_CurrentL==5) && al>0.001) col.a = 1;
								al*=c.a;
								col.rgb *= 1 - al;
								col.rgb += al * c.rgb;
							}
						}
					}
				}
				
				dif = i.dif7;
				if(dif.z > 0 && dif.z+0.05 > abs(dif.x) && dif.z+0.05 > abs(dif.y))
				{
					float traf = tex2D(_TrafTex6,dif.xy/dif.z/2+0.5).a;
					if(traf<1)
					{
						r = dif.xy/dif.z;
						d = dot(r,r);
						if(d < 1)//-1%
						{
							dif.xy /= 2;
							l = dot(dif,dif);
							if(l < 1)
							{
								c = _Col[6];
								c*=c;
								al = pow((1-l) * (1-d),4) * (1-traf);
								if((_CurrentR==6||_CurrentL==6) && al>0.001) col.a = 1;
								al*=c.a;
								col.rgb *= 1 - al;
								col.rgb += al * c.rgb;
							}
						}
					}
				}
				
				dif = i.dif8;
				if(dif.z > 0 && dif.z+0.05 > abs(dif.x) && dif.z+0.05 > abs(dif.y))
				{
					float traf = tex2D(_TrafTex7,dif.xy/dif.z/2+0.5).a;
					if(traf<1)
					{
						r = dif.xy/dif.z;
						d = dot(r,r);
						if(d < 1)//-1%
						{
							dif.xy /= 2;
							l = dot(dif,dif);
							if(l < 1)
							{
								c = _Col[7];
								c*=c;
								al = pow((1-l) * (1-d),4) * (1-traf);
								if((_CurrentR==7||_CurrentL==7) && al>0.001) col.a = 1;
								al*=c.a;
								col.rgb *= 1 - al;
								col.rgb += al * c.rgb;
							}
						}
					}
				}

				return col;
			}
			ENDCG
		}
	}
}