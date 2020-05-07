Shader "Background/BgShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_ShadowTex("Shadow Texture", 2D) = "white" {}
		_LightTex("Light Texture", 2D) = "black" {}
		_ShadowStrength("ShadowStrength",Range(0,1))=0.5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
							#pragma multi_compile_fwdbase
							#include"AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				UNITY_FOG_COORDS(1)
				float4 pos : SV_POSITION;
				SHADOW_COORDS(2)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _ShadowTex;
			float4 _ShadowTex_ST;
			sampler2D _LightTex;
			float _ShadowStrength;
			
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv1 = TRANSFORM_TEX(v.uv1, _ShadowTex);
				UNITY_TRANSFER_FOG(o,o.pos);
				TRANSFER_SHADOW(o);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 main = tex2D(_MainTex, i.uv);
			clip(main.a -0.9 );

				// sample the texture
				fixed4 col = 0;
				fixed4 outsideShadow = (SHADOW_ATTENUATION(i))*_ShadowStrength+(1-_ShadowStrength);
				//col = (SHADOW_ATTENUATION(i))*_ShadowStrength+(1-_ShadowStrength);
				
				fixed4 shadow = tex2D(_ShadowTex, i.uv1);

				col = lerp(1,outsideShadow,shadow);
				
				
				col *= main;//tex2D(_MainTex, i.uv);
			
			
				fixed4 light = tex2D(_LightTex, i.uv1);
				col *= shadow;
				col += light;//lerp(0,light,outsideShadow);
				
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
		Pass{
				Tags{"LightMode"="ShadowCaster"}

				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_shadowcaster
				#include "UnityCG.cginc"

				struct v2f{
					V2F_SHADOW_CASTER;
				};

				v2f vert(appdata_base v){
					v2f o;
					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					return o;
				}

				float4 frag(v2f i):SV_Target{
					SHADOW_CASTER_FRAGMENT(i)
				}
				ENDCG
				}
	}
}
