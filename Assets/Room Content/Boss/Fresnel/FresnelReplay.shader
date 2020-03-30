Shader "Custom/FresnelReplay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		[MaterialToggle] _North ("North", float) = 0
		[MaterialToggle] _East ("East", float) = 0
		[MaterialToggle] _South ("South", float) = 0
		[MaterialToggle] _West ("West", float) = 0
		[MaterialToggle] _ReadVelocity ("Read velocity", float) = 0
		_ImportScale ("Import scale", float) = 0
		_ClipLimit ("Clip limit", Range(0,0.5)) = 0
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
			float _ReadVelocity;
			float _ImportScale;
            float _ClipLimit;
			float _North;
			float _East;
			float _South;
			float _West;

            float4 frag (v2f_img i) : SV_Target
            {
				if (!((_North == 1 && i.uv.y > 1 - _ClipLimit) ||
					(_South == 1 && i.uv.y < _ClipLimit) ||
					(_East == 1 && i.uv.x > 1 - _ClipLimit) ||
					(_West == 1 && i.uv.x < _ClipLimit))){
						discard;
				}
				float r = (_ReadVelocity == 1 ? tex2D(_MainTex, i.uv).g : tex2D(_MainTex,i.uv).r) / _ImportScale;
				return float4(r,1,1,1);
            }
            ENDCG
        }
    }
}
