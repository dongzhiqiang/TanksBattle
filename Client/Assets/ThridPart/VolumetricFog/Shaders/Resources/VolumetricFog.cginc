// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#ifndef Volumetric_Fog_Include
#define Volumetric_Fog_Include

/////////////////Includes//////////////////////

#include "UnityCG.cginc"

//////////////////////////////////////////////////

/////////////////Properties//////////////////////

sampler2D _CameraDepthTexture;
float4 _Color;

float _Visibility;
float _Start;
float _End;
float4 _Size;

float4 _Center;

//////////////////////////////////////////////////

/////////////////////Types///////////////////////

struct A2V
{
	float4 vertex : POSITION;
};

struct FirstPersonV2F
{
	float4 vPos         : SV_POSITION;
	float4 vScreenPos   : TEXCOORD0;
	float3 vLocalPos    : TEXCOORD1;
	float3 vViewPos     : TEXCOORD2;
	float3 vLocalCamPos : TEXCOORD3;
};

struct ThirdPersonV2F
{
	float4 vPos         : SV_POSITION;
	float4 vScreenPos   : TEXCOORD0;
	float3 vLocalPos    : TEXCOORD1;
	float3 vViewPos     : TEXCOORD2;
	float3 vLocalCamPos : TEXCOORD3;
	float3 vLocalCenter : TEXCOORD4;
};

//////////////////////////////////////////////////

/////////////////////Function////////////////////

FirstPersonV2F FirstPersonVert (A2V IN)
{
	FirstPersonV2F OUT;

	OUT.vLocalPos = IN.vertex.xyz;
	OUT.vViewPos = mul((float4x4)UNITY_MATRIX_MV, float4(IN.vertex.xyz, 1.0f)).xyz;
	OUT.vLocalCamPos = mul((float4x4)unity_WorldToObject, (float4(_WorldSpaceCameraPos, 1.0f))).xyz;
	OUT.vPos = UnityObjectToClipPos(IN.vertex);
	OUT.vScreenPos = ComputeScreenPos(OUT.vPos);

	return OUT;
}

float4 FirstPersonFrag (FirstPersonV2F IN) : COLOR
{
	float2 screenUV = IN.vScreenPos.xy / IN.vScreenPos.w;
   
	float depthFragment = DECODE_EYEDEPTH(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));	
	float3 viewEyeDirection = normalize(IN.vViewPos);
	float scaleFactor = (depthFragment / viewEyeDirection.z);
	depthFragment = length(viewEyeDirection * scaleFactor);

#if EXP
	float intensity = 1-saturate(1/exp(pow(depthFragment/_Visibility,2)));
#else // LINEAR
	float intensity = saturate(depthFragment / _Visibility);
#endif
				
	float3 dir = IN.vLocalPos - IN.vLocalCamPos;
	float localDepth = length(dir);
	float3 depthPos = IN.vLocalCamPos + normalize(dir) * depthFragment;
	float diff = step(0,depthFragment - localDepth);
	float height = (1-diff) * depthPos.y + diff * IN.vLocalPos.y;
#if SMOOTH_FADE
	float lerp = 1 - smoothstep(_Start, _End, clamp(height, _Start, _End));
#else // LINEAR_FADE 				
	float lerp = 1- (clamp(height,_Start,_End) - _Start) / (_End -_Start);				
#endif
	intensity *= lerp;

#if CUBE			
	float d1 = abs(depthPos.x), d2 = abs(depthPos.y), d3 = abs(depthPos.z);
	float filter = diff + step(d1,_Size.x) * step(d2,_Size.y)* step(d3,_Size.z);
	intensity *= filter;
#else //CYLINDER
	float d1 = 1 - step(1,pow(depthPos.x/_Size.x,2) + pow(depthPos.z/_Size.z,2));
	float d2 = _Size.y - abs(depthPos.y);
	float filter = diff + d1 * step(0,d2);
	intensity *= filter;
#endif		
	return float4(_Color.rgb, intensity * _Color.a);
}


ThirdPersonV2F ThirdPersonVert (appdata_full IN)
{
	ThirdPersonV2F OUT;

	OUT.vLocalPos = IN.vertex.xyz;
	OUT.vViewPos = mul((float4x4)UNITY_MATRIX_MV, float4(IN.vertex.xyz, 1.0f)).xyz;
	OUT.vLocalCamPos = mul((float4x4)unity_WorldToObject, (float4(_WorldSpaceCameraPos, 1.0f))).xyz;
	OUT.vPos = UnityObjectToClipPos(IN.vertex);
	OUT.vScreenPos = ComputeScreenPos(OUT.vPos);
	OUT.vLocalCenter = mul((float4x4)unity_WorldToObject, (float4(_Center.xyz, 1.0f))).xyz;
	return OUT;
}

float4 ThirdPersonFrag (ThirdPersonV2F IN) : COLOR
{
	float2 screenUV = IN.vScreenPos.xy / IN.vScreenPos.w;
      
	float depthFragment = DECODE_EYEDEPTH(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));	
	float3 viewEyeDirection = normalize(IN.vViewPos);
	float scaleFactor = (depthFragment / viewEyeDirection.z);
	depthFragment = length(viewEyeDirection * scaleFactor);

	float3 dir = IN.vLocalPos - IN.vLocalCamPos;
	float3 depthPos = IN.vLocalCamPos + normalize(dir) * depthFragment;
	float dis = length(depthPos - IN.vLocalCenter.xyz);
#if EXP
	float intensity = 1-saturate(1/exp(pow(dis/_Visibility,2)));
#else // LINEAR
	float intensity = saturate(dis / _Visibility);
#endif
	float localDepth = length(dir);
	float diff = step(0,depthFragment - localDepth);
	float height = (1-diff) * depthPos.y + diff * IN.vLocalPos.y;
#if SMOOTH_FADE
	float lerp = 1 - smoothstep(_Start, _End, clamp(height, _Start, _End));
#else // LINEAR_FADE 				
	float lerp = 1- (clamp(height,_Start,_End) - _Start) / (_End -_Start);				
#endif
	intensity *= lerp;

#if CUBE			
	float d1 = abs(depthPos.x), d2 = abs(depthPos.y), d3 = abs(depthPos.z);
	float filter = diff + step(d1,_Size.x) * step(d2,_Size.y)* step(d3,_Size.z);
	intensity *= filter;
#else //CYLINDER
	float d1 = 1 - step(1,pow(depthPos.x/_Size.x,2) + pow(depthPos.z/_Size.z,2));
	float d2 = _Size.y - abs(depthPos.y);
	float filter = diff + d1 * step(0,d2);
	intensity *= filter;
#endif		
	return float4(_Color.rgb, intensity * _Color.a);
}

//////////////////////////////////////////////////
#endif