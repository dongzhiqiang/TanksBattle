Shader "Custom/Outline" {  
   Properties {
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _Outline ("Outline width", Range (.002, 0.03)) = .005
    }
    
	SubShader
    {
	 Tags{"DisableBatching" = "True"}
	pass
        {
            Name "OUTLINE"
			
			Cull front
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            uniform half _Outline;
            uniform fixed4 _OutlineColor;
                
            struct v2f {
                float4 pos : POSITION;
                
            };
            v2f vert (appdata_full v)
            {	
				v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

                float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                float2 offset = TransformViewToProjection(norm.xy);

                o.pos.xy += offset * o.pos.z * _Outline;
                
                return o;
            }
            float4 frag (v2f i) : COLOR
            {
                return _OutlineColor;
            }
            ENDCG
        }
    }
}