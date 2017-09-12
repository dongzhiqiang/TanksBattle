#ifndef VACUUM_SHADERS_T2M_VARIABLES_CGINC
#define VACUUM_SHADERS_T2M_VARIABLES_CGINC

fixed4 _Color;

sampler2D _V_T2M_Control;
sampler2D _V_T2M_Splat1; half _V_T2M_Splat1_uvScale;
sampler2D _V_T2M_Splat2; half _V_T2M_Splat2_uvScale;

#ifdef V_T2M_BUMP
	sampler2D _V_T2M_Splat1_bumpMap;
	sampler2D _V_T2M_Splat2_bumpMap;
#endif

#ifdef V_T2M_3_TEX
sampler2D _V_T2M_Splat3; half _V_T2M_Splat3_uvScale;

	#ifdef V_T2M_BUMP
		sampler2D _V_T2M_Splat3_bumpMap;
	#endif
#endif

#ifdef V_T2M_4_TEX
sampler2D _V_T2M_Splat4; half _V_T2M_Splat4_uvScale;

	#ifdef V_T2M_BUMP
		sampler2D _V_T2M_Splat4_bumpMap;
	#endif
#endif 

#ifdef V_T2M_2_CONTROL_MAPS
	sampler2D _V_T2M_Control2;

	sampler2D _V_T2M_Splat5; half _V_T2M_Splat5_uvScale;
	#ifdef V_T2M_BUMP
		sampler2D _V_T2M_Splat5_bumpMap;
	#endif

	#ifdef V_T2M_6_TEX
		sampler2D _V_T2M_Splat6; half _V_T2M_Splat6_uvScale;
		#ifdef V_T2M_BUMP
			sampler2D _V_T2M_Splat6_bumpMap;
		#endif
	#endif

	#ifdef V_T2M_7_TEX
		sampler2D _V_T2M_Splat7; half _V_T2M_Splat7_uvScale;
	#endif

	#ifdef V_T2M_8_TEX
		sampler2D _V_T2M_Splat8; half _V_T2M_Splat8_uvScale;
	#endif
#endif

#ifdef V_T2M_SPECULAR
	half _Shininess;
#endif

#ifdef V_T2M_STANDARD
	half _Glossiness;
	half _Metallic;
#endif

#endif
