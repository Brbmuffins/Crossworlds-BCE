Shader "Crossworlds/UI/PlayerFrameBackgroundKey"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _KeyThreshold ("Light Neutral Threshold", Range(0,1)) = 0.82
        _NeutralTolerance ("Neutral Tolerance", Range(0,0.2)) = 0.055
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            sampler2D _MainTex;
            fixed4 _Color;
            float _KeyThreshold;
            float _NeutralTolerance;

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, input.uv) * input.color;
                float channelSpread = max(color.r, max(color.g, color.b)) - min(color.r, min(color.g, color.b));
                float lightNeutral = step(_KeyThreshold, min(color.r, min(color.g, color.b))) * step(channelSpread, _NeutralTolerance);
                color.a *= 1.0 - lightNeutral;
                return color;
            }
            ENDCG
        }
    }
}
