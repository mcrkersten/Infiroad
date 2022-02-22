// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:0,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:2,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:33187,y:32775,varname:node_3138,prsc:2|diff-3205-OUT,emission-3500-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:31723,y:32565,ptovrint:False,ptlb:ColorBase,ptin:_ColorBase,varname:node_7241,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.6470588,c2:0.6470588,c3:0.6470588,c4:1;n:type:ShaderForge.SFN_TexCoord,id:5091,x:31068,y:32642,varname:node_5091,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_VertexColor,id:2053,x:31773,y:32760,varname:node_2053,prsc:2;n:type:ShaderForge.SFN_Multiply,id:3500,x:32640,y:32874,varname:node_3500,prsc:2|A-1424-OUT,B-2053-RGB,C-4562-OUT;n:type:ShaderForge.SFN_Multiply,id:3205,x:32643,y:32599,varname:node_3205,prsc:2|A-1424-OUT,B-2053-RGB,C-2895-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4562,x:31784,y:33012,ptovrint:False,ptlb:Emissiv_Amnt,ptin:_Emissiv_Amnt,varname:node_4562,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:2895,x:31784,y:32929,ptovrint:False,ptlb:Vtx_Color_Amnt,ptin:_Vtx_Color_Amnt,varname:node_2895,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Color,id:6240,x:31723,y:32303,ptovrint:False,ptlb:Noise_Color,ptin:_Noise_Color,varname:node_6240,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.9338235,c2:0.9338235,c3:0.9338235,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:9145,x:31723,y:32483,ptovrint:False,ptlb:Base_Noise_Amnt,ptin:_Base_Noise_Amnt,varname:node_9145,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Posterize,id:6925,x:31723,y:32138,varname:node_6925,prsc:2|IN-5091-UVOUT,STPS-5039-OUT;n:type:ShaderForge.SFN_Noise,id:1993,x:31914,y:32138,varname:node_1993,prsc:2|XY-6925-OUT;n:type:ShaderForge.SFN_ValueProperty,id:5039,x:31527,y:32369,ptovrint:False,ptlb:Color_Noise_Steps,ptin:_Color_Noise_Steps,varname:node_5039,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:50;n:type:ShaderForge.SFN_Multiply,id:3939,x:32121,y:32218,varname:node_3939,prsc:2|A-1993-OUT,B-6240-RGB,C-9145-OUT;n:type:ShaderForge.SFN_Add,id:1424,x:32325,y:32416,varname:node_1424,prsc:2|A-3939-OUT,B-7241-RGB;proporder:7241-6240-9145-5039-4562-2895;pass:END;sub:END;*/

Shader "Shader Forge/Mobile_Vtx" {
    Properties {
        _ColorBase ("ColorBase", Color) = (0.6470588,0.6470588,0.6470588,1)
        _Noise_Color ("Noise_Color", Color) = (0.9338235,0.9338235,0.9338235,1)
        _Base_Noise_Amnt ("Base_Noise_Amnt", Float ) = 0
        _Color_Noise_Steps ("Color_Noise_Steps", Float ) = 50
        _Emissiv_Amnt ("Emissiv_Amnt", Float ) = 0
        _Vtx_Color_Amnt ("Vtx_Color_Amnt", Float ) = 1
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal 
            #pragma target 3.0
            uniform float4 _ColorBase;
            uniform float _Emissiv_Amnt;
            uniform float _Vtx_Color_Amnt;
            uniform float4 _Noise_Color;
            uniform float _Base_Noise_Amnt;
            uniform float _Color_Noise_Steps;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD10;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.vertexColor = v.vertexColor;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #elif UNITY_SHOULD_SAMPLE_SH
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - 0;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += gi.indirect.diffuse;
                float2 node_6925 = floor(i.uv0 * _Color_Noise_Steps) / (_Color_Noise_Steps - 1);
                float2 node_1993_skew = node_6925 + 0.2127+node_6925.x*0.3713*node_6925.y;
                float2 node_1993_rnd = 4.789*sin(489.123*(node_1993_skew));
                float node_1993 = frac(node_1993_rnd.x*node_1993_rnd.y*(1+node_1993_skew.x));
                float3 node_1424 = ((node_1993*_Noise_Color.rgb*_Base_Noise_Amnt)+_ColorBase.rgb);
                float3 diffuseColor = (node_1424*i.vertexColor.rgb*_Vtx_Color_Amnt);
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
////// Emissive:
                float3 emissive = (node_1424*i.vertexColor.rgb*_Emissiv_Amnt);
/// Final Color:
                float3 finalColor = diffuse + emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_META 1
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal 
            #pragma target 3.0
            uniform float4 _ColorBase;
            uniform float _Emissiv_Amnt;
            uniform float _Vtx_Color_Amnt;
            uniform float4 _Noise_Color;
            uniform float _Base_Noise_Amnt;
            uniform float _Color_Noise_Steps;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.vertexColor = v.vertexColor;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                float2 node_6925 = floor(i.uv0 * _Color_Noise_Steps) / (_Color_Noise_Steps - 1);
                float2 node_1993_skew = node_6925 + 0.2127+node_6925.x*0.3713*node_6925.y;
                float2 node_1993_rnd = 4.789*sin(489.123*(node_1993_skew));
                float node_1993 = frac(node_1993_rnd.x*node_1993_rnd.y*(1+node_1993_skew.x));
                float3 node_1424 = ((node_1993*_Noise_Color.rgb*_Base_Noise_Amnt)+_ColorBase.rgb);
                o.Emission = (node_1424*i.vertexColor.rgb*_Emissiv_Amnt);
                
                float3 diffColor = (node_1424*i.vertexColor.rgb*_Vtx_Color_Amnt);
                o.Albedo = diffColor;
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
