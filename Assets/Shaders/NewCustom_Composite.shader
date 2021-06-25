Shader "Custon/NewComposite"
{
	Properties
	{
		_LayerArtery("LayerArtery", 2D) = "white" {}
		_LayerFilter("LayerFilter", 2D) = "white" {}
		_LayerMuscle("LayerMuscle", 2D) = "white" {}
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
			sampler2D _LayerFilter;
			sampler2D _LayerNerve;
			sampler2D _LayerMuscle;
			sampler2D _LayerVeine;
			sampler2D _LayerNeedle;
			sampler2D _LayerNoise;

			float4 u_xlat0;
			float4 u_xlat1;

			v2f vertexFunc(appdata IN)
			{
				v2f OUT;

				OUT.position = UnityObjectToClipPos(IN.vertex);
				OUT.uv.xy = IN.uv.xy;

				return OUT;
			}

			float4 PixelColorAzerty;
			float4 PixelColorMuscle;
			float4 PixelColorNerve;
			float4 PixelColorVeine;
			float4 PixelColorNoise;
			float4 PixelColorFilter;
			float4 PixelColorNeedle;
			
			float4 NoiseTextureResult;

			float4 fragmentFunc(v2f IN) : SV_Target
			{
				float4 OUT;

				PixelColorMuscle = tex2D(_LayerMuscle, IN.uv.xy);
				PixelColorAzerty = tex2D(_LayerArtery, IN.uv.xy);
				PixelColorNerve = tex2D(_LayerNerve, IN.uv.xy);
				PixelColorVeine = tex2D(_LayerVeine, IN.uv.xy);

				PixelColorNoise = tex2D(_LayerNoise, IN.uv.xy);

				OUT = (PixelColorAzerty + PixelColorVeine) * PixelColorNoise;

				return OUT;
			}
			ENDCG
		}
	}
}