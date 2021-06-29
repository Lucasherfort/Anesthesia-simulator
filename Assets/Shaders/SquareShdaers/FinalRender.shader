Shader "Square/FinalRender"
{
	Properties
	{
		_LayerArtery("LayerArtery", 2D) = "white" {}
		_LayerBorder("LayerBorder", 2D) = "white" {}
		_LayerNerve("LayerNerve", 2D) = "white" {}
		_LayerVeine("LayerVeine", 2D) = "white" {}
		_LayerNeedle("LayerNeedle", 2D) = "white" {}
		_LayerNoise("LayerNoise", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM

			#pragma vertex vertexFunc
			#pragma fragment fragmentFunc

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _LayerArtery;
			sampler2D _LayerNerve;
			sampler2D _LayerBorder;
			sampler2D _LayerVeine;
			sampler2D _LayerNeedle;
			sampler2D _LayerNoise;

			v2f vertexFunc(appdata IN)
			{
				v2f OUT;

				OUT.position = UnityObjectToClipPos(IN.vertex);
				OUT.uv.xy = IN.uv.xy;

				return OUT;
			}

			float4 PixelColorAzerty;
			float4 PixelColorBorder;
			float4 PixelColorNerve;
			float4 PixelColorVeine;
			float4 PixelColorNoise;
			float4 PixelColorNeedle;

			float4 fragmentFunc(v2f IN) : SV_Target
			{
				float4 finalTexture;

				PixelColorAzerty = tex2D(_LayerArtery, IN.uv.xy);
				PixelColorNerve = tex2D(_LayerNerve, IN.uv.xy);
				PixelColorVeine = tex2D(_LayerVeine, IN.uv.xy);
				PixelColorBorder = tex2D(_LayerBorder, IN.uv.xy);
				PixelColorNeedle = tex2D(_LayerNeedle, IN.uv.xy);

				PixelColorNoise = tex2D(_LayerNoise, IN.uv.xy);

				finalTexture = (PixelColorBorder + PixelColorVeine) * PixelColorNoise;

				return finalTexture;
			}
			ENDCG
		}
	}
}