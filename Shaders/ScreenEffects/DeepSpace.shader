// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "UnityHelpers/PostEffects/Deep Space"
{
	Properties
	{
        _SkyboxRadius("Skybox Radius", Float) = 1000 //Doesn't really make a difference in the outcome, might remove later

        _NebulaColor("Nebula Color", Color) = (1, 1, 1, 1)
        _NebulaMoveMultiplier("Nebula Follow Camera", Float) = 0
		_NebulaNoiseValue("Nebula Noise Value", Float) = 0.0001
        _NebulaShrink("Nebula Shrink", Range(0, 1)) = 0
        _NebulaVelocity("Move Speed", Vector) = (0, 0, 0)
	}

	SubShader
	{
		Tags{ "RenderType" = "Background" "Queue" = "Background" "PreviewType" = "Skybox" }
		Pass
		{
			Tags{ "RenderType" = "Background" "Queue" = "Background" "PreviewType" = "Skybox" }
			ZWrite Off
			Cull Off
			Fog { Mode Off }

			CGPROGRAM

			#include "UnityCG.cginc"
			#include "SimplexNoise3D.hlsl"
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 position : POSITION;
				float3 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float3 texcoord : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
			};

            uniform float4x4 _MatrixHClipToWorld;

            float _SkyboxRadius;

            half4 _NebulaColor;
            float _NebulaMoveMultiplier;
			float _NebulaNoiseValue;
            float _NebulaShrink;
            half3 _NebulaVelocity;

            //Source: https://stackoverflow.com/questions/39935774/unity-shaderlab-transform-from-screen-space-to-world-space
            inline half3 TransformUVToWorldPos(half2 uv, float depth)
            {
                #ifndef SHADER_API_GLCORE
                    half4 positionCS = half4(uv * 2 - 1, depth, 1) * LinearEyeDepth(depth);
                #else
                    half4 positionCS = half4(uv * 2 - 1, depth * 2 - 1, 1) * LinearEyeDepth(depth);
                #endif
                return mul(_MatrixHClipToWorld, positionCS).xyz;
            }

            inline float GetNoiseValueAt(half3 atPos)
            {
				float nebulaNoiseValue = _NebulaNoiseValue;
                return ((snoise((atPos) * _NebulaNoiseValue + ((_NebulaVelocity.xyz * _Time.y))) + 1) / 2);
            }
            inline float Raycast(half2 value2D, float minStepDist, float startDist, float endDistance)
            {
                float value = 0;
                float currentDepth = startDist;
                half3 currentPos;
                int count = 0;
                while (currentDepth < endDistance)
                {
                    currentPos = TransformUVToWorldPos(value2D, currentDepth);
                    value += GetNoiseValueAt(currentPos);
                    currentDepth += minStepDist;
                    count += 1;
                }
                if (count > 0)
                {
                    value /= count;
                }
                return value;
            }

			v2f vert(appdata v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.position);
				o.texcoord = v.texcoord;

				o.worldPos = mul(unity_ObjectToWorld, v.position);
                o.viewDir = normalize(WorldSpaceViewDir(v.position));

				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				half4 finalColor = 0;
                
                //float3 pointDirection = normalize(i.worldPos);

                //half3 currentPos = half3(0, 0, 0);
                //Calculate nebula value
                //half3 currentPos = TransformUVToWorldPos(half2(i.texcoord.x, i.texcoord.y), _SkyboxRadius);
                //float nebulaValue = ((snoise((currentPos) * _NebulaNoiseValue + ((_NebulaVelocity.xyz * _Time.y))) + 1) / 2);
                //float nebulaValue = GetNoiseValueAt(currentPos);
                float returnedValue = Raycast(half2(i.texcoord.x, i.texcoord.y), 1, _SkyboxRadius, _NebulaMoveMultiplier);
                float nebulaValue = returnedValue;
                //float nebulaValue = pow(returnedValue, 5);
                if (_NebulaShrink < 1)
                {
                    nebulaValue = (clamp(nebulaValue, _NebulaShrink, 1) - _NebulaShrink) / (1 - _NebulaShrink);
                }
                else
                {
                    nebulaValue = 0;
                }

                //finalColor = half4((i.texcoord.x + i.texcoord.y) / 2, 0, 0, 0);
                //finalColor = half4(i.worldPos.x, i.worldPos.y, i.worldPos.z, 1);
                //finalColor = UnityObjectToClipPos(i.worldPos);
                finalColor = _NebulaColor * nebulaValue;
                //finalColor = half4(currentPos.x, currentPos.y, currentPos.z, 1);
                //finalColor = half4((currentPos.x + currentPos.y + currentPos.z) / 3, 0, 0, 0);

                return finalColor;
			}
            ENDCG
		}
	}
}