#ifndef UNIVERSAL_CARTOON_LIT_PASS_INCLUDED
#define UNIVERSAL_CARTOON_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct Attributes
{
    float4 positionOS    : POSITION;
    float3 normalOS      : NORMAL;
    float4 tangentOS     : TANGENT;
    float2 texcoord      : TEXCOORD0;
    float2 lightmapUV    : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

    float3 posWS                    : TEXCOORD2;    // xyz: posWS

//#ifdef _NORMALMAP
//    float4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
//    float4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
//    float4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
//#else
    float3  normal                  : TEXCOORD3;
    float3 viewDir                  : TEXCOORD4;
//#endif

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

#ifdef _MAIN_LIGHT_SHADOWS
    float4 shadowCoord              : TEXCOORD7;
#endif

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData.positionWS = input.posWS;

//#ifdef _NORMALMAP
//    half3 viewDirWS = half3(input.normal.w, input.tangent.w, input.bitangent.w);
//    inputData.normalWS = TransformTangentToWorld(normalTS,
//        half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz));
//#else
    half3 viewDirWS = input.viewDir;
    inputData.normalWS = input.normal;
//#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    viewDirWS = SafeNormalize(viewDirWS);

    inputData.viewDirectionWS = viewDirWS;
#if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
    inputData.shadowCoord = input.shadowCoord;
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif
    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Simple Lighting) shader
Varyings LitPassVertexSimple(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.posWS.xyz = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;
//
//#ifdef _NORMALMAP
//    output.normal = half4(normalInput.normalWS, viewDirWS.x);
//    output.tangent = half4(normalInput.tangentWS, viewDirWS.y);
//    output.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);
//#else
    output.normal = NormalizeNormalPerVertex(normalInput.normalWS);
    output.viewDir = viewDirWS;
//#endif

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normal.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

#if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    return output;
}

half Map(half x, half in_min, half in_max, half out_min, half out_max) {
	return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
}

half4 CartoonLighting(half3 lightColor, half3 lightDir, half3 normal)
{
	half NdotL = saturate(Map(dot(normal, lightDir),0,0.001,0,1));
	return half4(lightColor * NdotL,NdotL);
}

// Used for StandardSimpleLighting shader
half4 LitPassFragmentSimple(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float2 uv = input.uv;
    half4 diffuseAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    half3 diffuse = diffuseAlpha.rgb * _BaseColor.rgb;


    half alpha = diffuseAlpha.a /** _BaseColor.a*/;
    AlphaDiscard(alpha, _Cutoff);
#ifdef _ALPHAPREMULTIPLY_ON
    diffuse *= alpha;
#endif

//    half3 normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
//    half3 emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
//    half4 specular = SampleSpecularSmoothness(uv, alpha, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));
//    half smoothness = specular.a;

    InputData inputData;
    InitializeInputData(input, /*normalTS*/input.normal, inputData);
	

	Light mainLight = GetMainLight(inputData.shadowCoord);
	MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, half4(0, 0, 0, 0));

	half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
	//return mainLight.shadowAttenuation;

	half3 lightDirection = mainLight.direction;
#ifdef _SEPERATE_LIGHT_DIRECTION
	lightDirection = normalize( half3( Map(_SeperatedLightDirection.x,0,1,-1,1), Map(_SeperatedLightDirection.y, 0, 1, -1, 1), Map(_SeperatedLightDirection.z, 0, 1, -1, 1)));
#endif

#ifdef _CORE_SHADOWS
	half4 lighting = CartoonLighting(attenuatedLightColor, lightDirection, inputData.normalWS);
	half3 diffuseColor =/* inputData.bakedGI*(1 - lighting.a) +*/ lighting.rgb;
#else
	half4 lighting = half4(mainLight.color, mainLight.distanceAttenuation * mainLight.shadowAttenuation);
	//return lighting.a;
	half3 diffuseColor = half3(1, 1, 1);
#endif
	
	//half4 color =  UniversalFragmentBlinnPhong(inputData, diffuse, specular, smoothness, emission, alpha);

	//half3 shadowed = half3(0, 0, 0);
#ifdef _GI_RECEIVE
	half3 shadowed = inputData.bakedGI*diffuse;
#elif defined _USE_SHADOWTEX//elif defined (_USE_SHADOWTEX)
	half3 shadowed = SampleAlbedoAlpha(uv + float2(_BaseMap_ST.y, _BaseMap_ST.y), TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).rgb;
#else
	half3 shadowed = _ShadowColor * diffuse;
#endif

//	half3 shadowed = _ShadowColor * diffuse;
//	if (_BaseColor.a != 1)
//		shadowed = lerp(SampleAlbedoAlpha(uv + float2(_BaseMap_ST.y, _BaseMap_ST.y), TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).rgb, shadowed*diffuse, _BaseColor.a);
//#endif
	


	half3 color = lerp( shadowed/*lerp(diffuseColor*_ShadowColor.rgb, shadowed,_ShadowColor.a)*/, diffuseColor * diffuse, lighting.a);// +emission;

    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    return half4(color,alpha);
};

#endif
