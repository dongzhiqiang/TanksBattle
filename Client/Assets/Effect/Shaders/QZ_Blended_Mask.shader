// Shader created with Shader Forge v1.25 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.25;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:32719,y:32712,varname:node_4013,prsc:2|emission-1645-OUT,alpha-2654-OUT;n:type:ShaderForge.SFN_Tex2d,id:2976,x:31742,y:32437,ptovrint:False,ptlb:01,ptin:_01,varname:node_2976,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:5d16b535e81893e438ce044bd0f382f6,ntxv:0,isnm:False|UVIN-366-OUT;n:type:ShaderForge.SFN_Tex2d,id:8938,x:32116,y:33430,ptovrint:False,ptlb:02,ptin:_02,varname:node_8938,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:5850cf862b2bb334da1de0e05627b6fd,ntxv:0,isnm:False|UVIN-1655-OUT;n:type:ShaderForge.SFN_Multiply,id:1645,x:32112,y:32463,varname:node_1645,prsc:2|A-2976-RGB,B-3158-RGB,C-1912-RGB;n:type:ShaderForge.SFN_Color,id:3158,x:31742,y:32634,ptovrint:False,ptlb:01Color,ptin:_01Color,varname:node_3158,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_VertexColor,id:1912,x:31742,y:32806,varname:node_1912,prsc:2;n:type:ShaderForge.SFN_VertexColor,id:4574,x:32116,y:33642,varname:node_4574,prsc:2;n:type:ShaderForge.SFN_Multiply,id:2654,x:32450,y:33481,varname:node_2654,prsc:2|A-8938-R,B-4574-A,C-3158-A;n:type:ShaderForge.SFN_TexCoord,id:9692,x:29978,y:32578,varname:node_9692,prsc:2,uv:0;n:type:ShaderForge.SFN_Panner,id:2444,x:30183,y:32578,varname:node_2444,prsc:2,spu:0.06,spv:0|UVIN-9692-UVOUT;n:type:ShaderForge.SFN_Tex2d,id:8429,x:30511,y:32617,ptovrint:False,ptlb:raodong,ptin:_raodong,varname:node_8429,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:4e833b18c55458244ad1d507f5985a26,ntxv:0,isnm:False|UVIN-2444-UVOUT;n:type:ShaderForge.SFN_Multiply,id:3923,x:30761,y:32582,varname:node_3923,prsc:2|A-3985-OUT,B-8429-R;n:type:ShaderForge.SFN_Slider,id:3985,x:30061,y:32422,ptovrint:False,ptlb:raodong_KongZhi,ptin:_raodong_KongZhi,varname:node_3985,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:0.1;n:type:ShaderForge.SFN_Add,id:1655,x:31042,y:32472,varname:node_1655,prsc:2|A-9369-UVOUT,B-3923-OUT;n:type:ShaderForge.SFN_TexCoord,id:9369,x:30714,y:32206,varname:node_9369,prsc:2,uv:0;n:type:ShaderForge.SFN_Multiply,id:366,x:31338,y:32814,varname:node_366,prsc:2|A-1655-OUT,B-8898-OUT;n:type:ShaderForge.SFN_TexCoord,id:784,x:30046,y:33192,varname:node_784,prsc:2,uv:0;n:type:ShaderForge.SFN_Panner,id:6906,x:30244,y:33174,varname:node_6906,prsc:2,spu:-0.07,spv:0|UVIN-784-UVOUT;n:type:ShaderForge.SFN_Tex2d,id:5139,x:30579,y:33231,ptovrint:False,ptlb:raodong_copy,ptin:_raodong_copy,varname:_raodong_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:902a2fbcdac14054bae4e9ca5d4ae8a6,ntxv:0,isnm:False|UVIN-6906-UVOUT;n:type:ShaderForge.SFN_Multiply,id:6340,x:30829,y:33196,varname:node_6340,prsc:2|A-6511-OUT,B-5139-R;n:type:ShaderForge.SFN_Slider,id:6511,x:30067,y:32986,ptovrint:False,ptlb:raodong_KongZhi_copy,ptin:_raodong_KongZhi_copy,varname:_raodong_KongZhi_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.1,max:0.1;n:type:ShaderForge.SFN_Add,id:8898,x:31110,y:33086,varname:node_8898,prsc:2|A-1108-UVOUT,B-6340-OUT;n:type:ShaderForge.SFN_TexCoord,id:1108,x:30782,y:32820,varname:node_1108,prsc:2,uv:0;proporder:2976-8938-3158-8429-3985-5139-6511;pass:END;sub:END;*/

Shader "Shader Forge/QZ_Blended_Mask" {
    Properties {
        _01 ("01", 2D) = "white" {}
        _02 ("02", 2D) = "white" {}
        _01Color ("01Color", Color) = (0.5,0.5,0.5,1)
        _raodong ("raodong", 2D) = "white" {}
        _raodong_KongZhi ("raodong_KongZhi", Range(0, 0.1)) = 0
        _raodong_copy ("raodong_copy", 2D) = "white" {}
        _raodong_KongZhi_copy ("raodong_KongZhi_copy", Range(0, 0.1)) = 0.1
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
            uniform sampler2D _02; uniform float4 _02_ST;
            uniform float4 _01Color;
            uniform sampler2D _raodong; uniform float4 _raodong_ST;
            uniform float _raodong_KongZhi;
            uniform sampler2D _raodong_copy; uniform float4 _raodong_copy_ST;
            uniform float _raodong_KongZhi_copy;
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
                float4 node_4043 = _Time + _TimeEditor;
                float2 node_2444 = (i.uv0+node_4043.g*float2(0.06,0));
                float4 _raodong_var = tex2D(_raodong,TRANSFORM_TEX(node_2444, _raodong));
                float2 node_1655 = (i.uv0+(_raodong_KongZhi*_raodong_var.r));
                float2 node_6906 = (i.uv0+node_4043.g*float2(-0.07,0));
                float4 _raodong_copy_var = tex2D(_raodong_copy,TRANSFORM_TEX(node_6906, _raodong_copy));
                float2 node_366 = (node_1655*(i.uv0+(_raodong_KongZhi_copy*_raodong_copy_var.r)));
                float4 _01_var = tex2D(_01,TRANSFORM_TEX(node_366, _01));
                float3 emissive = (_01_var.rgb*_01Color.rgb*i.vertexColor.rgb);
                float3 finalColor = emissive;
                float4 _02_var = tex2D(_02,TRANSFORM_TEX(node_1655, _02));
                fixed4 finalRGBA = fixed4(finalColor,(_02_var.r*i.vertexColor.a*_01Color.a));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
