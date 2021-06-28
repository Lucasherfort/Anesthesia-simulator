Shader "Phantom/ColorFilter"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}
	_OutputElementsColor("OutputElementsColor", Color) = (0,1,0,0)
	_OutputFilterColor("OutputFilterColor", Color) = (0.5,0.5,0.5,1)
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
      uniform float4 _CrossElementsColor;
      uniform float4 _OutputElementsColor;
      uniform float4 _CrossFilterColor;
      uniform float4 _OutputFilterColor;
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
     
      float4 frag(v2f IN) : SV_Target
      {

          float4 OutputTexture;
		  float4 OutputTexture1;
	      float4 OutputTexture2;
	  
		  OutputTexture1 = tex2D(_MainTex, IN.texcoord.xy);

		  // Magenta filter
		  if (OutputTexture1.r > 0.5f && OutputTexture1.b > 0.5f && OutputTexture1.a > 0.5f)
		  {
			  OutputTexture1 = _OutputElementsColor;
		  }
		  else
		  {
			  OutputTexture1 = float4(0, 0, 0, 0);
		  }

		  OutputTexture2 = tex2D(_MainTex, IN.texcoord.xy);

		  // Green filter
		  if (OutputTexture2.g > 0.5f && OutputTexture2.a > 0.5f)
		  {
			  OutputTexture2 = _OutputFilterColor;
		  }
		  else
		  {
			  OutputTexture2 = float4(0, 0, 0, 0);
		  }

		  OutputTexture = OutputTexture1 + OutputTexture2;
		  
          return OutputTexture;
      }   
      ENDCG     
    } 
  }
}
