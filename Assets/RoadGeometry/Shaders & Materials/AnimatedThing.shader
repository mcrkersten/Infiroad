Shader "Unlit/AnimatedThing"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_WaveAmp ("Wave Amp", Float ) = 0.1
		_WaveFreq ("Wave Freq", Float ) = 8
    }
    SubShader
    {
        Tags {
			"RenderType"="Transparent"
			"Queue"="Transparent"
		}

        Pass
        {

			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


			static float TAU = 6.28318530718;
            sampler2D _MainTex;
			float _WaveAmp;
			float _WaveFreq;

			

            v2f vert (appdata v) {
                v2f o;
				float wave = sin( v.uv.x * TAU * _WaveFreq + _Time.y ) * v.uv.y * _WaveAmp;
				v.vertex.z += wave;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
				return tex2D(_MainTex, i.uv);
            }

            ENDCG
        }
    }
}
