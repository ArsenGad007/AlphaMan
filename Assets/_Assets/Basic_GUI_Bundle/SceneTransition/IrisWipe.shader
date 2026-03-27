Shader "Custom/IrisWipe"
{
    Properties
    {
        _Radius ("Radius", Float) = 0.5
        _Softness ("Softness", Float) = 0.05
        _Color ("Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Radius;
            float _Softness;
            float4 _Color;
            float _Aspect;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f    { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv - 0.5;
                uv.x *= _Aspect; // исправляем растяжение
                float dist = length(uv);
                float alpha = smoothstep(_Radius, _Radius + _Softness, dist);
                return fixed4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
}