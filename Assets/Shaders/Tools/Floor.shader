Shader "Unlit/Floor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            sampler2D _MainTex;

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                return o;
            }

            float4 GetHilbert(float2 id,float2 uv)
            {
                float d = (id.y-id.x)/4;
                int i = abs((uv.x>1)*3-(uv.y>1));
                uv=uv%1;
                uv=(i==0?uv.yx:i==3?1-uv.yx:uv)*2;
                return float4(id.x+d*(i++),id.x+d*i,uv);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 id = float2(0,100);
                float4 h = GetHilbert(id, (((i.worldPos.xz/50+1)*4)%1)*2);
                h=GetHilbert(h.xy,h.zw);
                h=GetHilbert(h.xy,h.zw);
                h=GetHilbert(h.xy,h.zw);
                h=GetHilbert(h.xy,h.zw);
                h.x+=_Time.y/3;
                float3 c=max(0,float3(sin(h.x),sin(h.x+0.5),sin(h.x-0.5))*3-2.7);
                c+=tex2D(_MainTex,i.worldPos.xz/3.125).rgb/float3(1.5,2,2);
                return float4(c,1);
            }
            ENDCG
        }
    }
}