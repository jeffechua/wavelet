Shader "WaveEngine/SystemRender"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            float4 frag (v2f_img i) : SV_Target
            {
                // sample the texture
                float col = tex2D(_MainTex, i.uv);
                return col > 0 ? float4(col, 0, 0,1) : float4(0, 0, -col, 1);
            }
            ENDCG
        }
    }
}
