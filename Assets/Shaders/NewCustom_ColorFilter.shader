Shader "Custom/NewColorFilter"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}
    _InputColor("InputColor", Color) = (1,0,1,0)
    _OutputColor("OutputColor", Color) = (1,1,1,0)
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
      uniform float4 _InputColor;
      uniform float4 _OutputColor;
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
      
	  float4 OutputTexture;

	  float4 frag(v2f IN) : SV_Target
      {

		float4 OUT;
	  
		OutputTexture.rgba = tex2D(_MainTex, IN.texcoord.xy).rgba;

		if (_InputColor.r == float4(1, 0, 1, 0).r)
		{
			OutputTexture = _OutputColor;
		}

		OUT = OutputTexture;
		  
        return OUT;
      }   
      ENDCG     
    } 
  }
}
