Shader "Custom/NewColorFilter"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}

    _CrossMagentaInput("CrossMagentaInput", Color) = (1,1,1,1)
    _CrossMagentaOutput("CrossMagentaOutput", Color) = (1,1,1,1)

	_CrossYellowInput("CrossYellowInput", Color) = (1,1,1,1)
	_CrossYellowOutput("CrossYellowOutput", Color) = (1,1,1,1)

	_CrossGreenInput("CrossGreenInput", Color) = (1,1,1,1)
	_CrossGreenOutput("CrossGreenOutput", Color) = (1,1,1,1)

	_CrossCyanInput("CrossCyanInput", Color) = (1,1,1,1)
	_CrossCyanOutput("CrossCyanOutput", Color) = (1,1,1,1)
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

	  uniform sampler2D _MainTex;

      uniform float4 _CrossMagentaInput;
      uniform float4 _CrossMagentaOutput;

	  uniform float4 _CrossYellowInput;
	  uniform float4 _CrossYellowOutput;

	  uniform float4 _CrossGreenInput;
	  uniform float4 _CrossGreenOutput;

	  uniform float4 _CrossCyanInput;
	  uniform float4 _CrossCyanOutput;


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
      
	  float4 OutputTexture;

	  float4 frag(v2f IN) : SV_Target
      {

		float4 OUT;
	  
		OutputTexture = tex2D(_MainTex, IN.texcoord.xy);

		// CrossMagenta
		if (OutputTexture.r > _CrossMagentaInput.r && OutputTexture.b > _CrossMagentaInput.b && OutputTexture.a > _CrossMagentaInput.a)
		{
			OutputTexture = _CrossMagentaOutput;
		}

		// CrossYellow
		if (OutputTexture.r > _CrossYellowInput.r && OutputTexture.g > _CrossYellowInput.g && OutputTexture.a > _CrossYellowInput.a)
		{
			OutputTexture = _CrossYellowOutput;
		}

		// CrossGreen
		if (OutputTexture.r > _CrossGreenInput.g && OutputTexture.a > _CrossGreenInput.a)
		{
			OutputTexture = _CrossGreenOutput;
		}

		// CrossCyan
		if (OutputTexture.r > _CrossCyanInput.g && OutputTexture.b > _CrossCyanInput.b && OutputTexture.a > _CrossCyanInput.a)
		{
			OutputTexture = _CrossCyanOutput;
		}

		OUT = OutputTexture;
		  
        return OUT;
      }   
      ENDCG     
    } 
  }
}
