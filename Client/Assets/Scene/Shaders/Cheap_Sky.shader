Shader "Custom/Cheapsky" {

    Properties {

        _MainTex ("Base (RGB)", 2D) = "white" {}

    }

 

    SubShader {

        Pass {

            Material {

                Diffuse(1,1,1,1)

            }

           ZWrite Off

           Lighting Off

           //Fog {Mode Off}

           

           SetTexture [_MainTex] {

                combine texture

           }

        }

    }

}