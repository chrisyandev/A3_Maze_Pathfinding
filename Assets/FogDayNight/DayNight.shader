Shader "Assign3/DayNight" {
Properties{
		[Toggle] _IsDay("Is Day?", Float) = 0.0
		_DayColor ("Day Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_NightColor ("Night Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_MainTex("Main Texture", 2D) = "white" {}
	}

	SubShader{
		CGPROGRAM
		#pragma surface surf Lambert
		
		float _IsDay;
		fixed4 _DayColor;
		fixed4 _NightColor;
		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 texColor = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 resultColor = _IsDay == 1.0 ? texColor * _DayColor : texColor * _NightColor;

			o.Albedo = resultColor.rgb;
            o.Alpha = resultColor.a;
		}
		ENDCG
	}

	FallBack "Standard"
}
