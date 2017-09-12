// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:1,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:True,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:33101,y:32308,varname:node_4013,prsc:2|emission-8043-OUT;n:type:ShaderForge.SFN_Tex2d,id:457,x:32306,y:32336,ptovrint:False,ptlb:node_2254,ptin:_node_2254,varname:node_2254,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:70387129d7d1ebf43a33d07b53c858ae,ntxv:2,isnm:False|UVIN-2979-UVOUT;n:type:ShaderForge.SFN_Color,id:4927,x:32499,y:32603,ptovrint:False,ptlb:node_4392,ptin:_node_4392,varname:node_4392,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:2118,x:32499,y:32429,varname:node_2118,prsc:2|A-457-RGB,B-4371-RGB;n:type:ShaderForge.SFN_Panner,id:2979,x:32126,y:32336,varname:node_2979,prsc:2,spu:0,spv:0.3|UVIN-4566-UVOUT;n:type:ShaderForge.SFN_Multiply,id:4625,x:32675,y:32408,varname:node_4625,prsc:2|A-4791-OUT,B-2118-OUT,C-4927-RGB;n:type:ShaderForge.SFN_Vector1,id:4791,x:32499,y:32336,varname:node_4791,prsc:2,v1:1.5;n:type:ShaderForge.SFN_Tex2d,id:4371,x:32306,y:32524,ptovrint:False,ptlb:node_2377,ptin:_node_2377,varname:node_2377,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:4f4519c8dadf0a5478a89bac42233550,ntxv:2,isnm:False;n:type:ShaderForge.SFN_TexCoord,id:4566,x:31928,y:32336,varname:node_4566,prsc:2,uv:0;n:type:ShaderForge.SFN_ValueProperty,id:9304,x:32677,y:32305,ptovrint:False,ptlb:node_9304,ptin:_node_9304,varname:node_9304,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Multiply,id:8043,x:32894,y:32342,varname:node_8043,prsc:2|A-9304-OUT,B-4625-OUT;proporder:457-4927-4371-9304;pass:END;sub:END;*/

Shader "Shader Forge/S_UV_Move" {
    Properties {
        _node_2254 ("node_2254", 2D) = "black" {}
        _node_4392 ("node_4392", Color) = (0.5,0.5,0.5,1)
        _node_2377 ("node_2377", 2D) = "black" {}
        _node_9304 ("node_9304", Float ) = 0
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
            Cull Front
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            uniform float4 _TimeEditor;
            uniform sampler2D _node_2254; uniform float4 _node_2254_ST;
            uniform float4 _node_4392;
            uniform sampler2D _node_2377; uniform float4 _node_2377_ST;
            uniform float _node_9304;
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
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 node_8607 = _Time + _TimeEditor;
                float2 node_2979 = (i.uv0+node_8607.g*float2(0,0.3));
                float4 _node_2254_var = tex2D(_node_2254,TRANSFORM_TEX(node_2979, _node_2254));
                float4 _node_2377_var = tex2D(_node_2377,TRANSFORM_TEX(i.uv0, _node_2377));
                float3 emissive = (_node_9304*(1.5*(_node_2254_var.rgb*_node_2377_var.rgb)*_node_4392.rgb));
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
