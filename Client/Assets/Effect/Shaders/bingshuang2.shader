// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.25 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.25;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:33070,y:32723,varname:node_4013,prsc:2|emission-6532-OUT,alpha-3475-OUT;n:type:ShaderForge.SFN_Tex2d,id:304,x:31555,y:32988,ptovrint:False,ptlb:node_304,ptin:_node_304,varname:node_304,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:4422f16641a1dad47ab1c07e2e583d67,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:2492,x:32368,y:32873,varname:node_2492,prsc:2|A-191-RGB,B-9679-OUT,C-2369-RGB;n:type:ShaderForge.SFN_ValueProperty,id:9679,x:31953,y:32966,ptovrint:False,ptlb:node_9679,ptin:_node_9679,varname:node_9679,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:5;n:type:ShaderForge.SFN_Color,id:2369,x:31953,y:33056,ptovrint:False,ptlb:node_2369,ptin:_node_2369,varname:node_2369,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Fresnel,id:3921,x:32353,y:33301,varname:node_3921,prsc:2|EXP-675-OUT;n:type:ShaderForge.SFN_Multiply,id:9253,x:32656,y:33028,varname:node_9253,prsc:2|A-2492-OUT,B-3921-OUT;n:type:ShaderForge.SFN_ValueProperty,id:675,x:32147,y:33382,ptovrint:False,ptlb:fresbelK,ptin:_fresbelK,varname:node_675,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Panner,id:4679,x:31557,y:32677,varname:node_4679,prsc:2,spu:0.002,spv:0.002|UVIN-8426-UVOUT;n:type:ShaderForge.SFN_TexCoord,id:8426,x:31207,y:32687,varname:node_8426,prsc:2,uv:0;n:type:ShaderForge.SFN_Add,id:3349,x:31779,y:32699,varname:node_3349,prsc:2|A-4679-UVOUT,B-304-B;n:type:ShaderForge.SFN_Tex2d,id:191,x:32025,y:32770,ptovrint:False,ptlb:node_304_copy,ptin:_node_304_copy,varname:_node_304_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:4422f16641a1dad47ab1c07e2e583d67,ntxv:0,isnm:False|UVIN-3349-OUT;n:type:ShaderForge.SFN_Multiply,id:6532,x:32812,y:32691,varname:node_6532,prsc:2|A-9913-OUT,B-9253-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9913,x:32490,y:32642,ptovrint:False,ptlb:node_9913,ptin:_node_9913,varname:node_9913,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:3475,x:32812,y:32964,ptovrint:False,ptlb:xxxx,ptin:_xxxx,varname:node_3475,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;proporder:304-9679-2369-675-191-9913-3475;pass:END;sub:END;*/

Shader "Shader Forge/bingshuang" {
    Properties {
        _node_304 ("node_304", 2D) = "white" {}
        _node_9679 ("node_9679", Float ) = 5
        _node_2369 ("node_2369", Color) = (0.5,0.5,0.5,1)
        _fresbelK ("fresbelK", Float ) = 0
        _node_304_copy ("node_304_copy", 2D) = "white" {}
        _node_9913 ("node_9913", Float ) = 0
        _xxxx ("xxxx", Float ) = 0
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
            Blend One OneMinusSrcAlpha
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
            uniform sampler2D _node_304; uniform float4 _node_304_ST;
            uniform float _node_9679;
            uniform float4 _node_2369;
            uniform float _fresbelK;
            uniform sampler2D _node_304_copy; uniform float4 _node_304_copy_ST;
            uniform float _node_9913;
            uniform float _xxxx;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
////// Lighting:
////// Emissive:
                float4 node_2753 = _Time + _TimeEditor;
                float4 _node_304_var = tex2D(_node_304,TRANSFORM_TEX(i.uv0, _node_304));
                float2 node_3349 = ((i.uv0+node_2753.g*float2(0.002,0.002))+_node_304_var.b);
                float4 _node_304_copy_var = tex2D(_node_304_copy,TRANSFORM_TEX(node_3349, _node_304_copy));
                float3 emissive = (_node_9913*((_node_304_copy_var.rgb*_node_9679*_node_2369.rgb)*pow(1.0-max(0,dot(normalDirection, viewDirection)),_fresbelK)));
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,_xxxx);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
