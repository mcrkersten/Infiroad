Shader "PRISM/PRISMLongExposure"
{
	HLSLINCLUDE
#pragma warning( disable:4005 )
#include "CUPPCore.hlsl"

// Disable warnings we aren't interested in
#if defined(UNITY_COMPILER_HLSL)
#pragma warning (disable : 3205) // conversion of larger type to smaller
#pragma warning (disable : 3568) // unknown pragma ignored
#pragma warning (disable : 3571) // "pow(f,e) will not work for negative f"; however in majority of our calls to pow we know f is not negative
#pragma warning (disable : 3206) // implicit truncation of vector type
#endif

#include "PRISMCG.hlsl"
#include "PRISMDoF.hlsl"
#include "PRISMColorSpaces.hlsl"
#pragma multi_compile __ ULTRA_QUALITY

//
		//sampler2D _MainTex;
	UNITY_DECLARE_TEX2D(_MainTex);
	float4 _MainTex_TexelSize;

	UNITY_DECLARE_TEX2D(_LongExpTex);
	float4 _LongExpTex_TexelSize;

	uniform float individualExposureWeight;// 2.

	inline float4 SampleMainTexture(float2 uv)
	{
		return UNITY_SAMPLE_TEX2D(_MainTex, uv);
	}

	inline float4 SampleLongExpTexture(float2 uv)
	{
		return UNITY_SAMPLE_TEX2D(_LongExpTex, uv);
	}


	float4 fragAddNewFrameToLongExp(PostProcessVaryings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

		float4 tex = SampleMainTexture(uv);
		float4 lexptex = SampleLongExpTexture(uv);

		float4 outColor = lerp(lexptex, tex, individualExposureWeight);
			//(tex + lexptex) * 0.5;

		return outColor;
	}


		
		ENDHLSL

	SubShader
	{

		Cull Off ZWrite Off ZTest Always
			Pass
		{
			Name "Initial LongExposure"
			HLSLPROGRAM
			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment fragAddNewFrameToLongExp
			ENDHLSL
		}
			
	}
	Fallback Off
}