﻿Shader "WaveEngine/SystemRender"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_IntensityScale ("Intensity Scale", float) = 1
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _IntensityScale;

            float4 frag (v2f_img i) : SV_Target
            {
                // sample the texture
                float val = tex2D(_MainTex, i.uv).r * _IntensityScale;
				val = abs(val) < 0.5 ? val/2 : sign(val);
                return val > 0 ? float4(val, 0, 0, 1) : float4(0, 0, -val, 1);
            }
            ENDCG
        }
    }
}