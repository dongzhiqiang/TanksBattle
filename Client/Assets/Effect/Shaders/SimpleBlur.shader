// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/SimpleBlur" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader
{
	Pass
	{
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }

CGPROGRAM
// Upgrade NOTE: excluded shader from Xbox360 because it uses wrong array syntax (type[size] name)
//#pragma exclude_renderers xbox360
//#pragma target 3.0
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest 

#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform half _SampleDist;




struct v2f {
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
};

v2f vert (appdata_img v)
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = v.texcoord.xy;
	return o;
}

fixed4 frag (v2f i) : COLOR
{
	half2 texCoord = i.uv;
	fixed4 color = tex2D(_MainTex, texCoord);
	fixed4 sum = color; 
	half dis =_SampleDist*0.01;
	sum += tex2D(_MainTex, half2(texCoord.x + dis , texCoord.y + dis )) +
		tex2D(_MainTex, half2(texCoord.x + dis , texCoord.y)) +
		tex2D(_MainTex, half2(texCoord.x , texCoord.y + dis )) +
		tex2D(_MainTex, half2(texCoord.x - dis , texCoord.y - dis )) +
		tex2D(_MainTex, half2(texCoord.x + dis , texCoord.y - dis )) +
		tex2D(_MainTex, half2(texCoord.x - dis , texCoord.y + dis )) +
		tex2D(_MainTex, half2(texCoord.x - dis , texCoord.y)) +
		tex2D(_MainTex, half2(texCoord.x , texCoord.y - dis )) ;
	sum /=  9;
	return sum;
}
ENDCG

	}
}

Fallback off

}