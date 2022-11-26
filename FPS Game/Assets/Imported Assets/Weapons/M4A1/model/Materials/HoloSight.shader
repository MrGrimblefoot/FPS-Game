Shader "Custom/HoloSight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,0,0,1)
        _Distance ("Distance", Float) = 100
        _Scale ("Scale", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100

        Pass
        {
            ZTest Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Distance;
            float _Scale;

            v2f vert (appdata v)
            {
                v2f o;

                float3 lens_origin = UnityObjectToViewPos(float3(0, 0, 0));
                float3 p0 = UnityObjectToViewPos(float3(0, 0, _Distance));
                float3 n = UnityObjectToViewPos(float3(0, 0, 1)) - lens_origin;
                float3 uDir = UnityObjectToViewPos(float3(1, 0, 0)) - lens_origin;
                float3 vDir = UnityObjectToViewPos(float3(0, 1, 0)) - lens_origin;
                float3 vert = UnityObjectToViewPos(v.vertex);

                float a = dot(p0, n) / dot(vert, n);
                float3 vert_prime = a * vert;

                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //o.uv = v.vertex.xy + float2(0.5, 0.5);
                o.uv = float2(dot(vert_prime - p0, uDir), dot(vert_prime - p0, vDir));
                o.uv = o.uv / ((_Scale / 100) * a);
                o.uv += float2(0.5, 0.5);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 tex = tex2D(_MainTex, i.uv);

                fixed4 col = _Color;
                col.a = tex.r;

                if (i.uv.x < 0 || i.uv.x > 1 || i.uv.y < 0 || i.uv.y > 1)
                    col.a = 0;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
