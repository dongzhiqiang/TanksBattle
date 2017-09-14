// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.25 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.25;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:32968,y:33021,varname:node_4013,prsc:2|emission-9621-OUT,alpha-2689-OUT;n:type:ShaderForge.SFN_Step,id:4307,x:31739,y:32833,varname:node_4307,prsc:2|A-4906-OUT,B-1727-OUT;n:type:ShaderForge.SFN_Tex2d,id:7640,x:31477,y:33538,ptovrint:False,ptlb:tietu,ptin:_tietu,varname:node_7640,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:0052bc871df242b4189b8ea3629854d2,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:6136,x:31083,y:32917,ptovrint:False,ptlb:mask,ptin:_mask,varname:node_6136,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:6aaa0c3099e07ca459a327a1f3fd3b0a,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:2689,x:32192,y:33668,varname:node_2689,prsc:2|A-5239-R,B-4307-OUT;n:type:ShaderForge.SFN_Subtract,id:2789,x:32057,y:32516,varname:node_2789,prsc:2|A-6936-OUT,B-4307-OUT;n:type:ShaderForge.SFN_Step,id:6936,x:31849,y:32530,varname:node_6936,prsc:2|A-1806-OUT,B-1727-OUT;n:type:ShaderForge.SFN_Multiply,id:3851,x:32396,y:32620,varname:node_3851,prsc:2|A-2789-OUT,B-3001-RGB,C-2241-OUT,D-168-RGB;n:type:ShaderForge.SFN_Color,id:3001,x:32057,y:32678,ptovrint:False,ptlb:miaobian01,ptin:_miaobian01,varname:node_3001,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:2241,x:32073,y:32848,ptovrint:False,ptlb:mianbiao02,ptin:_mianbiao02,varname:node_2241,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:-1;n:type:ShaderForge.SFN_Add,id:9621,x:32513,y:33033,varname:node_9621,prsc:2|A-3851-OUT,B-4076-OUT;n:type:ShaderForge.SFN_Multiply,id:1727,x:31356,y:32933,varname:node_1727,prsc:2|A-9869-OUT,B-6136-R;n:type:ShaderForge.SFN_Vector1,id:1806,x:31651,y:32530,varname:node_1806,prsc:2,v1:0.6;n:type:ShaderForge.SFN_Vector1,id:4906,x:31563,y:32921,varname:node_4906,prsc:2,v1:0.598;n:type:ShaderForge.SFN_Multiply,id:4076,x:32031,y:33510,varname:node_4076,prsc:2|A-4307-OUT,B-7640-RGB;n:type:ShaderForge.SFN_Tex2d,id:5239,x:31462,y:33725,ptovrint:False,ptlb:node_5239,ptin:_node_5239,varname:node_5239,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:6d77296a444ca68448524cf3041d8afb,ntxv:0,isnm:False;n:type:ShaderForge.SFN_ValueProperty,id:9869,x:31219,y:32731,ptovrint:False,ptlb:mask_K,ptin:_mask_K,varname:node_9869,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Tex2d,id:168,x:32061,y:33256,ptovrint:False,ptlb:bianyuan,ptin:_bianyuan,varname:node_168,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-1313-UVOUT;n:type:ShaderForge.SFN_Panner,id:1313,x:31730,y:33304,varname:node_1313,prsc:2,spu:0,spv:0.1|UVIN-2245-UVOUT;n:type:ShaderForge.SFN_TexCoord,id:2245,x:31494,y:33304,varname:node_2245,prsc:2,uv:0;n:type:ShaderForge.SFN_If,id:1558,x:31826,y:33058,varname:node_1558,prsc:2|A-5174-OUT,B-1727-OUT,GT-2058-OUT,EQ-2058-OUT,LT-2058-OUT;n:type:ShaderForge.SFN_Vector1,id:5174,x:31624,y:33013,varname:node_5174,prsc:2,v1:1;n:type:ShaderForge.SFN_Vector1,id:2058,x:31624,y:33137,varname:node_2058,prsc:2,v1:0;proporder:6136-9869-3001-2241-7640-5239-168;pass:END;sub:END;*/

Shader "Shader Forge/QZ_zhuoshaoBlended" {
    Properties {
        _mask ("mask", 2D) = "white" {}
        _mask_K ("mask_K", Float ) = 0
        _miaobian01 ("miaobian01", Color) = (0.5,0.5,0.5,1)
        _mianbiao02 ("mianbiao02", Float ) = -1
        _tietu ("tietu", 2D) = "black" {}
        _node_5239 ("node_5239", 2D) = "white" {}
        _bianyuan ("bianyuan", 2D) = "white" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            uniform float4 _TimeEditor;
            uniform sampler2D _tietu; uniform float4 _tietu_ST;
            uniform sampler2D _mask; uniform float4 _mask_ST;
            uniform float4 _miaobian01;
            uniform float _mianbiao02;
            uniform sampler2D _node_5239; uniform float4 _node_5239_ST;
            uniform float _mask_K;
            uniform sampler2D _bianyuan; uniform float4 _bianyuan_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 _mask_var = tex2D(_mask,TRANSFORM_TEX(i.uv0, _mask));
                float node_1727 = (_mask_K*_mask_var.r);
                float node_4307 = step(0.598,node_1727);
                float4 node_6718 = _Time + _TimeEditor;
                float2 node_1313 = (i.uv0+node_6718.g*float2(0,0.1));
                float4 _bianyuan_var = tex2D(_bianyuan,TRANSFORM_TEX(node_1313, _bianyuan));
                float4 _tietu_var = tex2D(_tietu,TRANSFORM_TEX(i.uv0, _tietu));
                float3 emissive = (((step(0.6,node_1727)-node_4307)*_miaobian01.rgb*_mianbiao02*_bianyuan_var.rgb)+(node_4307*_tietu_var.rgb));
                float3 finalColor = emissive;
                float4 _node_5239_var = tex2D(_node_5239,TRANSFORM_TEX(i.uv0, _node_5239));
                fixed4 finalRGBA = fixed4(finalColor,(_node_5239_var.r*node_4307));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
