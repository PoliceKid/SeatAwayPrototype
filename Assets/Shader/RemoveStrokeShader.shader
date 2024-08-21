Shader "Custom/RemoveStrokeShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _StrokeColor ("Stroke Color", Color) = (0,0,0,1)
        _StrokeWidth ("Stroke Width", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _StrokeColor;
            float _StrokeWidth;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                float strokeFactor = smoothstep(_StrokeWidth, _StrokeWidth + 0.01, length(i.uv - 0.5) - 0.5 + _StrokeWidth);

                // Kiểm tra nếu pixel nằm ở rìa ngoài cùng thì giữ stroke
                if (strokeFactor > 0.5)
                {
                    return _StrokeColor;
                }

                // Nếu không thì giữ nguyên màu của texture
                return texColor * i.color;
            }
            ENDCG
        }
    }
}