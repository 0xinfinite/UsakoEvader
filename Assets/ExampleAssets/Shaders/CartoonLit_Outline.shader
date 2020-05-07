// Shader targeted for low end devices. Single Pass Forward Rendering.
Shader "Universal Render Pipeline/Cartoon Lit Outline"
{
    // Keep properties of StandardSpecular shader for upgrade reasons.
    Properties
    {
        _BaseColor("Base Color", Color) = (0.5, 0.5, 0.5, 1)
		_ShadowColor("Shadows Color", Color) = (0.5, 0.5, 0.5, 1)
		[Toggle(_GI_RECEIVE)]_GI_RECEIVE("Enable GI", Float) = 1.0
        _BaseMap("Base Map (RGB) Smoothness / Alpha (A)", 2D) = "white" {}
	[Toggle(_USE_SHADOWTEX)]_USE_SHADOWTEX("Use Shadow on Texture", Float) = 1.0
		[Toggle(_CORE_SHADOWS)]_CORE_SHADOWS("Core Shadow", Float) = 1.0
		[Toggle(_SEPERATE_LIGHT_DIRECTION)]_SEPERATE_LIGHT_DIRECTION("Seperate Light Direction", Float) = 0.0
		_SeperatedLightDirection("Light Direction", Color) = (0.5,0,0.5,0)

        _Cutoff("Alpha Clipping", Range(0.0, 1.0)) = 0.5
        _SpecColor("Specular Color", Color) = (0.5, 0.5, 0.5, 0.5)
        _SpecGlossMap("Specular Map", 2D) = "white" {}
       [Enum(Specular Alpha,0,Albedo Alpha,1)] _SmoothnessSource("Smoothness Source", Float) = 0.0
        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0

        [HideInInspector] _BumpScale("Scale", Float) = 1.0
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}

        _EmissionColor("Emission Color", Color) = (0,0,0)
        [NoScaleOffset]_EmissionMap("Emission Map", 2D) = "white" {}

        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0

			/*[ToogleOff] */[Toggle(_RECEIVE_SHADOWS_OFF)]_ReceiveShadows("Receive Shadows Off", Float) = 1.0

        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
        [HideInInspector] _Smoothness("SMoothness", Float) = 0.5

        // ObsoleteProperties
        [HideInInspector] _MainTex("BaseMap", 2D) = "white" {}
        [HideInInspector] _Color("Base Color", Color) = (0.5, 0.5, 0.5, 1)
        [HideInInspector] _Shininess("Smoothness", Float) = 0.0
        [HideInInspector] _GlossinessSource("GlossinessSource", Float) = 0.0
        [HideInInspector] _SpecSource("SpecularHighlights", Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            // Use same blending / depth states as Standard shader
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
   //         #pragma shader_feature _ _SPECGLOSSMAP _SPECULAR_COLOR
   //         #pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA
  //          #pragma shader_feature _NORMALMAP
  //          #pragma shader_feature _EMISSION
            #pragma shader_feature _RECEIVE_SHADOWS_OFF
			#pragma shader_feature _CORE_SHADOWS
			#pragma shader_feature _GI_RECEIVE
			#pragma shader_feature _USE_SHADOWTEX
			#pragma shader_feature _SEPERATE_LIGHT_DIRECTION

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimple
            #define BUMP_SCALE_NOT_SUPPORTED 1

            #include "CartoonLitInput.hlsl"//"Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "CartoonLitForwardPass.hlsl"
            ENDHLSL
        }
 Pass
		{
			Name "Outline"
			//Tags { "LightMode" = "UniversalForward" }

			// Use same blending / depth states as Standard shader
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
			Cull Front//[_Cull]

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			#pragma vertex LitPassVertexSimple
			#pragma fragment LitPassFragmentSimple
			#define BUMP_SCALE_NOT_SUPPORTED 1

			#include "CartoonLitInput.hlsl"//"Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
			//#include "CartoonLitForwardPass.hlsl"


struct Attributes
{
	float4 positionOS    : POSITION;
	float3 normalOS      : NORMAL;
	//float4 tangentOS     : TANGENT;
	float2 texcoord      : TEXCOORD0;
	//float2 lightmapUV    : TEXCOORD1;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
	float2 uv                       : TEXCOORD0;

	float3 posWS                    : TEXCOORD1;    // xyz: posWS

	float3  normal                  : TEXCOORD3;
	//float3 viewDir                  : TEXCOORD4;
	
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
			//half3 viewDirWS = input.viewDir;
			inputData.normalWS = input.normal;
			//#endif

				inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);


				//viewDirWS = SafeNormalize(viewDirWS);

				//inputData.viewDirectionWS = viewDirWS;
		
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

		VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz+ input.normalOS*0.003);
		//VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
		
		//input.positionOS.xyz += input.normalOS;

		output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
		output.posWS.xyz = vertexInput.positionWS;
		output.positionCS = vertexInput.positionCS;
	
			//output.normal = NormalizeNormalPerVertex(normalInput.normalWS);
			


				return output;
			}

			

		

			// Used for StandardSimpleLighting shader
			half4 LitPassFragmentSimple(Varyings input) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				float2 uv = input.uv;
				half4 diffuseAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
				half3 diffuse = diffuseAlpha.rgb * _BaseColor.rgb;


				half alpha = diffuseAlpha.a/* * _BaseColor.a*/;
				clip(alpha-1);//AlphaDiscard(alpha, 1);
			#ifdef _ALPHAPREMULTIPLY_ON
				diffuse *= alpha;
			#endif

						return half4(diffuse*diffuse,alpha);
					};
			ENDHLSL
		}

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "CartoonLitInput.hlsl"//"Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "ShadowCasterPass.hlsl"//"Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "CartoonLitInput.hlsl"//"Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{ "LightMode" = "Meta" }

            Cull Off

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaSimple

            #pragma shader_feature _EMISSION
            #pragma shader_feature _SPECGLOSSMAP

            #include "CartoonLitInput.hlsl"//"Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitMetaPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "Universal2D"
            Tags{ "LightMode" = "Universal2D" }
            Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON

            #include "CartoonLitInput.hlsl"//"Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Universal2D.hlsl"
            ENDHLSL
        }
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.CartoonLitShader"
}
