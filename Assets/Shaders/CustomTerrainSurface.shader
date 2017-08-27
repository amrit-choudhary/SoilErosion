Shader "Custom/CustomTerrainSurface" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_TerrainGradientTex("Terrain Gradient", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_MaxHeight("MaxHeight", Float) = 0
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			struct Input {
				float2 uv_MainTex;
				float3 worldPos;
			};

			sampler2D _MainTex;
			sampler2D _TerrainGradientTex;
			half _Glossiness;
			half _Metallic;
			fixed4 _Color;
			float _MaxHeight;

			void surf(Input IN, inout SurfaceOutputStandard o) {
				// Albedo comes from a texture tinted by color
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				c = tex2D(_TerrainGradientTex, float2(IN.worldPos.y / _MaxHeight, 0.5));
				o.Albedo = c.rgb;
				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = 1;
			}
			ENDCG
		}
			FallBack "Diffuse"
}
