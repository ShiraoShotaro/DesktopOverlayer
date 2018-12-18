Shader "DOVR/DesktopShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FadeRate ("Fade Rate", Range(0.0, 1.0)) = 0
		[KeywordEnum(NORMAL, BOTTOM2UP, UP2BOTTOM)] _Mode ("Fade Mode", Float) = 0
		_FadeSize ("Fade Height / Width", Range(0.0, 1.0)) = 0.2
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "RenderType"="Transparent" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _MODE_NORMAL _MODE_UP2BOTTOM _MODE_BOTTOM2UP
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float _FadeRate;	// フェードのレート
			float _FadeSize;	// フェード幅

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			float calcSingleFadeAlpha(float pos) {
				float P = (_FadeSize + 1) * _FadeRate;
				float Q = P - _FadeSize;
				float Alpha = 0;
				if (pos >= P) Alpha = 1.0f;
				else if (P > pos && pos > Q) Alpha = (pos - Q) / _FadeSize;
				return Alpha;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// Fadeを実装
				float Alpha = _FadeRate;
				#ifdef _MODE_UP2BOTTOM
				Alpha = calcSingleFadeAlpha(i.uv.y);

				#elif _MODE_BOTTOM2UP
				Alpha = calcSingleFadeAlpha(1 - i.uv.y);
				#else

				#endif
				
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				col.a = Alpha;
				return col;
			}
			ENDCG
		}
	}
}
