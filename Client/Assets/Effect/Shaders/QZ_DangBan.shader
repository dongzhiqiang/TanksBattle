// Shader created with Shader Forge v1.25 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.25;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:True,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:33481,y:33826,varname:node_4013,prsc:2|emission-9737-OUT,alpha-6332-OUT;n:type:ShaderForge.SFN_Tex2d,id:127,x:30326,y:33268,ptovrint:False,ptlb:01,ptin:_01,varname:node_127,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-2281-UVOUT;n:type:ShaderForge.SFN_Panner,id:2281,x:30119,y:33244,varname:node_2281,prsc:2,spu:0.1,spv:0|UVIN-3251-UVOUT,DIST-6792-OUT;n:type:ShaderForge.SFN_TexCoord,id:3251,x:29787,y:33084,varname:node_3251,prsc:2,uv:0;n:type:ShaderForge.SFN_Multiply,id:9737,x:32793,y:33540,varname:node_9737,prsc:2|A-448-OUT,B-6806-OUT,C-1809-RGB;n:type:ShaderForge.SFN_ValueProperty,id:6806,x:32453,y:33619,ptovrint:False,ptlb:beishu,ptin:_beishu,varname:node_6806,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Time,id:6064,x:29534,y:33416,varname:node_6064,prsc:2;n:type:ShaderForge.SFN_Multiply,id:6792,x:29846,y:33402,varname:node_6792,prsc:2|A-2274-OUT,B-6064-T;n:type:ShaderForge.SFN_ValueProperty,id:2274,x:29534,y:33301,ptovrint:False,ptlb:01T,ptin:_01T,varname:node_2274,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Tex2d,id:15,x:30282,y:33982,ptovrint:False,ptlb:02,ptin:_02,varname:_02,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-2567-UVOUT;n:type:ShaderForge.SFN_Panner,id:2567,x:30039,y:33884,varname:node_2567,prsc:2,spu:0.1,spv:0|UVIN-3821-UVOUT,DIST-8822-OUT;n:type:ShaderForge.SFN_TexCoord,id:3821,x:29802,y:33884,varname:node_3821,prsc:2,uv:0;n:type:ShaderForge.SFN_Time,id:9790,x:29490,y:34130,varname:node_9790,prsc:2;n:type:ShaderForge.SFN_Multiply,id:8822,x:29802,y:34116,varname:node_8822,prsc:2|A-8886-OUT,B-9790-T;n:type:ShaderForge.SFN_ValueProperty,id:8886,x:29490,y:34020,ptovrint:False,ptlb:02T,ptin:_02T,varname:_01shijian_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Tex2d,id:132,x:30272,y:34626,ptovrint:False,ptlb:03,ptin:_03,varname:_03,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-8030-UVOUT;n:type:ShaderForge.SFN_Panner,id:8030,x:30029,y:34528,varname:node_8030,prsc:2,spu:0.1,spv:0|UVIN-9512-UVOUT,DIST-4106-OUT;n:type:ShaderForge.SFN_TexCoord,id:9512,x:29792,y:34528,varname:node_9512,prsc:2,uv:0;n:type:ShaderForge.SFN_Time,id:8601,x:29480,y:34774,varname:node_8601,prsc:2;n:type:ShaderForge.SFN_Multiply,id:4106,x:29792,y:34760,varname:node_4106,prsc:2|A-765-OUT,B-8601-T;n:type:ShaderForge.SFN_ValueProperty,id:765,x:29480,y:34664,ptovrint:False,ptlb:03T,ptin:_03T,varname:_02T_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Add,id:448,x:32424,y:33310,varname:node_448,prsc:2|A-1051-OUT,B-8518-OUT,C-1505-OUT;n:type:ShaderForge.SFN_Multiply,id:1513,x:30921,y:34629,varname:node_1513,prsc:2|A-132-R,B-8605-OUT;n:type:ShaderForge.SFN_Multiply,id:1224,x:30900,y:33996,varname:node_1224,prsc:2|A-15-R,B-1918-OUT;n:type:ShaderForge.SFN_Multiply,id:7013,x:30893,y:33310,varname:node_7013,prsc:2|A-127-R,B-4856-OUT;n:type:ShaderForge.SFN_Color,id:1809,x:32476,y:33810,ptovrint:False,ptlb:node_1809,ptin:_node_1809,varname:node_1809,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:4856,x:30414,y:33550,ptovrint:False,ptlb:01B,ptin:_01B,varname:node_4856,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:1918,x:30430,y:34128,ptovrint:False,ptlb:02B,ptin:_02B,varname:_01B_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:8605,x:30424,y:34796,ptovrint:False,ptlb:03B,ptin:_03B,varname:_02B_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Add,id:7798,x:32435,y:34235,varname:node_7798,prsc:2|A-127-R,B-15-R,C-132-R;n:type:ShaderForge.SFN_Multiply,id:6332,x:32778,y:34375,varname:node_6332,prsc:2|A-7798-OUT,B-8151-R,C-6860-OUT;n:type:ShaderForge.SFN_Tex2d,id:8151,x:32387,y:34564,ptovrint:False,ptlb:mask,ptin:_mask,varname:node_8151,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_ValueProperty,id:6860,x:32399,y:34838,ptovrint:False,ptlb:maskA,ptin:_maskA,varname:node_6860,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Color,id:9038,x:30904,y:33559,ptovrint:False,ptlb:01C,ptin:_01C,varname:node_9038,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:1051,x:31422,y:33309,varname:node_1051,prsc:2|A-7013-OUT,B-9038-RGB;n:type:ShaderForge.SFN_Color,id:5217,x:31115,y:34206,ptovrint:False,ptlb:02C,ptin:_02C,varname:_01C_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:8518,x:31456,y:34002,varname:node_8518,prsc:2|A-1224-OUT,B-5217-RGB;n:type:ShaderForge.SFN_Color,id:3404,x:31129,y:34810,ptovrint:False,ptlb:03C,ptin:_03C,varname:_01C_copy_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:1505,x:31464,y:34633,varname:node_1505,prsc:2|A-1513-OUT,B-3404-RGB;proporder:127-6806-2274-15-8886-132-765-1809-4856-1918-8605-8151-6860-9038-5217-3404;pass:END;sub:END;*/

Shader "Shader Forge/QZ_DangBan" {
    Properties {
        _01 ("01", 2D) = "white" {}
        _beishu ("beishu", Float ) = 1
        _01T ("01T", Float ) = 1
        _02 ("02", 2D) = "white" {}
        _02T ("02T", Float ) = 1
        _03 ("03", 2D) = "white" {}
        _03T ("03T", Float ) = 1
        _node_1809 ("node_1809", Color) = (0.5,0.5,0.5,1)
        _01B ("01B", Float ) = 0
        _02B ("02B", Float ) = 0
        _03B ("03B", Float ) = 0
        _mask ("mask", 2D) = "white" {}
        _maskA ("maskA", Float ) = 1
        _01C ("01C", Color) = (0.5,0.5,0.5,1)
        _02C ("02C", Color) = (0.5,0.5,0.5,1)
        _03C ("03C", Color) = (0.5,0.5,0.5,1)
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
            uniform sampler2D _01; uniform float4 _01_ST;
            uniform float _beishu;
            uniform float _01T;
            uniform sampler2D _02; uniform float4 _02_ST;
            uniform float _02T;
            uniform sampler2D _03; uniform float4 _03_ST;
            uniform float _03T;
            uniform float4 _node_1809;
            uniform float _01B;
            uniform float _02B;
            uniform float _03B;
            uniform sampler2D _mask; uniform float4 _mask_ST;
            uniform float _maskA;
            uniform float4 _01C;
            uniform float4 _02C;
            uniform float4 _03C;
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
                float4 node_6064 = _Time + _TimeEditor;
                float2 node_2281 = (i.uv0+(_01T*node_6064.g)*float2(0.1,0));
                float4 _01_var = tex2D(_01,TRANSFORM_TEX(node_2281, _01));
                float4 node_9790 = _Time + _TimeEditor;
                float2 node_2567 = (i.uv0+(_02T*node_9790.g)*float2(0.1,0));
                float4 _02_var = tex2D(_02,TRANSFORM_TEX(node_2567, _02));
                float4 node_8601 = _Time + _TimeEditor;
                float2 node_8030 = (i.uv0+(_03T*node_8601.g)*float2(0.1,0));
                float4 _03_var = tex2D(_03,TRANSFORM_TEX(node_8030, _03));
                float3 emissive = ((((_01_var.r*_01B)*_01C.rgb)+((_02_var.r*_02B)*_02C.rgb)+((_03_var.r*_03B)*_03C.rgb))*_beishu*_node_1809.rgb);
                float3 finalColor = emissive;
                float4 _mask_var = tex2D(_mask,TRANSFORM_TEX(i.uv0, _mask));
                fixed4 finalRGBA = fixed4(finalColor,((_01_var.r+_02_var.r+_03_var.r)*_mask_var.r*_maskA));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
