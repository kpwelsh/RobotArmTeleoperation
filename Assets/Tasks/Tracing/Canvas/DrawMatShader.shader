Shader "Unlit/DrawMatShader"
{
    Properties
    {
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha 
        Tags { "RenderType"="Opaque" }
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _UVPosition;
            float4x4 _NormalizedToWorld; 
            float4 _LastDrawPoint;
            float4 _DrawPoint;
            float _Radius;
            fixed4 _DrawColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                if (_Radius < 0) return float4(0,0,0,0);
                float r = _Radius * _Radius;

                float3 pos = tex2D(_UVPosition, i.uv).rgb;
                pos = mul(_NormalizedToWorld, float4(pos, 1)).xyz; 

                float3 a = pos - _DrawPoint;
                float3 b = pos - _LastDrawPoint;
                
                if (dot(a, a) <= r || dot(b, b) <= r) {
                    return _DrawColor;
                }
                float3 n = _LastDrawPoint - _DrawPoint;
                float l = sqrt(dot(n,n));
                n = n / l;
                float projectedDistance = dot(a, n);
                if (projectedDistance >= 0 
                    && projectedDistance <= l
                    && dot(a, a) - projectedDistance * projectedDistance <= r) {
                    
                    return _DrawColor;
                }
                return float4(0,0,0,0);
            }
            ENDCG
        }
    }
}
