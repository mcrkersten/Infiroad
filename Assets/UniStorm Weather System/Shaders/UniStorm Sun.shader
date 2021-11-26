Shader "UniStorm/Celestial/Sun" {
Properties {
	[HDR]_SunColor ("Sun Color", Color) = (0.5,0.5,0.5,0.5)
	_SunBrightness ("Sun Brightness", Range(0.0, 1.0)) = 1.0
	_MainTex ("Sun Texture", 2D) = "white" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	_OpaqueY("Opaque Y", Float) = -30
	_TransparentY("Transparent Y", Float) = -10
}

Category {
	Tags { "Queue"="Transparent-451" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	ColorMask RGB
    ZWrite Off
	Cull Off
	Lighting Off 

	SubShader {
		Pass {
            Stencil {
                Ref 1
                Comp always
                Pass replace
            }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _SunColor;
			float _SunBrightness;
			half _OpaqueY;
			half _TransparentY;
			uniform float3 _uWorldSpaceCameraPos;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				#ifdef SOFTPARTICLES_ON
				float4 projPos : TEXCOORD2;
				#endif
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.vertex.z = 1.0e-9f;
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);

				float4 worldV = mul(unity_ObjectToWorld, v.vertex);
				o.color.a = 1 - saturate(((_uWorldSpaceCameraPos.y - worldV.y) - _OpaqueY) / (_TransparentY - _OpaqueY));
				return o;
			}

			sampler2D_float _CameraDepthTexture;
			float _InvFade;
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = i.color * (_SunColor) * tex2D(_MainTex, i.texcoord) * _SunBrightness;
			    float intensity = dot(col.rgb, float3(0.3, 0.3, 0.3));
                if (col.a < 0.01) discard;
			    return float4(col.rgb * col.a, intensity * col.a);
			}
			ENDCG 
		}
	}	
}
}
