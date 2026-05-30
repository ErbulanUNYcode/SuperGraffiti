Shader "Graffiti/Render"
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
			ZWrite On
			ZTest On
			Blend One Zero

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			Texture2D _MainTex;
			float4 _Pos[8];
			float4 _Rot[8];
			fixed4 _Col[8];
			float4 _Grain1;
			float4 _Grain2;
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
				noperspective float3 dif1 : TEXCOORD0;
				noperspective float3 dif2 : TEXCOORD1;
				noperspective float3 dif3 : TEXCOORD2;
				noperspective float3 dif4 : TEXCOORD3;
				noperspective float3 dif5 : TEXCOORD4;
				noperspective float3 dif6 : TEXCOORD5;
				noperspective float3 dif7 : TEXCOORD6;
				noperspective float3 dif8 : TEXCOORD7;
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
					d = length(dif.xy);
					a = atan2(dif.x, dif.y) + _Rot[i].z;
					dif.xy = float2(sin(a), cos(a)) * d;

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


			float Rand(int2 p)
			{
				uint n = asuint(p.x) * 1973u + asuint(p.y) * 9277u + 26699u;
				n = (n << 13u) ^ n;
				return (float)((n * (n * n * 15731u + 789221u) + 1376312589u) & 0x7fffffffu) / 2147483648.0;
			}


			float2 Spray(float3 dif,float2 rand,float power,float type,sampler2D trafTex,bool current)
			{
				float s,d,l,traf;
				float2 r,result=0;

				if(type==0 || current)
				{
					if(dif.z > 0 && dif.z+0.05 > abs(dif.x) && dif.z+0.05 > abs(dif.y))
					{
						r = dif.xy/dif.z;
						d = dot(r,r);
						if(d < 1)
						{
							dif.xy *= 0.5;
							l = dot(dif,dif);
							if(l < 1)
							{
								traf = tex2D(trafTex,dif.xy/dif.z+0.5).a;
								if(traf<1)
								{
									result.x = pow((1-l) * (1-d),4) * (1-traf);
									if(current && result.x>0.001) result.y = 1;
									if(type!=0) result.x = 0;
									result.x*=power;
								}
							}
							dif.xy*=2;
						}
					}
				}

				float3 difOrig = dif;
				if(type>0)
				{
					s = 40/type;
					dif*=s;
					dif.xy-=(dif.xy+2048+rand)%1-0.5;
					dif.z-=(dif.z+Rand(dif.xy+rand.yx*100))%1-0.5;
					dif/=s;
					if(dif.z > 0 && dif.z+0.05 > abs(dif.x) && dif.z+0.05 > abs(dif.y))
					{
						r = dif.xy/dif.z;
						d = dot(r,r);
						if(d < 1)
						{
							dif.xy *= 0.5;
							l = dot(dif,dif);
							if(l < 1)
							{
								traf = tex2D(trafTex,dif.xy/dif.z+0.5).a;
								if(traf<1)
								{
									dif.xy *= 2;
									r.x=dot((dif-difOrig)*s*2,(dif-difOrig)*s*2);
									result.x = r.x<1;
									result.x*=(pow((1-l) * (1-d),4) * (1-traf)*power>Rand(dif.xy*dif.z*100))*(1-r.x);
								}
							}
						}
					}
				}
				else if(type<0)
				{
					s = -10/type;
					dif*=s;
					dif.xy-=(dif.xy+2048+rand)%1-0.5;
					dif.z-=(dif.z+Rand(dif.xy+rand.yx*100))%1-0.5;
					dif/=s;
					if(dif.z > 0 && dif.z+0.05 > abs(dif.x) && dif.z+0.05 > abs(dif.y))
					{
						r = dif.xy/dif.z;
						d = dot(r,r);
						if(d < 1)
						{
							dif.xy *= 0.5;
							l = dot(dif,dif);
							if(l < 1)
							{
								traf = tex2D(trafTex,dif.xy/dif.z+0.5).a;
								if(traf<1)
								{
									dif.xy *= 2;
									difOrig+=(difOrig<=dif?0.5:-0.5)/s;
									difOrig = (dif-difOrig)*s*2;
									difOrig*=difOrig;
									difOrig*=difOrig;
									r.x = (difOrig.x+difOrig.y+difOrig.z);
									result.x = r.x>2;
									result.x*=(pow((1-l) * (1-d),4) * (1-traf)*power*(2-dif.z)>Rand(dif.xy*dif.z*100))*min((r.x-2)*3,1);
								}
							}
						}
					}
				}

				return result;
			}


			fixed4 frag (v2f i) : SV_Target
			{
				uint width, height;
				_MainTex.GetDimensions(width, height);
				float2 uv = i.pos.xy / _ScreenParams.xy;
				int2 pixelCoord = int2(uv.x * width, uv.y * height);
				float4 col = _MainTex.Load(int3(pixelCoord, 0));
				col.a=0;
				float old = col;
				
				_Col[0].rgb*=_Col[0].rgb;
				float2 sp = Spray
				(
					i.dif1,
					float2(_Pos[0].w,_Rot[0].w),
					_Col[0].a*1.5,
					_Grain1.x,
					_TrafTex0,
					_CurrentR==0||_CurrentL==0
				);
				col.xyz*=1-sp.x;
				col.xyz+=_Col[0].xyz*_Col[0].xyz*sp.x;
				col.a=sp.y;
				
				_Col[1].rgb*=_Col[1].rgb;
				sp = Spray
				(
					i.dif2,
					float2(_Pos[1].w,_Rot[1].w),
					_Col[1].a*1.5,
					_Grain1.y,
					_TrafTex1,
					_CurrentR==1||_CurrentL==1
				);
				col.xyz*=1-sp.x;
				col.xyz+=_Col[1].xyz*_Col[1].xyz*sp.x;
				col.a+=sp.y;

				_Col[2].rgb*=_Col[2].rgb;
				sp = Spray
				(
					i.dif3,
					float2(_Pos[2].w,_Rot[2].w),
					_Col[2].a*1.5,
					_Grain1.z,
					_TrafTex2,
					_CurrentR==2||_CurrentL==2
				);
				col.xyz*=1-sp.x;
				col.xyz+=_Col[2].xyz*_Col[2].xyz*sp.x;
				col.a+=sp.y;

				_Col[3].rgb*=_Col[3].rgb;
				sp = Spray
				(
					i.dif4,
					float2(_Pos[3].w,_Rot[3].w),
					_Col[3].a*1.5,
					_Grain1.w,
					_TrafTex3,
					_CurrentR==3||_CurrentL==3
				);
				col.xyz*=1-sp.x;
				col.xyz+=_Col[3].xyz*_Col[3].xyz*sp.x;
				col.a+=sp.y;

				_Col[4].rgb*=_Col[4].rgb;
				sp = Spray
				(
					i.dif5,
					float2(_Pos[4].w,_Rot[4].w),
					_Col[4].a*1.5,
					_Grain2.x,
					_TrafTex4,
					_CurrentR==4||_CurrentL==4
				);
				col.xyz*=1-sp.x;
				col.xyz+=_Col[4].xyz*_Col[4].xyz*sp.x;
				col.a+=sp.y;

				_Col[5].rgb*=_Col[5].rgb;
				sp = Spray
				(
					i.dif6,
					float2(_Pos[5].w,_Rot[5].w),
					_Col[5].a*1.5,
					_Grain2.y,
					_TrafTex5,
					_CurrentR==5||_CurrentL==5
				);
				col.xyz*=1-sp.x;
				col.xyz+=_Col[5].xyz*_Col[5].xyz*sp.x;
				col.a+=sp.y;

				_Col[6].rgb*=_Col[6].rgb;
				sp = Spray
				(
					i.dif7,
					float2(_Pos[6].w,_Rot[6].w),
					_Col[6].a*1.5,
					_Grain2.z,
					_TrafTex6,
					_CurrentR==6||_CurrentL==6
				);
				col.xyz*=1-sp.x;
				col.xyz+=_Col[6].xyz*_Col[6].xyz*sp.x;
				col.a+=sp.y;

				_Col[7].rgb*=_Col[7].rgb;
				sp = Spray
				(
					i.dif8,
					float2(_Pos[7].w,_Rot[7].w),
					_Col[7].a*1.5,
					_Grain2.w,
					_TrafTex7,
					_CurrentR==7||_CurrentL==7
				);
				col.xyz*=1-sp.x;
				col.xyz+=_Col[7].xyz*_Col[7].xyz*sp.x;
				col.a+=sp.y;

				if(all(col == old)) clip(-1);

				return min(col,0.999);
			}
			ENDCG
		}
	}
}