// Shader created with Shader Forge v1.25 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.25;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:32719,y:32712,varname:node_4013,prsc:2|emission-3501-OUT;n:type:ShaderForge.SFN_Tex2d,id:6220,x:31263,y:32512,ptovrint:False,ptlb:01,ptin:_01,varname:node_6220,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-7659-UVOUT;n:type:ShaderForge.SFN_Tex2d,id:2531,x:31792,y:32998,ptovrint:False,ptlb:mask,ptin:_mask,varname:node_2531,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:9353,x:30930,y:32801,ptovrint:False,ptlb:02,ptin:_02,varname:_02,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-8588-UVOUT;n:type:ShaderForge.SFN_Multiply,id:3220,x:32206,y:32649,varname:node_3220,prsc:2|A-1007-OUT,B-3777-OUT,C-1747-OUT;n:type:ShaderForge.SFN_Multiply,id:3501,x:32498,y:32732,varname:node_3501,prsc:2|A-3220-OUT,B-2531-RGB,C-4896-OUT,D-7246-RGB;n:type:ShaderForge.SFN_Panner,id:7659,x:30984,y:32514,varname:node_7659,prsc:2,spu:-0.1,spv:0.01|UVIN-8799-UVOUT,DIST-6931-OUT;n:type:ShaderForge.SFN_TexCoord,id:8799,x:30778,y:32514,varname:node_8799,prsc:2,uv:0;n:type:ShaderForge.SFN_Panner,id:8588,x:30729,y:32801,varname:node_8588,prsc:2,spu:-0.1,spv:-0.02|UVIN-6814-UVOUT,DIST-6845-OUT;n:type:ShaderForge.SFN_TexCoord,id:6814,x:30522,y:32801,varname:node_6814,prsc:2,uv:0;n:type:ShaderForge.SFN_Time,id:7869,x:30070,y:32961,varname:node_7869,prsc:2;n:type:ShaderForge.SFN_Multiply,id:6845,x:30342,y:33000,varname:node_6845,prsc:2|A-7869-T,B-8759-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8759,x:30070,y:33167,ptovrint:False,ptlb:T02,ptin:_T02,varname:node_8759,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Time,id:5741,x:30274,y:32528,varname:node_5741,prsc:2;n:type:ShaderForge.SFN_Multiply,id:6931,x:30546,y:32567,varname:node_6931,prsc:2|A-5741-T,B-2793-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2793,x:30251,y:32760,ptovrint:False,ptlb:T01,ptin:_T01,varname:_T03,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:4896,x:32192,y:33006,ptovrint:False,ptlb:value,ptin:_value,varname:node_4896,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Color,id:7246,x:32206,y:33128,ptovrint:False,ptlb:color,ptin:_color,varname:node_7246,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Add,id:1007,x:31835,y:32351,varname:node_1007,prsc:2|A-2531-RGB,B-6220-RGB;n:type:ShaderForge.SFN_Multiply,id:3777,x:31200,y:33026,varname:node_3777,prsc:2|A-9353-RGB,B-6598-RGB;n:type:ShaderForge.SFN_Tex2d,id:6598,x:30904,y:33124,ptovrint:False,ptlb:02x,ptin:_02x,varname:node_6598,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-8039-UVOUT;n:type:ShaderForge.SFN_Panner,id:8039,x:30630,y:33246,varname:node_8039,prsc:2,spu:-0.1,spv:0.01|UVIN-2593-UVOUT,DIST-659-OUT;n:type:ShaderForge.SFN_TexCoord,id:2593,x:30423,y:33246,varname:node_2593,prsc:2,uv:0;n:type:ShaderForge.SFN_Time,id:7985,x:29971,y:33406,varname:node_7985,prsc:2;n:type:ShaderForge.SFN_Multiply,id:659,x:30243,y:33445,varname:node_659,prsc:2|A-7985-T,B-2034-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2034,x:29892,y:33592,ptovrint:False,ptlb:T02x,ptin:_T02x,varname:node_2034,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:1747,x:31860,y:32802,ptovrint:False,ptlb:value2,ptin:_value2,varname:node_1747,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;proporder:6220-9353-6598-2531-2793-8759-2034-4896-1747-7246;pass:END;sub:END;*/

Shader "Shader Forge/QZ_liuguang" {
    Properties {
        _01 ("01", 2D) = "white" {}
        _02 ("02", 2D) = "white" {}
        _02x ("02x", 2D) = "white" {}
        _mask ("mask", 2D) = "white" {}
        _T01 ("T01", Float ) = 0
        _T02 ("T02", Float ) = 0
        _T02x ("T02x", Float ) = 0
        _value ("value", Float ) = 0
        _value2 ("value2", Float ) = 0
        _color ("color", Color) = (0.5,0.5,0.5,1)
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
            Blend One One
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
            uniform sampler2D _01; uniform float4 _01_ST;
            uniform sampler2D _mask; uniform float4 _mask_ST;
            uniform sampler2D _02; uniform float4 _02_ST;
            uniform float _T02;
            uniform float _T01;
            uniform float _value;
            uniform float4 _color;
            uniform sampler2D _02x; uniform float4 _02x_ST;
            uniform float _T02x;
            uniform float _value2;
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
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 _mask_var = tex2D(_mask,TRANSFORM_TEX(i.uv0, _mask));
                float4 node_5741 = _Time + _TimeEditor;
                float2 node_7659 = (i.uv0+(node_5741.g*_T01)*float2(-0.1,0.01));
                float4 _01_var = tex2D(_01,TRANSFORM_TEX(node_7659, _01));
                float4 node_7869 = _Time + _TimeEditor;
                float2 node_8588 = (i.uv0+(node_7869.g*_T02)*float2(-0.1,-0.02));
                float4 _02_var = tex2D(_02,TRANSFORM_TEX(node_8588, _02));
                float4 node_7985 = _Time + _TimeEditor;
                float2 node_8039 = (i.uv0+(node_7985.g*_T02x)*float2(-0.1,0.01));
                float4 _02x_var = tex2D(_02x,TRANSFORM_TEX(node_8039, _02x));
                float3 emissive = (((_mask_var.rgb+_01_var.rgb)*(_02_var.rgb*_02x_var.rgb)*_value2)*_mask_var.rgb*_value*_color.rgb);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
