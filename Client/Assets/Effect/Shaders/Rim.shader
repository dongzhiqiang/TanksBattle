// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Rim" {
	Properties {
	_Color ("Rim Color", Color) = (0.5,0.5,0.5,0.5)
	_FPOW("FPOW Fresnel", Float) = 5.0
    _R0("R0 Fresnel", Float) = 0.05
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha One
	
	AlphaTest Greater .01
	//ColorMask RGB
	Lighting Off 
	ZWrite Off 
	//Fog { Color (0,0,0,0) }
	Fog {mode off }

	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			
			fixed4 _Color;
			half _FPOW;
			half _R0;
			
			struct appdata_t {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				
				float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                half fresnel = saturate(1.0 - dot(v.normal, viewDir));
				fresnel = pow(fresnel, _FPOW);
				fresnel = _R0 + (1.0 - _R0) * fresnel;
				o.color = fresnel;
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				return 2.0f * i.color * _Color ;
			}
			ENDCG 
		}
	}	
}
}
