Shader "Custom/ChannelMapper"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_MapRed ("Red", float) = 0
		_MapGreen ("Green", float) = 0
		_MapBlue ("Blue", float) = 0
    }
    SubShader
    {
		Cull Off ZWrite Off

		Pass
        {
			Blend One One

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _MapRed;
			float _MapBlue;
			float _MapGreen;
			float _Invert;

            float4 frag (v2f_img i) : SV_Target
            {
                float val = tex2D(_MainTex, i.uv).r;
				return float4(val * _MapRed, val*_MapGreen, val * _MapBlue, 1);
            }
            ENDCG
        }
    }
}
