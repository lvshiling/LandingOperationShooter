Shader "Terrain/Terrain 4 textures RGBA lightmap" {
	Properties{
		
		[NoScaleOffset] _MainTex0 ("Color 1 - R", 2D) = "white" {}
		[NoScaleOffset] _MainTex1 ("Color 2 - G", 2D) = "white" {}
		[NoScaleOffset] _MainTex2 ("Color 3 - B", 2D) = "white" {}
		[NoScaleOffset] _MainTex3 ("Color 4 - A", 2D) = "white" {}
						_Tiling ("Tiling", Vector) = (100,100,100,100)
		[NoScaleOffset] _Control("Control 1 (RGBA)", 2D) = "white" {}
_LightAdditive("sdflijds", Float) = 1.0
		[NoScaleOffset] _Light ("Lightmap", 2D) = "white" {}
	}
	
	SubShader {
		Tags {
	   "SplatCount" = "4"
	   "RenderType" = "Opaque"
		}
		
		CGPROGRAM
		#pragma surface surf NoLighting exclude_path:prepass approxview halfasview
		#pragma exclude_renderers xbox360 ps3 flash
		
		 fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			return (0.0);
		}
		
		struct Input {
			float2 uv_Control : TEXCOORD0;
			fixed2 uv2_Mask;
		};
		 
		fixed4 _Tiling, _Tiling2;
		sampler2D _Control;
		sampler2D _MainTex0,_MainTex1,_MainTex2,_MainTex3;
		sampler2D _Light;
fixed _LightAdditive;
		 

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 splat_control = tex2D (_Control, IN.uv_Control);

			fixed3 lay1 = tex2D (_MainTex0, IN.uv_Control * _Tiling.x).rgb;
			fixed3 lay2 = tex2D (_MainTex1, IN.uv_Control * _Tiling.y).rgb;
			fixed3 lay3 = tex2D (_MainTex2, IN.uv_Control * _Tiling.z).rgb;
			fixed3 lay4 = tex2D (_MainTex3, IN.uv_Control * _Tiling.w).rgb;
			
			fixed4 light = tex2D (_Light, IN.uv_Control);

			o.Alpha = 0.0;
			float alphatest = (1. - splat_control.a);
			o.Albedo.rgb = saturate(light + _LightAdditive) * (saturate(lay1 * (splat_control.r - alphatest)) + saturate(lay2 * (splat_control.g - alphatest)) + saturate(lay3 * (splat_control.b - alphatest)) + lay4 * alphatest); //умножать здесь
			//o.Emission = light*o.Albedo;
		}
		ENDCG 
	}
	// Fallback to VertexLit
	Fallback "VertexLit"
}