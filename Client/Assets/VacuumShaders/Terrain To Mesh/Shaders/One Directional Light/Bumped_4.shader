// VacuumShaders 2015
// https://www.facebook.com/VacuumShaders

Shader "VacuumShaders/Terrain To Mesh/One Directional Light/Bumped/4 Textures" 
{ 
	Properties     
	{
		_Color("Tint Color", color) = (1, 1, 1, 1)
		_V_T2M_Control ("Control Map (RGBA)", 2D) = "black" {}
		 
		//TTM				
		[V_T2M_SplatBumpedMap] _V_T2M_Splat1 ("Layer 1 (R)", 2D) = "white" {}
		[HideInInspector] _V_T2M_Splat1_uvScale("", float) = 1	
		[HideInInspector] _V_T2M_Splat1_bumpMap("", 2D) = ""{}	

		[V_T2M_SplatBumpedMap] _V_T2M_Splat2 ("Layer 2 (G)", 2D) = "white" {}
		[HideInInspector] _V_T2M_Splat2_uvScale("", float) = 1	
		[HideInInspector] _V_T2M_Splat2_bumpMap("", 2D) = ""{}	
		
		[V_T2M_SplatBumpedMap] _V_T2M_Splat3 ("Layer 3 (B)", 2D) = "white" {}
		[HideInInspector] _V_T2M_Splat3_uvScale("", float) = 1	
		[HideInInspector] _V_T2M_Splat3_bumpMap("", 2D) = ""{}	
		
		[V_T2M_SplatBumpedMap] _V_T2M_Splat4 ("Layer 4 (A)", 2D) = "white" {}
		[HideInInspector] _V_T2M_Splat4_uvScale("", float) = 1	
		[HideInInspector] _V_T2M_Splat4_bumpMap("", 2D) = ""{}		
	}
	 
	
	SubShader   
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		     
		Pass 
	    {   
			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" } 

			CGPROGRAM
			#pragma vertex vert 
	    	#pragma fragment frag
			#define UNITY_PASS_FORWARDBASE   		  
			#pragma multi_compile_fwdbase nodynlightmap nodirlightmap
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#pragma exclude_renderers flash


			#define V_T2M_3_TEX
			#define V_T2M_4_TEX
			#define V_T2M_BUMP

			#include "../cginc/T2M_ODL.cginc" 

			ENDCG

		} //Pass

	} //SubShader
	 
	Fallback "Legacy Shaders/VertexLit"

} //Shader
