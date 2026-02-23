Shader "Custom/CRT_WorldInterp_Raw"
{
	Properties
	{
		_GrainLevel("Grain Level", Range(0, 1)) = 0
		_Count("Count", int) = 0
	}

	SubShader
	{
		Lighting Off
		Blend One Zero

		Pass
		{
			Name "Interp"

			CGPROGRAM
			#include "UnityCustomRenderTexture.cginc"
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag

			// 4 угла квадрата в мировых координатах (Vector3), передаём с CPU
			float4 _Corners[32];
			int _Count;
			float _GrainLevel;

			float4 frag(v2f_customrendertexture IN) : SV_Target
			{
				float4 col = tex2D(_SelfTexture2D, IN.localTexcoord.xy);
				[loop]
				for(int i = 0; i < _Count; i++)
				{
					int id = i * 4;
					//float4 col = 0;
					float3 top = lerp(_Corners[id].xyz, _Corners[1 + id].xyz, IN.localTexcoord.x);
					float3 bottom = lerp(_Corners[2 + id].xyz, _Corners[3 + id].xyz, IN.localTexcoord.x);
					float3 pos = lerp(top, bottom, IN.localTexcoord.y);

					float3 minB = float3(-0.25, -0.25, 0);
					float3 maxB = float3( 0.25,  0.25, 1);

					float3 d = max(minB - pos, pos - maxB);
					float outside = max(max(d.x, d.y), d.z);

					#define BORDER
					if (outside > 0) continue;

					if(_GrainLevel != 0)
					{
					}

					float mag = dot(pos, pos);
					if(mag > 1) continue;
					float dt = dot(pos.xy/pos.z, pos.xy/pos.z);
					dt = max((1-dt*10) * (1- mag),0);
					dt*= dt;
					if(dt < 0.02) continue;
					dt = (dt-0.02)/0.98;
					dt*= dt;
					dt *= (1- mag);
					col.rgb *= 1 - dt;
					col.rgb += dt*(i==0?float3(1,0,0):float3(1,0.5,0));
				}

				return col;
			}

			ENDCG
		}
	}
}