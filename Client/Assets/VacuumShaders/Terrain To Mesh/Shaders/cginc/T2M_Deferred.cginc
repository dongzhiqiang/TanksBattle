#ifndef VACUUM_SHADERS_T2M_DEFERRED_CGINC
#define VACUUM_SHADERS_T2M_DEFERRED_CGINC

#include "../cginc/T2M_Variables.cginc"

struct Input 
{
	float2 uv_V_T2M_Control;

	#ifdef V_T2M_2_CONTROL_MAPS
		float2 uv_V_T2M_Control2;
	#endif
};

#ifdef V_T2M_STANDARD
void surf (Input IN, inout SurfaceOutputStandard o)
#else
void surf (Input IN, inout SurfaceOutput o) 
#endif
{
	half4 splat_control = tex2D (_V_T2M_Control, IN.uv_V_T2M_Control);

	fixed4 mainTex  = splat_control.r * tex2D (_V_T2M_Splat1, IN.uv_V_T2M_Control * _V_T2M_Splat1_uvScale);
	       mainTex += splat_control.g * tex2D (_V_T2M_Splat2, IN.uv_V_T2M_Control * _V_T2M_Splat2_uvScale);
	
	#ifdef V_T2M_3_TEX
		mainTex += splat_control.b * tex2D (_V_T2M_Splat3, IN.uv_V_T2M_Control * _V_T2M_Splat3_uvScale);
	#endif
	#ifdef V_T2M_4_TEX
		mainTex += splat_control.a * tex2D (_V_T2M_Splat4, IN.uv_V_T2M_Control * _V_T2M_Splat4_uvScale);
	#endif


	#ifdef V_T2M_2_CONTROL_MAPS
		 half4 splat_control2 = tex2D (_V_T2M_Control2, IN.uv_V_T2M_Control2);

		 mainTex.rgb += tex2D (_V_T2M_Splat5, IN.uv_V_T2M_Control2 * _V_T2M_Splat5_uvScale) * splat_control2.r;

		 #ifdef V_T2M_6_TEX
			mainTex.rgb += tex2D (_V_T2M_Splat6, IN.uv_V_T2M_Control2 * _V_T2M_Splat6_uvScale) * splat_control2.g;
		 #endif

		 #ifdef V_T2M_7_TEX
			mainTex.rgb += tex2D (_V_T2M_Splat7, IN.uv_V_T2M_Control2 * _V_T2M_Splat7_uvScale) * splat_control2.b;
		 #endif

		 #ifdef V_T2M_8_TEX
			mainTex.rgb += tex2D (_V_T2M_Splat8, IN.uv_V_T2M_Control2 * _V_T2M_Splat8_uvScale) * splat_control2.a;
		 #endif
	#endif



	mainTex.rgb *= _Color.rgb;

	 
	#ifdef V_T2M_BUMP
		fixed4 nrm = 0.0f;
		nrm += splat_control.r * tex2D(_V_T2M_Splat1_bumpMap, IN.uv_V_T2M_Control * _V_T2M_Splat1_uvScale);
		nrm += splat_control.g * tex2D(_V_T2M_Splat2_bumpMap, IN.uv_V_T2M_Control * _V_T2M_Splat2_uvScale);

		#ifdef V_T2M_3_TEX
			nrm += splat_control.b * tex2D (_V_T2M_Splat3_bumpMap, IN.uv_V_T2M_Control * _V_T2M_Splat3_uvScale);
		#endif

		#ifdef V_T2M_4_TEX
			nrm += splat_control.a * tex2D (_V_T2M_Splat4_bumpMap, IN.uv_V_T2M_Control * _V_T2M_Splat4_uvScale);
		#endif
		 
		 
		o.Normal = UnpackNormal(nrm);
	#endif


	

	#ifdef V_T2M_STANDARD
		o.Metallic = _Metallic;
		o.Smoothness = _Glossiness;
	#else
		#ifdef V_T2M_SPECULAR
			o.Gloss = mainTex.a;
			o.Specular = _Shininess;
		#endif
	#endif
	
	o.Albedo = mainTex.rgb;
	o.Alpha = 1.0;
}

#endif