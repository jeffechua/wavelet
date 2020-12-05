Shader "WaveEngine/SystemRender"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_IntensityScale ("Intensity Scale", float) = 1
		_Threshold ("Threshold", float) = 0.5
		_STMult ("Sub-threshold Multiplier", float) = 0.5
        [MaterialToggle] _Absolute ("Absolute", float) = 0
        [MaterialToggle] _Clean ("Clean", float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _IntensityScale;
			float _Threshold;
			float _STMult;
            float _Absolute;
            float _Clean;

            float4 frag (v2f_img i) : SV_Target
            {
                float val = tex2D(_MainTex, i.uv).r * _IntensityScale;
                if(_Absolute)
                    val = abs(val);
				val = abs(val) < _Threshold ? (_Clean ? 0 : val * _STMult) : sign(val);
                return val > 0 ? float4(1, 0, 0, val) : float4(0, 0, 1, -val);
            }
            ENDCG
        }
    }
}
