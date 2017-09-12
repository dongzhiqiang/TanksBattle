// Shader created with Shader Forge v1.25 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.25;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:True,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:33481,y:33826,varname:node_4013,prsc:2|emission-9512-OUT;n:type:ShaderForge.SFN_Tex2d,id:127,x:30293,y:33313,ptovrint:False,ptlb:01,ptin:_01,varname:node_127,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-2281-UVOUT;n:type:ShaderForge.SFN_Panner,id:2281,x:30086,y:33289,varname:node_2281,prsc:2,spu:0.1,spv:0|UVIN-3251-UVOUT,DIST-6792-OUT;n:type:ShaderForge.SFN_TexCoord,id:3251,x:29754,y:33129,varname:node_3251,prsc:2,uv:0;n:type:ShaderForge.SFN_Multiply,id:9737,x:32793,y:33540,varname:node_9737,prsc:2|A-2144-OUT,B-6806-OUT,C-1809-RGB,D-2597-RGB;n:type:ShaderForge.SFN_ValueProperty,id:6806,x:32414,y:33515,ptovrint:False,ptlb:beishu,ptin:_beishu,varname:node_6806,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Time,id:6064,x:29501,y:33461,varname:node_6064,prsc:2;n:type:ShaderForge.SFN_Multiply,id:6792,x:29813,y:33447,varname:node_6792,prsc:2|A-2274-OUT,B-6064-T;n:type:ShaderForge.SFN_ValueProperty,id:2274,x:29501,y:33346,ptovrint:False,ptlb:01T,ptin:_01T,varname:node_2274,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Tex2d,id:15,x:30249,y:34027,ptovrint:False,ptlb:02,ptin:_02,varname:_02,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-2567-UVOUT;n:type:ShaderForge.SFN_Panner,id:2567,x:30006,y:33929,varname:node_2567,prsc:2,spu:0.1,spv:0|UVIN-3821-UVOUT,DIST-8822-OUT;n:type:ShaderForge.SFN_TexCoord,id:3821,x:29769,y:33929,varname:node_3821,prsc:2,uv:0;n:type:ShaderForge.SFN_Time,id:9790,x:29457,y:34175,varname:node_9790,prsc:2;n:type:ShaderForge.SFN_Multiply,id:8822,x:29769,y:34161,varname:node_8822,prsc:2|A-8886-OUT,B-9790-T;n:type:ShaderForge.SFN_ValueProperty,id:8886,x:29457,y:34065,ptovrint:False,ptlb:02T,ptin:_02T,varname:_01shijian_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:1224,x:30867,y:34041,varname:node_1224,prsc:2|A-15-RGB,B-1918-OUT;n:type:ShaderForge.SFN_Multiply,id:7013,x:30860,y:33355,varname:node_7013,prsc:2|A-127-RGB,B-4856-OUT;n:type:ShaderForge.SFN_Color,id:1809,x:32414,y:33672,ptovrint:False,ptlb:node_1809,ptin:_node_1809,varname:node_1809,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:4856,x:30381,y:33595,ptovrint:False,ptlb:01B,ptin:_01B,varname:node_4856,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:1918,x:30397,y:34173,ptovrint:False,ptlb:02B,ptin:_02B,varname:_01B_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Add,id:7798,x:32435,y:34235,varname:node_7798,prsc:2|A-127-R,B-15-R;n:type:ShaderForge.SFN_Multiply,id:6332,x:32778,y:34375,varname:node_6332,prsc:2|A-7798-OUT,B-8151-R,C-6860-OUT;n:type:ShaderForge.SFN_Tex2d,id:8151,x:32387,y:34564,ptovrint:False,ptlb:mask,ptin:_mask,varname:node_8151,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_ValueProperty,id:6860,x:32399,y:34838,ptovrint:False,ptlb:maskA,ptin:_maskA,varname:node_6860,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Color,id:9038,x:30871,y:33604,ptovrint:False,ptlb:01C,ptin:_01C,varname:node_9038,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:1051,x:31387,y:33314,varname:node_1051,prsc:2|A-7013-OUT,B-9038-RGB;n:type:ShaderForge.SFN_Color,id:5217,x:31082,y:34251,ptovrint:False,ptlb:02C,ptin:_02C,varname:_01C_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:8518,x:31423,y:34047,varname:node_8518,prsc:2|A-1224-OUT,B-5217-RGB;n:type:ShaderForge.SFN_VertexColor,id:2597,x:32416,y:33939,varname:node_2597,prsc:2;n:type:ShaderForge.SFN_Multiply,id:2144,x:32413,y:33286,varname:node_2144,prsc:2|A-1051-OUT,B-8518-OUT;n:type:ShaderForge.SFN_Multiply,id:9512,x:33171,y:33836,varname:node_9512,prsc:2|A-9737-OUT,B-6332-OUT;proporder:127-6806-2274-15-8886-1809-4856-1918-8151-6860-9038-5217;pass:END;sub:END;*/

Shader "Shader Forge/QZ_2U_Mu_mask" {
    Properties {
        _01 ("01", 2D) = "white" {}
        _beishu ("beishu", Float ) = 1
        _01T ("01T", Float ) = 1
        _02 ("02", 2D) = "white" {}
        _02T ("02T", Float ) = 1
        _node_1809 ("node_1809", Color) = (0.5,0.5,0.5,1)
        _01B ("01B", Float ) = 0
        _02B ("02B", Float ) = 0
        _mask ("mask", 2D) = "white" {}
        _maskA ("maskA", Float ) = 1
        _01C ("01C", Color) = (0.5,0.5,0.5,1)
        _02C ("02C", Color) = (0.5,0.5,0.5,1)
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
            #pragma target 2.0
            uniform float4 _TimeEditor;
            uniform sampler2D _01; uniform float4 _01_ST;
            uniform float _beishu;
            uniform float _01T;
            uniform sampler2D _02; uniform float4 _02_ST;
            uniform float _02T;
            uniform float4 _node_1809;
            uniform float _01B;
            uniform float _02B;
            uniform sampler2D _mask; uniform float4 _mask_ST;
            uniform float _maskA;
            uniform float4 _01C;
            uniform float4 _02C;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 node_6064 = _Time + _TimeEditor;
                float2 node_2281 = (i.uv0+(_01T*node_6064.g)*float2(0.1,0));
                float4 _01_var = tex2D(_01,TRANSFORM_TEX(node_2281, _01));
                float4 node_9790 = _Time + _TimeEditor;
                float2 node_2567 = (i.uv0+(_02T*node_9790.g)*float2(0.1,0));
                float4 _02_var = tex2D(_02,TRANSFORM_TEX(node_2567, _02));
                float4 _mask_var = tex2D(_mask,TRANSFORM_TEX(i.uv0, _mask));
                float3 emissive = (((((_01_var.rgb*_01B)*_01C.rgb)*((_02_var.rgb*_02B)*_02C.rgb))*_beishu*_node_1809.rgb*i.vertexColor.rgb)*((_01_var.r+_02_var.r)*_mask_var.r*_maskA));
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
