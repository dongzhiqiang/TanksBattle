Shader "Hidden/J3Tech/Volumetric Fog/First Person Volumetric Fog Cull Off"
{
    Properties
    {
        [HideInInspector]_Color ("Main Color", Color) = (1,1,1,1)
        [HideInInspector]_Visibility ("Visibility", Float) = 1.0
		[HideInInspector]_Start ("Start Fade", Float) = 1.0
		[HideInInspector]_End ("End Fade", Float) = 1.0
		[HideInInspector]_Size ("Size", Vector) = (1,1,1,1)
    }
    
    SubShader
    {
        Tags { "Queue"="Overlay-1" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha 

		Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
		ColorMask RGB

		Fog { Mode Off }
        LOD 200
	
        Pass
        {				
            CGPROGRAM
            #pragma vertex FirstPersonVert
            #pragma fragment FirstPersonFrag
			#pragma multi_compile CUBE CYLINDER
			#pragma multi_compile EXP LINEAR
			#pragma multi_compile SMOOTH_FADE LINEAR_FADE
            #include "VolumetricFog.cginc"
            //#pragma target 3.0
            ENDCG
        }
	} 
}
