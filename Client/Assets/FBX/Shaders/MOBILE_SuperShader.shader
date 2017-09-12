Shader "Custom/SuperShader" {
    Properties {
        _MainTex ("Base(RGB) Gloss(A)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
		_Mask ("Specular(R)Rim(G)Ref(B)", 2D) = "white" {}
		_Reflection ("Reflection", Cube) = "_Skybox" {}
		_Emissive ("Emissive", Color) = (0.16,0.16,0.16,1)
		_RimColor ("Rim Color", Color) = (0.5,0.5,0.5,1)
		_RimPower ("Rim Power", Float ) = 1
        _SpecularLevel ("Specular Level", Float ) = 1
        _GlossLevel ("Gloss Level", Float ) = 0.5
        _RefLevel ("Ref Level", Float ) = 1
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
		Fog {mode off }
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            //#pragma multi_compile_fwdbase_fullshadows
            //#pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            //#pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _Mask; uniform float4 _Mask_ST;
			uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
			uniform samplerCUBE _Reflection;
			uniform fixed4 _Emissive;
			uniform fixed4 _RimColor;
            uniform half _RimPower;
			uniform half _SpecularLevel;
            uniform half _RefLevel;
            uniform half _GlossLevel;
            
			
            
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 binormalDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(float4(v.normal,0), _World2Object).xyz;
                o.tangentDir = normalize( mul( _Object2World, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.binormalDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(_Object2World, v.vertex);
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.binormalDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
/////// Normals:
                float2 node_186 = i.uv0;
                float3 normalLocal = UnpackNormal(tex2D(_BumpMap,TRANSFORM_TEX(node_186.rg, _BumpMap))).rgb;
                float3 normalDirection =  normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = dot( normalDirection, lightDirection );
                float3 diffuse = max( 0.0, NdotL) * attenColor + _Emissive.rgb;//UNITY_LIGHTMODEL_AMBIENT.rgb;
////// Emissive:
                float4 node_24 = tex2D(_Mask,TRANSFORM_TEX(node_186.rg, _Mask));
                float3 emissive = ((node_24.g*pow(1.0-max(0,dot(normalDirection, viewDirection)),2.0)*_RimPower*_RimColor.rgb)+(node_24.b*texCUBE(_Reflection,viewReflectDirection).rgb*_RefLevel));
///////// Gloss:
                float4 tex = tex2D(_MainTex,TRANSFORM_TEX(node_186.rg, _MainTex));
				float gloss = (tex.a*_GlossLevel);
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                NdotL = max(0.0, NdotL);
                float node_62 = (node_24.r*_SpecularLevel);
                float3 specularColor = float3(node_62,node_62,node_62);
                float3 specular = (floor(attenuation) * _LightColor0.xyz) * pow(max(0,dot(halfDirection,normalDirection)),specPow) * specularColor;
                float3 finalColor = 0;
                float3 diffuseLight = diffuse;
                finalColor += diffuseLight * tex.rgb;
                finalColor += specular;
                finalColor += emissive;
/// Final Color:
                return fixed4(finalColor,1);
            }
            ENDCG
        }
        
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
