Shader "tk2d/LitPremulVertexColor" 
{
	Properties 
	{
	    _MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off Blend One OneMinusSrcAlpha Cull Off Fog { Mode Off }
		LOD 110

		CGPROGRAM
		#pragma surface surf Lambert alpha
		struct Input 
		{
			float2 uv_MainTex;
			fixed4 color : COLOR;
		};

		sampler2D _MainTex;
		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 mainColor = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
			o.Albedo = mainColor.rgb;
			o.Alpha = mainColor.a;
		}
		ENDCG		
	}


	Fallback "tk2d/PremulVertexColor", 1
}
