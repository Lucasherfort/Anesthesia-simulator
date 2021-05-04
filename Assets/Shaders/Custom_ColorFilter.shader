Shader "Custom/ColorFilter"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}
    _PickColor1 ("Visable Color 1", Color) = (1,0,1,0)
    _PickColor2 ("Visable Color 2", Color) = (0,1,0,0)
    _OutputColor1 ("Output Color 1", Color) = (1,1,1,1)
    _OutputColor2 ("Output Color 2", Color) = (0.5,0.5,0.5,1)
  }
  SubShader
  {
    Pass 
    {
      CGPROGRAM
      
      #pragma vertex vert
      #pragma fragment frag
      
      #include "UnityCG.cginc"
      
      
      #define CODE_BLOCK_VERTEX
      uniform float4 _PickColor1;
      uniform float4 _PickColor2;
      uniform float4 _OutputColor1;
      uniform float4 _OutputColor2;
      uniform sampler2D _MainTex;

      struct appdata_t
      {
          float4 vertex :POSITION0;
          float2 texcoord :TEXCOORD0;
      };
      
      struct OUT_Data_Vert
      {
          float2 texcoord :TEXCOORD0;
          float4 vertex :SV_POSITION;
      };
      
      struct v2f
      {
          float2 texcoord :TEXCOORD0;
          float4 vertex :SV_POSITION;
      };
      
      struct OUT_Data_Frag
      {
          float4 color :SV_Target;
      };

      OUT_Data_Vert vert(appdata_t IN)
      {
          OUT_Data_Vert OUT;
          OUT.texcoord.xy = IN.texcoord.xy;
          OUT.vertex = UnityObjectToClipPos(IN.vertex);
          return OUT;
      }
      
      float4 PixelColorTexture1;
      float4 PixelColorTexture2;

      float4 frag(v2f IN) : SV_Target
      {

          float4 OUT;
	  
		  PixelColorTexture1 = tex2D(_MainTex, IN.texcoord.xy);

		  if (PixelColorTexture1.r > 0.5)
		  {
			  PixelColorTexture1 = _OutputColor1;
		  }
		  else
		  {
			  PixelColorTexture1 = float4(0, 0, 0, 0);
		  }

		  PixelColorTexture2 = tex2D(_MainTex, IN.texcoord.xy);

		  if (PixelColorTexture2.g > 0.5)
		  {
			  PixelColorTexture2 = _OutputColor2;
		  }
		  else
		  {
			  PixelColorTexture2 = float4(0, 0, 0, 0);
		  }

		  OUT = PixelColorTexture1 + PixelColorTexture2;
		  
          return OUT;
      }   
      ENDCG     
    } 
  }
}
