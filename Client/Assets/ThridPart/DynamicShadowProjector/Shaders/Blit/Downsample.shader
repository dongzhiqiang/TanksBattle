// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "DynamicShadowProjector/Blit/Downsample" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
    CGINCLUDE
	#include "UnityCG.cginc"
	#pragma target 3.0

	sampler2D _MainTex;
	half4 _MainTex_TexelSize;
	fixed4 _Color;

	struct v2f_blit
	{
		float4 pos : SV_POSITION;
		half2  uv0 : TEXCOORD0;
	};
	struct v2f_downsample
	{
		float4 pos : SV_POSITION;
		half2  uv0 : TEXCOORD0;
		half2  uv1 : TEXCOORD1;
		half2  uv2 : TEXCOORD2;
		half2  uv3 : TEXCOORD3;
	};

	v2f_blit vert_blit(appdata_img v)
	{
		v2f_blit o;
		o.pos = UnityObjectToClipPos(v.vertex);
    	o.uv0 = v.texcoord.xy;
		return o;
	}

	fixed4 frag_blit(v2f_blit i) : COLOR
	{
		return tex2D(_MainTex, i.uv0);
	}

	fixed4 frag_blit_withShadowColor(v2f_blit i) : COLOR
	{
		return lerp(fixed4(1,1,1,0), _Color, tex2D(_MainTex, i.uv0).a);
	}

	v2f_downsample vert_downsample(appdata_img v)
	{
		v2f_downsample o;
		o.pos = UnityObjectToClipPos(v.vertex);
    	o.uv0 = v.texcoord.xy + _MainTex_TexelSize.xy;
		o.uv1 = v.texcoord.xy - _MainTex_TexelSize.xy;
		o.uv2 = v.texcoord.xy + _MainTex_TexelSize.xy * half2(1,-1);
		o.uv3 = v.texcoord.xy + _MainTex_TexelSize.xy * half2(-1,1);
		return o;
	}

	fixed4 frag_downsample(v2f_downsample i) : COLOR
	{
		fixed4 color = 0.25*tex2D(_MainTex, i.uv0);
		color += 0.25*tex2D(_MainTex, i.uv1);
		color += 0.25*tex2D(_MainTex, i.uv2);
		color += 0.25*tex2D(_MainTex, i.uv3);
		return color;
	}

	fixed4 frag_downsample_withShadowColor(v2f_downsample i) : COLOR
	{
		fixed a = 0.25*tex2D(_MainTex, i.uv0).a;
		a += 0.25*tex2D(_MainTex, i.uv1).a;
		a += 0.25*tex2D(_MainTex, i.uv2).a;
		a += 0.25*tex2D(_MainTex, i.uv3).a;
		return lerp(fixed4(1,1,1,0), _Color, a);
	}

#define DSP_OFFSET_TYPE4 fixed4
#define DSP_OFFSET_TYPE2 fixed2
	DSP_OFFSET_TYPE4 _Offset0;
	DSP_OFFSET_TYPE4 _Offset1;
	DSP_OFFSET_TYPE4 _Offset2;
	DSP_OFFSET_TYPE4 _Offset3;
	fixed4 _Weight;

	fixed4 frag_downsample_with_blur(v2f_blit i) : COLOR
	{
		DSP_OFFSET_TYPE2 uv = i.uv0 + _Offset0.xy;
		fixed4 c0 = tex2D(_MainTex, uv);
		uv = i.uv0 + _Offset1.xy;
		fixed4 c1 = tex2D(_MainTex, uv);
		uv = i.uv0 + _Offset2.xy;
		fixed4 c2 = tex2D(_MainTex, uv);
		uv = i.uv0 + _Offset3.xy;
		fixed4 c3 = tex2D(_MainTex, uv);

		uv = i.uv0 + _Offset0.xw;
		fixed4 color = _Weight.x * c0;
		c0 = tex2D(_MainTex, uv);
		uv = i.uv0 + _Offset1.xw;
		color += _Weight.y * c1;
		c1 = tex2D(_MainTex, uv);
		uv = i.uv0 + _Offset2.xw;
		color += _Weight.z * c2;
		c2 = tex2D(_MainTex, uv);
		uv = i.uv0 + _Offset3.xw;
		color += _Weight.w * c3;
		c3 = tex2D(_MainTex, uv);

		uv = i.uv0 + _Offset0.zy;
		color += _Weight.x * c0;
		c0 = tex2D(_MainTex, uv);
		uv = i.uv0 + _Offset1.zy;
		color += _Weight.y * c1;
		c1 = tex2D(_MainTex, uv);
		uv = i.uv0 + _Offset2.zy;
		color += _Weight.z * c2;
		c2 = tex2D(_MainTex, uv);
		uv = i.uv0 + _Offset3.zy;
		color += _Weight.w * c3;
		c3 = tex2D(_MainTex, uv);

		uv = i.uv0 + _Offset0.zw;
		color += _Weight.x * c0;
		c0 = tex2D(_MainTex, uv);
		uv = i.uv0 + _Offset1.zw;
		color += _Weight.y * c1;
		c1 = tex2D(_MainTex, uv);
		uv = i.uv0 + _Offset2.zw;
		color += _Weight.z * c2;
		c2 = tex2D(_MainTex, uv);
		uv = i.uv0 + _Offset3.zw;
		color += _Weight.w * c3;
		c3 = tex2D(_MainTex, uv);

		color += _Weight.x * c0;
		color += _Weight.y * c1;
		color += _Weight.z * c2;
		color += _Weight.w * c3;

		return color;
	}

	fixed4 frag_downsample_with_blur_lod(v2f_blit i) : COLOR
	{
		fixed4 uv;
		uv.xy = i.uv0 + _Offset0.xy;
		uv.zw = 0;
		fixed4 c0 = tex2Dlod(_MainTex, uv);
		uv.xy = i.uv0 + _Offset1.xy;
		fixed4 c1 = tex2Dlod(_MainTex, uv);
		uv.xy = i.uv0 + _Offset2.xy;
		fixed4 c2 = tex2Dlod(_MainTex, uv);
		uv.xy = i.uv0 + _Offset3.xy;
		fixed4 c3 = tex2Dlod(_MainTex, uv);

		uv.xy = i.uv0 + _Offset0.xw;
		fixed4 color = _Weight.x * c0;
		c0 = tex2Dlod(_MainTex, uv);
		uv.xy = i.uv0 + _Offset1.xw;
		color += _Weight.y * c1;
		c1 = tex2Dlod(_MainTex, uv);
		uv.xy = i.uv0 + _Offset2.xw;
		color += _Weight.z * c2;
		c2 = tex2Dlod(_MainTex, uv);
		uv.xy = i.uv0 + _Offset3.xw;
		color += _Weight.w * c3;
		c3 = tex2D(_MainTex, uv);

		uv.xy = i.uv0 + _Offset0.zy;
		color += _Weight.x * c0;
		c0 = tex2D(_MainTex, uv);
		uv.xy = i.uv0 + _Offset1.zy;
		color += _Weight.y * c1;
		c1 = tex2D(_MainTex, uv);
		uv.xy = i.uv0 + _Offset2.zy;
		color += _Weight.z * c2;
		c2 = tex2Dlod(_MainTex, uv);
		uv.xy = i.uv0 + _Offset3.zy;
		color += _Weight.w * c3;
		c3 = tex2Dlod(_MainTex, uv);

		uv.xy = i.uv0 + _Offset0.zw;
		color += _Weight.x * c0;
		c0 = tex2Dlod(_MainTex, uv);
		uv.xy = i.uv0 + _Offset1.zw;
		color += _Weight.y * c1;
		c1 = tex2Dlod(_MainTex, uv);
		uv.xy = i.uv0 + _Offset2.zw;
		color += _Weight.z * c2;
		c2 = tex2Dlod(_MainTex, uv);
		uv.xy = i.uv0 + _Offset3.zw;
		color += _Weight.w * c3;
		c3 = tex2Dlod(_MainTex, uv);

		color += _Weight.x * c0;
		color += _Weight.y * c1;
		color += _Weight.z * c2;
		color += _Weight.w * c3;

		return color;
	}

	ENDCG

	SubShader {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode Off }
		// pass 0: downsample
		Pass {
			CGPROGRAM
			#pragma vertex vert_downsample
			#pragma fragment frag_downsample
			ENDCG
		}
		// pass 1: downsample with shadow color
		Pass {
			CGPROGRAM
			#pragma vertex vert_downsample
			#pragma fragment frag_downsample_withShadowColor
			ENDCG
		}
		// pass 2: blit
		Pass {
			CGPROGRAM
			#pragma vertex vert_blit
			#pragma fragment frag_blit
			ENDCG
		}
		// pass 3: blit with shadow color
		Pass {
			CGPROGRAM
			#pragma vertex vert_blit
			#pragma fragment frag_blit_withShadowColor
			ENDCG
		}
		// pass 4: downsample with blur
		Pass {
			CGPROGRAM
			#pragma vertex vert_blit
			#pragma fragment frag_downsample_with_blur
			ENDCG
		}
		// pass 5: downsample with blur for mipmap
		Pass {
			CGPROGRAM
			#pragma vertex vert_blit
			#pragma fragment frag_downsample_with_blur_lod
			ENDCG
		}
	}
}
