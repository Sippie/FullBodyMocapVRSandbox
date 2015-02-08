Shader "Custom Shaders/Detail/Detail Diffuse Vertex Colors" {
	Properties {
		//_Color ("Main Color", Color) = (1, 1, 1, 1)
		//_MainTexOp ("Main Texture Opacity", Range (0,1)) = 1
		//_DirtTexOp ("Dirt Texture Opacity", Range (0,1)) = 1
		_MainTex ("Main Texture", 2D) = "white" {}
		_DirtMap ("Dirt Texture", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		//#pragma surface surf Lambert
		#pragma surface surf Lambert approxview halfasview

		sampler2D _MainTex;
		sampler2D _DirtMap;
		//fixed4 _Color;
		//fixed _MainTexOp;
		//fixed _DirtTexOp;
		
		struct Input {
			float2 uv_MainTex;
			float2 uv_DirtMap;
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			/*fixed3 main = tex2D (_MainTex, IN.uv_MainTex).rgb;
			fixed3 dirt = tex2D (_DirtMap, IN.uv_DirtMap).rgb;
						
			o.Albedo = ((main * IN.color.rgb) + (dirt * IN.color.a));*/
			o.Alpha = 1.0;			
			
			float4 main = tex2D(_MainTex,(IN.uv_MainTex.xyxy).xy);
			float4 Multiply0 = IN.color * main;
			float4 dirt = tex2D(_DirtMap,(IN.uv_DirtMap.xyxy).xy);
			float4 Multiply1 = Multiply0 * dirt;
			float4 Invert0 = float4(1.0, 1.0, 1.0, 1.0) - float4( IN.color.w, IN.color.w, IN.color.w, IN.color.w);
			float4 Lerp0 = lerp(Multiply0, Multiply1, Invert0);
			o.Albedo = Lerp0;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
