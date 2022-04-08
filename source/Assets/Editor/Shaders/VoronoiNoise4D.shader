// This shader is meant to generate a Voronoi noise texture so we can Blit it
// into a render texture and save it on a png. Since it's on a shader, it uses
// the GPU and therefore is much faster than the CPU. You can use it as a
// reference on how to use it on a realtime shader for your game.

Shader "Editor/VoronoiNoiseGenerator" {

	Properties {

		_Variation ("Variation", Float) = 0
		[KeywordEnum(One, Two)] _Combination ("Combination", Float) = 0

		[Header(Noise Properties)]
		_Frequency ("Frequency", Float) = 1
		_Octaves ("Octaves", Int) = 1
		_Persistance ("Persistance", Range(0, 1)) = 0.5
		_Lacunarity ("Lacunarity", Range(0.1, 4)) = 2
		_Jitter ("Jitter", Range(0, 1)) = 1

		[Header(Noise Modifiers)]
		[HideInInspector] _NormFactor ("Normalization Factor", Float) = 1
		_RangeMin ("Range Min", Float) = 0
		_RangeMax ("Range Max", Float) = 1
		_Power ("Power", Range(1, 8)) = 1
		[Toggle(_INVERTED)] _Inverted ("Inverted", Float) = 0
	}

	SubShader {

		Pass {

			CGPROGRAM

			#pragma multi_compile _ _INVERTED
			#pragma multi_compile _COMBINATION_ONE _COMBINATION_TWO	

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			// Change this line to have it pointing to the right file.
			#include "Assets/Shaders/GPUVoronoiNoise4D.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			float _Variation;
			float _Octaves, _Frequency, _Lacunarity, _Persistance, _Jitter;
			float _NormFactor, _RangeMin, _RangeMax, _Power;

			float4 TorusMapping (float2 i) {
				float4 o = 0;
				o.x = sin(i.x * UNITY_TWO_PI);
				o.y = cos(i.x * UNITY_TWO_PI);
				o.z = sin(i.y * UNITY_TWO_PI);
				o.w = cos(i.y * UNITY_TWO_PI);
				return o;
			}

			v2f vert (appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float4 frag (v2f i) : SV_TARGET {
				float4 coords = TorusMapping(i.uv);
				float noise = 0;

				#ifdef _COMBINATION_ONE
					noise = fBm_F0(
						coords, _Variation, _Octaves, _Frequency,
						_Lacunarity, _Persistance, _Jitter);
				#endif

				#ifdef _COMBINATION_TWO
					noise = fBm_F1_F0(
						coords, _Variation, _Octaves, _Frequency,
						_Lacunarity, _Persistance, _Jitter);
				#endif
			
				noise = noise / _NormFactor;
				noise = (noise - _RangeMin) / (_RangeMax - _RangeMin);
				noise = saturate(noise);

				float k = pow(2, _Power - 1);
				noise = noise <= 0.5 ? k * pow(noise, _Power) : noise;
				noise = noise >= 0.5 ? 1 - k * pow(1 - noise, _Power) : noise;

				#ifdef _INVERTED
					noise = 1 - noise;
				#endif

				return float4(noise, noise, noise, 1);
			}

			ENDCG
		}
	}
}
