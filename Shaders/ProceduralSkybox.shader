// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

//Source:
//https://github.com/keijiro/UnitySkyboxShaders/blob/master/Assets/Skybox%20Shaders/Horizontal%20Skybox.shader

Shader "UnityHelpers/Skybox/Procedural Skybox"
{
	Properties
	{
		_Color1("Top Color", Color) = (1, 1, 1, 0)
		_Color2("Horizon Color", Color) = (1, 1, 1, 0)
		_Color3("Bottom Color", Color) = (1, 1, 1, 0)
		_Exponent1("Exponent Factor for Top Half", Float) = 1.0
		_Exponent2("Exponent Factor for Bottom Half", Float) = 1.0
		_Intensity("Intensity Amplifier", Float) = 1.0

        _SkyboxRadius("Skybox Radius", Float) = 1000 //Doesn't really make a difference in the outcome, might remove later

        _StarSpread("Star Spread", Range(0, 1)) = 0.1
        _StarMoveMultiplier("Star Follow Camera", Float) = 0

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

			half4 _Color1;
			half4 _Color2;
			half4 _Color3;
			half _Intensity;
			half _Exponent1;
			half _Exponent2;

			float _StarSpread;
            float _StarMoveMultiplier;
            float _SkyboxRadius;

            half4 _NebulaColor;
            float _NebulaMoveMultiplier;
			float _NebulaNoiseValue;
            float _NebulaShrink;
            half3 _NebulaVelocity;

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
                
                float3 pointDirection = normalize(i.worldPos);

                //Calculate nebula value
				float nebulaNoiseValue = _NebulaNoiseValue / _SkyboxRadius;
                float nebulaValue = ((snoise((pointDirection * _SkyboxRadius) * nebulaNoiseValue + (_WorldSpaceCameraPos * _NebulaMoveMultiplier + (_NebulaVelocity.xyz * _Time.y))) + 1) / 2);
                if (_NebulaShrink < 1)
                {
                    nebulaValue = (clamp(nebulaValue, _NebulaShrink, 1) - _NebulaShrink) / (1 - _NebulaShrink);
                }
                else
                {
                    nebulaValue = 0;
                }

				//Calculate star value
                float starNoiseValue = 40 / _SkyboxRadius; //0.004 goodrange=>(0.001 - 0.008)
                float starValue = (snoise((pointDirection * _SkyboxRadius) * starNoiseValue + (_WorldSpaceCameraPos * _StarMoveMultiplier)) + 1) / 2;
                if (starValue > (1 - _StarSpread))
                {
                    finalColor = 1;
				}
				else
                {
                    //Calculate sky color
					float p = normalize(i.texcoord).y;
					float p1 = 1.0f - pow(min(1.0f, 1.0f - p), _Exponent1);
					float p3 = 1.0f - pow(min(1.0f, 1.0f + p), _Exponent2);
					float p2 = 1.0f - p1 - p3;
					half4 skyColor = (_Color1 * p1 + _Color2 * p2 + _Color3 * p3) * _Intensity;

                    //Overlay nebula on top of sky
                    finalColor = lerp(skyColor, _NebulaColor * nebulaValue, nebulaValue);
				}

                return finalColor;
			}
			ENDCG
		}
	}
}