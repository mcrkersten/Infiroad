// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TerrainTest"
{
	Properties
	{
		_DisplaceAmount("Displace Amount", Range( 0 , 64)) = 0
		_EdgeLength ( "Edge length", Range( 2, 50 ) ) = 2
		_TessMaxDisp( "Max Displacement", Float ) = 16
		_TessPhongStrength( "Phong Tess Strength", Range( 0, 1 ) ) = 1
		_DisplacementOffset("DisplacementOffset", Range( 0 , 1)) = 0.25
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.5
		[Gamma]_Main("Main", 2D) = "white" {}
		_TintColor("TintColor", Color) = (1,1,1,0)
		_FoliageBacklightNormals("FoliageBacklightNormals", Range( 0 , 1)) = 0.1
		_FoliageBacklighting("FoliageBacklighting", Range( 0 , 16)) = 1
		_Properties("Properties", 2D) = "white" {}
		[NoScaleOffset][Normal]_Normals("Normals", 2D) = "bump" {}
		_NormalScale("NormalScale", Range( 0 , 8)) = 0
		_Surface("Surface", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "DisableBatching" = "True" }
		Cull Back
		CGPROGRAM
		#include "UnityPBSLighting.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "Tessellation.cginc"
		#pragma target 4.6
		#include "HLSLSupport.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityShaderUtilities.cginc"
		#include "Lighting.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#pragma surface surf StandardCustomLighting keepalpha addshadow fullforwardshadows vertex:vertexDataFunc tessellate:tessFunction tessphong:_TessPhongStrength 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
			float3 worldRefl;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform sampler2D _Properties;
		uniform float4 _Properties_ST;
		uniform float _DisplacementOffset;
		uniform float _DisplaceAmount;
		uniform float4 _TintColor;
		uniform sampler2D _Main;
		uniform float4 _Main_ST;
		uniform sampler2D _Normals;
		uniform float _NormalScale;
		uniform float skyAmbientBrightness;
		uniform sampler2D _Surface;
		uniform float4 _Surface_ST;
		uniform float _FoliageBacklighting;
		uniform float4 sunColor;
		uniform float4 sunHaze;
		uniform float3 sunVector;
		uniform float _FoliageBacklightNormals;
		uniform float _Smoothness;
		uniform float reflectionBrightness;
		uniform float4 horizonColor;
		uniform float4 spaceColor;
		uniform float horizonOffset;
		uniform float horizonTightness;
		uniform float atmosphereBrightness;
		uniform float fogFar;
		uniform float fogNear;
		uniform float sunIntensity;
		uniform float fogExp;
		uniform float _EdgeLength;
		uniform float _TessMaxDisp;
		uniform float _TessPhongStrength;


		inline float3 ASESafeNormalize(float3 inVec)
		{
			float dp3 = max( 0.001f , dot( inVec , inVec ) );
			return inVec* rsqrt( dp3);
		}


		float3 SpecCube1_Sample60_g253( float3 worldReflection, float mipLevel )
		{
			return Unity_GlossyEnvironment(
			 UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0),
				    unity_SpecCube1_HDR,
				    worldReflection,
				    1-mipLevel
				);
		}


		float MipConversion64_g253( float Roughness )
		{
			return ((1.7f*Roughness)-0.7f*(Roughness*Roughness))*6;
		}


		float3 SpecCube0_Sample56_g253( float3 worldReflection, float mipLevel )
		{
			return UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, worldReflection,mipLevel);
		}


		float4 probe0Pos51_g254(  )
		{
			return unity_SpecCube0_ProbePosition;
		}


		float4 probe1Pos52_g254(  )
		{
			return unity_SpecCube1_ProbePosition;
		}


		float4 probe1Pos12_g243(  )
		{
			return unity_SpecCube1_ProbePosition;
		}


		float3 probe1BoxMin13_g243(  )
		{
			return unity_SpecCube1_BoxMin;
		}


		float3 probe1BoxMax14_g243(  )
		{
			return unity_SpecCube1_BoxMax;
		}


		float3 BoxProjection10_g246( float3 worldRefl, float3 worldPos, float4 cubemapCenter, float3 boxMin, float3 boxMax )
		{
			// Do we have a valid reflection probe?
			    UNITY_BRANCH
			    if (cubemapCenter.w > 0.0)
			    {
			        float3 nrdir = normalize(worldRefl);
			        //#if 1
			            float3 rbmax = (boxMax.xyz - worldPos) / nrdir;
			            float3 rbmin = (boxMin.xyz - worldPos) / nrdir;
			            float3 rbminmax = (nrdir > 0.0f) ? rbmax : rbmin;
			/*
			        #else // Optimized version
			            float3 rbmax = (boxMax.xyz - worldPos);
			            float3 rbmin = (boxMin.xyz - worldPos);
			            float3 select = step (float3(0,0,0), nrdir);
			            float3 rbminmax = lerp (rbmax, rbmin, select);
			            rbminmax /= nrdir;
			        #endif
			*/
			        float fa = min(min(rbminmax.x, rbminmax.y), rbminmax.z);
			        worldPos -= cubemapCenter.xyz;
			        worldRefl = worldPos + nrdir * fa;
			    }
			    return worldRefl;
		}


		float3 SpecCube1_Sample37_g243( float3 WorldReflection, float Gloss )
		{
			return Unity_GlossyEnvironment( UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0),
				    unity_SpecCube1_HDR,
				    WorldReflection,
				    Gloss
				);
		}


		float4 probe0Pos8_g243(  )
		{
			return unity_SpecCube0_ProbePosition;
		}


		float4 probe0BoxMin9_g243(  )
		{
			return unity_SpecCube0_BoxMin;
		}


		float3 probe0BoxMax10_g243(  )
		{
			return unity_SpecCube0_BoxMax;
		}


		float3 BoxProjection10_g245( float3 worldRefl, float3 worldPos, float4 cubemapCenter, float3 boxMin, float3 boxMax )
		{
			// Do we have a valid reflection probe?
			    UNITY_BRANCH
			    if (cubemapCenter.w > 0.0)
			    {
			        float3 nrdir = normalize(worldRefl);
			        //#if 1
			            float3 rbmax = (boxMax.xyz - worldPos) / nrdir;
			            float3 rbmin = (boxMin.xyz - worldPos) / nrdir;
			            float3 rbminmax = (nrdir > 0.0f) ? rbmax : rbmin;
			/*
			        #else // Optimized version
			            float3 rbmax = (boxMax.xyz - worldPos);
			            float3 rbmin = (boxMin.xyz - worldPos);
			            float3 select = step (float3(0,0,0), nrdir);
			            float3 rbminmax = lerp (rbmax, rbmin, select);
			            rbminmax /= nrdir;
			        #endif
			*/
			        float fa = min(min(rbminmax.x, rbminmax.y), rbminmax.z);
			        worldPos -= cubemapCenter.xyz;
			        worldRefl = worldPos + nrdir * fa;
			    }
			    return worldRefl;
		}


		float MipConversion39_g243( float Roughness )
		{
			return ((1.7f*Roughness)-0.7f*(Roughness*Roughness))*6;
		}


		float3 SpecCube0_Sample38_g243( float3 WorldReflection, float Gloss )
		{
			return UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, WorldReflection,Gloss);
		}


		float4 probe0Pos51_g244(  )
		{
			return unity_SpecCube0_ProbePosition;
		}


		float4 probe1Pos52_g244(  )
		{
			return unity_SpecCube1_ProbePosition;
		}


		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityEdgeLengthBasedTessCull (v0.vertex, v1.vertex, v2.vertex, _EdgeLength , _TessMaxDisp );
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float2 uv_Properties = v.texcoord * _Properties_ST.xy + _Properties_ST.zw;
			float4 tex2DNode14 = tex2Dlod( _Properties, float4( uv_Properties, 0, 0.0) );
			float3 ase_vertexNormal = v.normal.xyz;
			float3 Displacement30 = ( ( tex2DNode14.g - _DisplacementOffset ) * ase_vertexNormal * _DisplaceAmount );
			v.vertex.xyz += Displacement30;
			v.vertex.w = 1;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float2 uv_Main = i.uv_texcoord * _Main_ST.xy + _Main_ST.zw;
			float4 ColorTex212 = ( _TintColor * tex2D( _Main, uv_Main ) * 1.5 );
			float2 uv_Normals20 = i.uv_texcoord;
			float3 normalizeResult23 = ASESafeNormalize( UnpackScaleNormal( tex2D( _Normals, uv_Normals20 ), _NormalScale ) );
			float3 Normals24 = normalizeResult23;
			float3 temp_output_29_0_g253 = Normals24;
			float3 newWorldNormal53_g253 = normalize( (WorldNormalVector( i , temp_output_29_0_g253 )) );
			float3 worldReflection60_g253 = newWorldNormal53_g253;
			float mipLevel60_g253 = 0.0;
			float3 localSpecCube1_Sample60_g253 = SpecCube1_Sample60_g253( worldReflection60_g253 , mipLevel60_g253 );
			float3 worldReflection56_g253 = newWorldNormal53_g253;
			float Roughness64_g253 = 0.9;
			float localMipConversion64_g253 = MipConversion64_g253( Roughness64_g253 );
			float mipLevel56_g253 = localMipConversion64_g253;
			float3 localSpecCube0_Sample56_g253 = SpecCube0_Sample56_g253( worldReflection56_g253 , mipLevel56_g253 );
			float4 localprobe0Pos51_g254 = probe0Pos51_g254();
			float3 ase_worldPos = i.worldPos;
			float4 localprobe1Pos52_g254 = probe1Pos52_g254();
			float3 lerpResult59_g253 = lerp( localSpecCube1_Sample60_g253 , localSpecCube0_Sample56_g253 , saturate( max( (saturate( (0.0 + (( 1.0 - ( distance( localprobe0Pos51_g254 , float4( ase_worldPos , 0.0 ) ) / ( distance( localprobe0Pos51_g254 , localprobe1Pos52_g254 ) * 0.5 ) ) ) - 0.25) * (1.0 - 0.0) / (0.75 - 0.25)) )*0.5 + 0.5) , 0.5 ) ));
			float3 ase_worldViewDir = Unity_SafeNormalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float fresnelNdotV65_g253 = dot( ase_normWorldNormal, ase_worldViewDir );
			float fresnelNode65_g253 = ( 0.7 + 0.3 * pow( max( 1.0 - fresnelNdotV65_g253 , 0.0001 ), 1.0 ) );
			float2 uv_Surface = i.uv_texcoord * _Surface_ST.xy + _Surface_ST.zw;
			float4 tex2DNode215 = tex2D( _Surface, uv_Surface );
			float surfAO218 = tex2DNode215.b;
			float surfSmooth217 = tex2DNode215.g;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult4 = dot( normalize( (WorldNormalVector( i , Normals24 )) ) , ase_worldlightDir );
			float2 uv_Properties = i.uv_texcoord * _Properties_ST.xy + _Properties_ST.zw;
			float4 tex2DNode14 = tex2D( _Properties, uv_Properties );
			float Foliage253 = tex2DNode14.r;
			float lerpResult372 = lerp( saturate( dotResult4 ) , (dotResult4*0.5 + 0.5) , Foliage253);
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float4 temp_output_11_0_g255 = ColorTex212;
			float temp_output_1_0_g255 = Foliage253;
			float temp_output_17_0_g255 = (ase_lightAtten*0.75 + 0.25);
			float temp_output_26_0_g255 = _FoliageBacklighting;
			float4 lerpResult43_g255 = lerp( sunColor , sunHaze , float4( 0.5019608,0.5019608,0.5019608,0 ));
			float4 lerpResult39_g255 = lerp( temp_output_11_0_g255 , lerpResult43_g255 , 0.35);
			float3 normalizeResult4_g255 = normalize( sunVector );
			float3 temp_output_13_0_g255 = ase_worldViewDir;
			float dotResult5_g255 = dot( normalizeResult4_g255 , temp_output_13_0_g255 );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_tangentToWorldFast = float3x3(ase_worldTangent.x,ase_worldBitangent.x,ase_worldNormal.x,ase_worldTangent.y,ase_worldBitangent.y,ase_worldNormal.y,ase_worldTangent.z,ase_worldBitangent.z,ase_worldNormal.z);
			float fresnelNdotV18_g255 = dot( mul(ase_tangentToWorldFast,Normals24), temp_output_13_0_g255 );
			float fresnelNode18_g255 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV18_g255, 1.0 ) );
			float lerpResult24_g255 = lerp( saturate( fresnelNode18_g255 ) , 1.0 , _FoliageBacklightNormals);
			float3 newWorldReflection3_g243 = normalize( WorldReflectionVector( i , Normals24 ) );
			float3 temp_output_1_0_g246 = newWorldReflection3_g243;
			float3 worldRefl10_g246 = temp_output_1_0_g246;
			float3 temp_output_2_0_g246 = ase_worldPos;
			float3 worldPos10_g246 = temp_output_2_0_g246;
			float4 localprobe1Pos12_g243 = probe1Pos12_g243();
			float4 temp_output_3_0_g246 = localprobe1Pos12_g243;
			float4 cubemapCenter10_g246 = temp_output_3_0_g246;
			float3 localprobe1BoxMin13_g243 = probe1BoxMin13_g243();
			float3 temp_output_4_0_g246 = localprobe1BoxMin13_g243;
			float3 boxMin10_g246 = temp_output_4_0_g246;
			float3 localprobe1BoxMax14_g243 = probe1BoxMax14_g243();
			float3 temp_output_5_0_g246 = localprobe1BoxMax14_g243;
			float3 boxMax10_g246 = temp_output_5_0_g246;
			float3 localBoxProjection10_g246 = BoxProjection10_g246( worldRefl10_g246 , worldPos10_g246 , cubemapCenter10_g246 , boxMin10_g246 , boxMax10_g246 );
			float3 WorldReflection37_g243 = localBoxProjection10_g246;
			float temp_output_220_0 = ( _Smoothness * surfSmooth217 );
			float temp_output_40_0_g243 = ( 1.0 - temp_output_220_0 );
			float Gloss37_g243 = temp_output_40_0_g243;
			float3 localSpecCube1_Sample37_g243 = SpecCube1_Sample37_g243( WorldReflection37_g243 , Gloss37_g243 );
			float3 temp_output_1_0_g245 = newWorldReflection3_g243;
			float3 worldRefl10_g245 = temp_output_1_0_g245;
			float3 temp_output_2_0_g245 = ase_worldPos;
			float3 worldPos10_g245 = temp_output_2_0_g245;
			float4 localprobe0Pos8_g243 = probe0Pos8_g243();
			float4 temp_output_3_0_g245 = localprobe0Pos8_g243;
			float4 cubemapCenter10_g245 = temp_output_3_0_g245;
			float4 localprobe0BoxMin9_g243 = probe0BoxMin9_g243();
			float3 temp_output_4_0_g245 = localprobe0BoxMin9_g243.xyz;
			float3 boxMin10_g245 = temp_output_4_0_g245;
			float3 localprobe0BoxMax10_g243 = probe0BoxMax10_g243();
			float3 temp_output_5_0_g245 = localprobe0BoxMax10_g243;
			float3 boxMax10_g245 = temp_output_5_0_g245;
			float3 localBoxProjection10_g245 = BoxProjection10_g245( worldRefl10_g245 , worldPos10_g245 , cubemapCenter10_g245 , boxMin10_g245 , boxMax10_g245 );
			float3 WorldReflection38_g243 = localBoxProjection10_g245;
			float Roughness39_g243 = temp_output_40_0_g243;
			float localMipConversion39_g243 = MipConversion39_g243( Roughness39_g243 );
			float Gloss38_g243 = localMipConversion39_g243;
			float3 localSpecCube0_Sample38_g243 = SpecCube0_Sample38_g243( WorldReflection38_g243 , Gloss38_g243 );
			float4 localprobe0Pos51_g244 = probe0Pos51_g244();
			float4 localprobe1Pos52_g244 = probe1Pos52_g244();
			float3 lerpResult15_g243 = lerp( localSpecCube1_Sample37_g243 , localSpecCube0_Sample38_g243 , saturate( saturate( max( (saturate( (0.0 + (( 1.0 - ( distance( localprobe0Pos51_g244 , float4( ase_worldPos , 0.0 ) ) / ( distance( localprobe0Pos51_g244 , localprobe1Pos52_g244 ) * 0.5 ) ) ) - 0.25) * (1.0 - 0.0) / (0.75 - 0.25)) )*0.5 + 0.5) , 0.5 ) ) ));
			float fresnelNdotV172 = dot( mul(ase_tangentToWorldFast,Normals24), ase_worldViewDir );
			float fresnelNode172 = ( 0.125 + 1.5 * pow( max( 1.0 - fresnelNdotV172 , 0.0001 ), 3.0 ) );
			float4 Lighting28 = ( ( ColorTex212 * float4( ( lerpResult59_g253 * skyAmbientBrightness * 1.0 * fresnelNode65_g253 ) , 0.0 ) * surfAO218 * ( 1.0 - surfSmooth217 ) * 2.0 ) + ( lerpResult372 * float4( ( ase_lightAtten * ase_lightColor.rgb * ase_lightColor.a ) , 0.0 ) * ColorTex212 ) + ( ( temp_output_11_0_g255 * float4( 0,0,0,0 ) * temp_output_1_0_g255 * temp_output_17_0_g255 * ( 0.5 * temp_output_26_0_g255 ) ) + ( lerpResult39_g255 * temp_output_1_0_g255 * pow( saturate( dotResult5_g255 ) , 2.0 ) * temp_output_26_0_g255 * temp_output_17_0_g255 * lerpResult24_g255 * length( lerpResult59_g253 ) ) ) + float4( ( ( lerpResult15_g243 * reflectionBrightness ) * surfAO218 * pow( surfSmooth217 , 2.0 ) * saturate( fresnelNode172 ) ) , 0.0 ) );
			float3 normalizeResult11_g256 = normalize( -sunVector );
			float3 temp_output_59_0_g256 = ase_worldViewDir;
			float dotResult12_g256 = dot( normalizeResult11_g256 , temp_output_59_0_g256 );
			float temp_output_15_0_g256 = saturate( dotResult12_g256 );
			float dotFromLight57_g256 = temp_output_15_0_g256;
			float4 lerpResult60_g256 = lerp( horizonColor , spaceColor , (dotFromLight57_g256*0.75 + 0.0));
			float dotResult5_g256 = dot( float3(0,-1,0) , temp_output_59_0_g256 );
			float4 lerpResult29_g256 = lerp( ( horizonColor * 2.0 ) , spaceColor , saturate( pow( saturate( (0.0 + (( dotResult5_g256 - horizonOffset ) - 0.0) * (1.0 - 0.0) / (horizonTightness - 0.0)) ) , 0.5 ) ));
			float atmoBright67_g256 = atmosphereBrightness;
			float4 lerpResult37_g256 = lerp( ( lerpResult29_g256 * ( 2.5 - atmoBright67_g256 ) ) , spaceColor , saturate( (temp_output_15_0_g256*0.5 + 0.25) ));
			float4 ase_vertex4Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 ase_viewPos = UnityObjectToViewPos( ase_vertex4Pos );
			float ase_screenDepth = -ase_viewPos.z;
			float cameraDepthFade26_g256 = (( ase_screenDepth -_ProjectionParams.y - fogNear ) / fogFar);
			float clampResult31_g256 = clamp( cameraDepthFade26_g256 , 0.0 , 1.0 );
			float4 lerpResult44_g256 = lerp( lerpResult60_g256 , lerpResult37_g256 , clampResult31_g256);
			float3 normalizeResult14_g256 = normalize( sunVector );
			float dotResult22_g256 = dot( normalizeResult14_g256 , temp_output_59_0_g256 );
			float temp_output_30_0_g256 = saturate( dotResult22_g256 );
			float temp_output_43_0_g256 = pow( clampResult31_g256 , 0.666 );
			float temp_output_42_0_g256 = ( sunIntensity * 0.5 );
			float dotToHorizon70_g256 = dotResult5_g256;
			float4 lerpResult96 = lerp( Lighting28 , ( ( lerpResult44_g256 + ( ( ( pow( temp_output_30_0_g256 , 3.0 ) * sunHaze * 0.2 * temp_output_43_0_g256 * temp_output_42_0_g256 ) + ( pow( temp_output_30_0_g256 , 64.0 ) * sunColor * 0.5 * temp_output_43_0_g256 * temp_output_42_0_g256 ) + ( pow( temp_output_30_0_g256 , 2048.0 ) * sunColor * 2.0 * pow( clampResult31_g256 , 4.0 ) * temp_output_42_0_g256 ) ) * ( 1.0 - dotToHorizon70_g256 ) ) ) * atmosphereBrightness ) , pow( clampResult31_g256 , fogExp ));
			c.rgb = lerpResult96.rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18921
1858;113;1713;883;634.5811;300.2467;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;37;-860.0065,-277.3183;Inherit;False;Property;_NormalScale;NormalScale;14;0;Create;True;0;0;0;False;0;False;0;0.5;0;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;20;-566.226,-324.8979;Inherit;True;Property;_Normals;Normals;13;2;[NoScaleOffset];[Normal];Create;True;0;0;0;False;0;False;20;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalizeNode;23;-220.0255,-347.4977;Inherit;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;24;-33.4483,-332.9486;Inherit;False;Normals;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;215;-661.5584,-629.3357;Inherit;True;Property;_Surface;Surface;15;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;27;-3681.151,-428.6093;Inherit;False;2016.915;1615.413;Lighting  Comp;42;11;143;7;10;9;4;26;6;2;1;25;167;181;174;172;184;185;207;173;214;219;221;222;176;220;175;238;239;257;254;297;296;372;373;375;376;333;299;405;406;409;416;;1,0.5666981,0,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;217;-215.345,-593.3359;Inherit;False;surfSmooth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;413;-1432.971,216.1191;Inherit;True;Property;_Properties;Properties;12;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;25;-3602.476,-369.0443;Inherit;False;24;Normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;221;-3438.979,463.1772;Inherit;False;217;surfSmooth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;187;-671.968,-1121.801;Inherit;False;Property;_TintColor;TintColor;9;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;14;-1111.69,369.2112;Inherit;True;Property;_tex;tex;10;2;[Gamma];[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;141;-750.2131,-944.1512;Inherit;True;Property;_Main;Main;8;1;[Gamma];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;417;-629.9822,-747.55;Inherit;False;Constant;_Float4;Float 4;12;0;Create;True;0;0;0;False;0;False;1.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;1;-3404.762,-362.6247;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;2;-3433.762,-163.6247;Inherit;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;176;-3479.244,350.5726;Inherit;False;Property;_Smoothness;Smoothness;7;0;Create;True;0;0;0;False;0;False;0.5;0.95;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;253;-741.533,407.8024;Inherit;False;Foliage;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;184;-3387.59,565.3975;Inherit;False;24;Normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;4;-3141.762,-268.6247;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;220;-3141.099,417.4203;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;213;-345.7313,-880.649;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;296;-2576.861,-409.041;Inherit;False;217;surfSmooth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;167;-2722.672,257.4509;Inherit;False;24;Normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;218;-204.8382,-517.0313;Inherit;False;surfAO;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;238;-2500,492;Inherit;False;217;surfSmooth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;9;-3140.267,53.01674;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.FresnelNode;172;-3154.473,574.7512;Inherit;False;Standard;TangentNormal;ViewDir;True;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0.125;False;2;FLOAT;1.5;False;3;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightAttenuation;6;-3240.148,-23.73215;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;406;-3049.708,175.1431;Inherit;False;253;Foliage;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;333;-2630.512,-215.3548;Inherit;False;Constant;_Float0;Float 0;10;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;409;-2811.978,356.2257;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;207;-2979.692,-339.6879;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;26;-2875.374,-165.8802;Inherit;False;24;Normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;212;-116.3995,-876.7831;Inherit;False;ColorTex;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;373;-3017.59,-274.2813;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;299;-2217.861,-89.04108;Inherit;False;Constant;_Float7;Float 7;10;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;254;-2690.483,110.6051;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;376;-2531.59,159.7187;Inherit;False;Property;_FoliageBacklightNormals;FoliageBacklightNormals;10;0;Create;True;0;0;0;False;0;False;0.1;0.75;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;185;-2289.59,572.3975;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;239;-2292,476;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;375;-2533.59,223.7187;Inherit;False;Property;_FoliageBacklighting;FoliageBacklighting;11;0;Create;True;0;0;0;False;0;False;1;2.5;0;16;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;416;-2481.304,-206.2325;Inherit;False;CustomIndirectLighting;-1;;253;33369916d215ad04daec9e7891aaa642;0;2;42;FLOAT;1;False;29;FLOAT3;0,0,0;False;2;FLOAT;66;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-2833.267,-65.98326;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;214;-2718.793,-352.739;Inherit;False;212;ColorTex;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;372;-2804.348,-299.1314;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;222;-2300.993,405.6657;Inherit;False;218;surfAO;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;255;-2425.838,-25.4921;Inherit;False;24;Normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;380;-2534.2,283.3853;Inherit;False;CustomReflections;-1;;243;216c007893ca6b646a890e892bb1987f;0;2;1;FLOAT3;0,0,0;False;40;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;219;-2430.734,-116.8173;Inherit;False;218;surfAO;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;257;-2425.72,55.54031;Inherit;False;253;Foliage;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;297;-2409.861,-399.0411;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-1082.63,792.8758;Inherit;False;Property;_DisplacementOffset;DisplacementOffset;6;0;Create;True;0;0;0;False;0;False;0.25;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;143;-2066.845,-221.1278;Inherit;False;5;5;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;181;-1851.754,284.7338;Inherit;False;4;4;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;410;-2120.838,57.5079;Inherit;False;Custom Foliage Backlighting;-1;;255;f5122d797b27de24fae72b2c89dd001a;0;8;31;FLOAT;1;False;19;FLOAT3;0,0,0;False;13;FLOAT3;0,0,0;False;1;FLOAT;0;False;40;FLOAT;0.35;False;27;FLOAT;0.05;False;26;FLOAT;2.5;False;11;COLOR;0.3125657,0.7264151,0.09251513,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-2545.084,-80.14696;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;11;-1779.267,-29.58326;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;21;-762.2299,531.7755;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;-0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-870.4897,931.3115;Inherit;False;Property;_DisplaceAmount;Displace Amount;0;0;Create;True;0;0;0;False;0;False;0;50;0;64;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;16;-920.6898,859.3112;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;311;-685.4836,220.3886;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;28;-1560.081,-36.90105;Inherit;False;Lighting;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-487.4897,521.3113;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;-387.8259,75.76837;Inherit;False;28;Lighting;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;30;-292.3535,521.0545;Inherit;False;Displacement;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;392;-466.1777,176.8996;Inherit;False;CustomDistanceFog;-1;;256;821cf51e4e97ebc4ca0a81168afa7c47;0;2;69;FLOAT;0;False;59;FLOAT3;0,0,0;False;2;COLOR;0;FLOAT;58
Node;AmplifyShaderEditor.OneMinusNode;173;-2914.673,545.1514;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;216;-211.5243,-692.9144;Inherit;False;Surface;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;198;-169.7549,1.586975;Inherit;False;216;Surface;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;174;-2696.472,471.0509;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;4;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;284;-30.86963,253.42;Inherit;False;212;ColorTex;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;175;-2902.043,469.7993;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;405;-2830.708,49.14307;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0.5;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;31;-32.8,331.3;Inherit;False;30;Displacement;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;96;-96.0369,119.6263;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;342,-1;Float;False;True;-1;6;ASEMaterialInspector;0;0;CustomLighting;TerrainTest;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;True;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;True;3;2;250;16;True;1;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;20;5;37;0
WireConnection;23;0;20;0
WireConnection;24;0;23;0
WireConnection;217;0;215;2
WireConnection;14;0;413;0
WireConnection;1;0;25;0
WireConnection;253;0;14;1
WireConnection;4;0;1;0
WireConnection;4;1;2;0
WireConnection;220;0;176;0
WireConnection;220;1;221;0
WireConnection;213;0;187;0
WireConnection;213;1;141;0
WireConnection;213;2;417;0
WireConnection;218;0;215;3
WireConnection;172;0;184;0
WireConnection;409;0;220;0
WireConnection;207;0;4;0
WireConnection;212;0;213;0
WireConnection;373;0;4;0
WireConnection;185;0;172;0
WireConnection;239;0;238;0
WireConnection;416;42;333;0
WireConnection;416;29;26;0
WireConnection;10;0;6;0
WireConnection;10;1;9;1
WireConnection;10;2;9;2
WireConnection;372;0;207;0
WireConnection;372;1;373;0
WireConnection;372;2;406;0
WireConnection;380;1;167;0
WireConnection;380;40;409;0
WireConnection;297;0;296;0
WireConnection;143;0;214;0
WireConnection;143;1;416;0
WireConnection;143;2;219;0
WireConnection;143;3;297;0
WireConnection;143;4;299;0
WireConnection;181;0;380;0
WireConnection;181;1;222;0
WireConnection;181;2;239;0
WireConnection;181;3;185;0
WireConnection;410;31;416;66
WireConnection;410;19;255;0
WireConnection;410;13;254;0
WireConnection;410;1;257;0
WireConnection;410;27;376;0
WireConnection;410;26;375;0
WireConnection;410;11;214;0
WireConnection;7;0;372;0
WireConnection;7;1;10;0
WireConnection;7;2;214;0
WireConnection;11;0;143;0
WireConnection;11;1;7;0
WireConnection;11;2;410;0
WireConnection;11;3;181;0
WireConnection;21;0;14;2
WireConnection;21;1;22;0
WireConnection;28;0;11;0
WireConnection;15;0;21;0
WireConnection;15;1;16;0
WireConnection;15;2;17;0
WireConnection;30;0;15;0
WireConnection;392;59;311;0
WireConnection;173;0;172;0
WireConnection;216;0;215;0
WireConnection;174;0;175;0
WireConnection;174;2;173;0
WireConnection;175;0;220;0
WireConnection;405;2;406;0
WireConnection;96;0;29;0
WireConnection;96;1;392;0
WireConnection;96;2;392;58
WireConnection;0;13;96;0
WireConnection;0;11;31;0
ASEEND*/
//CHKSM=073AC11A92FDC6B62F5EE60A6B1EDE99696EE1DE