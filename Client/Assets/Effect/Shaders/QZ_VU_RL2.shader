// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.25 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.25;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:True,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:32719,y:32712,varname:node_4013,prsc:2|emission-6926-OUT,alpha-934-OUT;n:type:ShaderForge.SFN_Tex2d,id:9385,x:31388,y:32614,ptovrint:False,ptlb:1,ptin:_1,varname:node_9385,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:ba6858566f44acd4ab7e0f8a25a939eb,ntxv:0,isnm:False|UVIN-1835-UVOUT;n:type:ShaderForge.SFN_Panner,id:1835,x:31132,y:32590,varname:node_1835,prsc:2,spu:1,spv:0|UVIN-7348-UVOUT,DIST-2205-OUT;n:type:ShaderForge.SFN_Time,id:2042,x:30666,y:32670,varname:node_2042,prsc:2;n:type:ShaderForge.SFN_TexCoord,id:7348,x:30880,y:32510,varname:node_7348,prsc:2,uv:0;n:type:ShaderForge.SFN_Multiply,id:2205,x:30895,y:32777,varname:node_2205,prsc:2|A-2042-T,B-2590-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2590,x:30648,y:32916,ptovrint:False,ptlb:Time_1,ptin:_Time_1,varname:node_2590,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:6926,x:32288,y:32861,varname:node_6926,prsc:2|A-9117-OUT,B-967-RGB;n:type:ShaderForge.SFN_Tex2d,id:967,x:31801,y:33209,ptovrint:False,ptlb:mask,ptin:_mask,varname:node_967,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:9117,x:32160,y:32413,varname:node_9117,prsc:2|A-8147-RGB,B-9972-RGB,C-5014-OUT,D-2011-OUT;n:type:ShaderForge.SFN_Color,id:9972,x:31890,y:32418,ptovrint:False,ptlb:node_9972,ptin:_node_9972,varname:node_9972,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_VertexColor,id:8147,x:31890,y:32200,varname:node_8147,prsc:2;n:type:ShaderForge.SFN_ValueProperty,id:2011,x:31787,y:32958,ptovrint:False,ptlb:beishu1,ptin:_beishu1,varname:node_2011,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_Tex2d,id:4621,x:31353,y:33089,ptovrint:False,ptlb:2,ptin:_2,varname:_2,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:ba6858566f44acd4ab7e0f8a25a939eb,ntxv:0,isnm:False|UVIN-1190-UVOUT;n:type:ShaderForge.SFN_Panner,id:1190,x:31097,y:33065,varname:node_1190,prsc:2,spu:1,spv:0|UVIN-341-UVOUT,DIST-3910-OUT;n:type:ShaderForge.SFN_Time,id:8912,x:30631,y:33145,varname:node_8912,prsc:2;n:type:ShaderForge.SFN_TexCoord,id:341,x:30845,y:32985,varname:node_341,prsc:2,uv:0;n:type:ShaderForge.SFN_Multiply,id:3910,x:30860,y:33252,varname:node_3910,prsc:2|A-8912-T,B-8734-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8734,x:30613,y:33391,ptovrint:False,ptlb:Time_2,ptin:_Time_2,varname:_Time_2,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:5014,x:31698,y:32732,varname:node_5014,prsc:2|A-9385-RGB,B-4621-RGB;n:type:ShaderForge.SFN_Multiply,id:934,x:32529,y:33281,varname:node_934,prsc:2|A-9385-R,B-4621-R,C-967-R,D-4993-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4993,x:32094,y:33465,ptovrint:False,ptlb:node_4993,ptin:_node_4993,varname:node_4993,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;proporder:9385-2590-967-9972-2011-4621-8734-4993;pass:END;sub:END;*/

Shader "Shader Forge/QZ_VU_RL2" {
    Properties {
        _1 ("1", 2D) = "white" {}
        _Time_1 ("Time_1", Float ) = 1
        _mask ("mask", 2D) = "white" {}
        _node_9972 ("node_9972", Color) = (0.5,0.5,0.5,1)
        _beishu1 ("beishu1", Float ) = 2
        _2 ("2", 2D) = "white" {}
        _Time_2 ("Time_2", Float ) = 1
        _node_4993 ("node_4993", Float ) = 0
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
            uniform sampler2D _1; uniform float4 _1_ST;
            uniform float _Time_1;
            uniform sampler2D _mask; uniform float4 _mask_ST;
            uniform float4 _node_9972;
            uniform float _beishu1;
            uniform sampler2D _2; uniform float4 _2_ST;
            uniform float _Time_2;
            uniform float _node_4993;
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
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 node_2042 = _Time + _TimeEditor;
                float2 node_1835 = (i.uv0+(node_2042.g*_Time_1)*float2(1,0));
                float4 _1_var = tex2D(_1,TRANSFORM_TEX(node_1835, _1));
                float4 node_8912 = _Time + _TimeEditor;
                float2 node_1190 = (i.uv0+(node_8912.g*_Time_2)*float2(1,0));
                float4 _2_var = tex2D(_2,TRANSFORM_TEX(node_1190, _2));
                float4 _mask_var = tex2D(_mask,TRANSFORM_TEX(i.uv0, _mask));
                float3 emissive = ((i.vertexColor.rgb*_node_9972.rgb*(_1_var.rgb*_2_var.rgb)*_beishu1)*_mask_var.rgb);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,(_1_var.r*_2_var.r*_mask_var.r*_node_4993));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
