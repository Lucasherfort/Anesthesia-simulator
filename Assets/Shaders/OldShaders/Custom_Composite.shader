Shader "OldShader/Composite"
{
	// Paramètres dans l'inspecteur
	Properties
	{
		_Crossection1("Crossection 1 (layer1)", 2D) = "white" {}
		_Crossection2("Crossection 2 (layer2)", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
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

			sampler2D _Crossection1;
			sampler2D _Crossection2;
			sampler2D _Noise;

			float4 u_xlat0;
			float4 u_xlat1;

			v2f vertexFunc(appdata IN)
			{
				v2f OUT;

				OUT.position = UnityObjectToClipPos(IN.vertex);
				OUT.uv.xy = IN.uv.xy;

				return OUT;
			}

			float4 PixelColorTexture1;
			float4 PixelColorTexture2;

			float4 fragmentFunc(v2f IN) : SV_Target
			{
				float4 OUT;
				PixelColorTexture1 = tex2D(_Crossection1, IN.uv.xy);
				PixelColorTexture2 = tex2D(_Crossection2, IN.uv.xy);
				PixelColorTexture1 = (PixelColorTexture1 + PixelColorTexture2);
				PixelColorTexture2 = tex2D(_Noise, IN.uv.xy);

				OUT = (PixelColorTexture1 * PixelColorTexture2);

				return OUT;
			}
			ENDCG
		}
	}
}