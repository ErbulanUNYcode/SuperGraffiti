Shader "Custom/BezierFill"
{
    Properties
    {
        _Point ("Point B", Vector) = (0.5, 0.5, 0.5, 0.5)
        _Count ("Count", Range(3, 8)) = 1
        _Mirror ("Mirror", Range(0, 1)) = 0
        _Fold ("Fold", Range(0.2, 1)) = 1
        _Reverse ("Reverse", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                fixed4 color : COLOR;
            };

            float4 _Point;
            int _Count;
            bool _Mirror;
            bool _Reverse;
            float _Fold;

            v2f vert(appdata v)
            {
                v2f o;
                float i = round(v.color.r * 255); // твой индекс
                if(i==3) o.color=atan2(0,0);
                _Count*=2;
                if (!_Mirror && i < _Count && i%2==1) i--;
                float a = 6.2831853f / _Count;
                float2 offset = i>_Count? 0:float2(sin(a*i),cos(a*i));

                float3 localPos = float3(offset, 0);
                o.pos = UnityObjectToClipPos(localPos);
                o.uv = v.uv;
                o.color = v.color;

                return o;
            }

            float ReturnT(float b, float c, float p)
            {
                float lo = 0.0f;
                float hi = 1.0f;
                float t = 0.5f;
                float f;
                float n;
                float v;

                for (int i = 0; i < 10; i++)
                {
                    t = (lo + hi) * 0.5f;
                    
                    n = 1.0 - t;
                    v = (b*t+c*n);
                    f = ((b*n)*t+v*n)*t+(v*t+(c*t+n)*n)*n;

                    if (f > p)
                        lo = t;
                    else
                        hi = t;
                }

                return (lo + hi) * 0.5f;
            }

            float BezierFill(float2 uv)
            {
                float2 B = _Point.xy;
                float2 C = _Point.zw;

                return (ReturnT(B.x, C.x, uv.x)+ReturnT(C.y, B.y, uv.y))/*==(dot(uv-B, uv-B)>0.001 && dot(uv-C, uv-C)>0.001)*/;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv.yx*1.8;
                uv.x/= _Fold;
                float fill = BezierFill(uv);
                if (!_Reverse) fill = 2.01 - fill;
                clip(1-fill);
                return fixed4(0.15,0.1,((fill>0.99)||(i.uv.x+i.uv.y)>0.99)?1:0.05,1);
            }

            ENDCG
        }
    }
}