//PostProcess Help: https://www.alanzucconi.com/2015/07/08/screen-shaders-and-postprocessing-effects-in-unity3d/

Shader "UnityHelpers/PostEffects/FakeObjects"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		//Tags{ "RenderType" = "Background" "Queue" = "Background" "PreviewType" = "Skybox" }
		Pass
		{
			//Tags{ "RenderType" = "Background" "Queue" = "Background" "PreviewType" = "Skybox" }

			//ZTest Always
            Cull Off
            ZWrite Off
            //Fog { Mode off }
            Blend Off

			CGPROGRAM

			#include "UnityCG.cginc"
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma require compute
			
			//Provided CPU side
            uniform float4x4 _MatrixHClipToWorld;
			uniform RWStructuredBuffer<float3> _AllSquares;
			//uniform int _AllSquaresLength; //The virtual length of _AllSquares, must be smaller than or equal to the actual length
			float epsilon = 0.001;
			float3 _CameraPos;
			sampler2D _CameraDepthTexture;
			sampler2D _MainTex;

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

			inline float RayToPlaneIntersection(float3 pointOnPlane, float3 planeNormal, float3 rayStartPos, float3 rayDir)
			{
				//Source: https://en.wikipedia.org/wiki/Line%E2%80%93plane_intersection
				return dot(pointOnPlane - rayStartPos, planeNormal) / dot(rayDir, planeNormal);
			}
			inline float3 ProjectToPlane(float3 pointOnPlane, float3 planeNormal, float3 rayStartPos)
			{
				//Source: https://stackoverflow.com/questions/9605556/how-to-project-a-point-onto-a-plane-in-3d
				float3 v = rayStartPos - pointOnPlane;
				float dist = v.x * planeNormal.x + v.y * planeNormal.y + v.z * planeNormal.z;
				return pointOnPlane - dist * planeNormal;
			}
			//Corners should be in order (clockwise or counter-clockwise) and point should be on the same plane as polygon
			//Returns 1 if in square and 0 if outside
			inline int PointInSquare(float3 pointToBeChecked, float3 bottomLeftCorner, float3 bottomRightCorner, float3 topRightCorner, float3 topLeftCorner)
			{
				//< means angle between, ikjl are the corners of the square and p is the point
				//<ipj + <jpk + <kpl + <lpi should be around 360
				//courtesy of @bmabsout
				float twopi = 6.28318;
				
				float3 ip = normalize(pointToBeChecked - bottomLeftCorner);
				float3 jp = normalize(pointToBeChecked - bottomRightCorner);
				float3 kp = normalize(pointToBeChecked - topRightCorner);
				float3 lp = normalize(pointToBeChecked - topLeftCorner);

				float ang1 = acos(dot(ip, jp));
				float ang2 = acos(dot(jp, kp));
				float ang3 = acos(dot(kp, lp));
				float ang4 = acos(dot(lp, ip));
				float sum = ang1 + ang2 + ang3 + ang4;

				return 1 - clamp(floor(abs((sum - twopi) / epsilon)), 0, 1);
				//return sum < (twopi + epsilon) && sum > (twopi - epsilon);
			}
			inline float GetSquareDepth(float3 rayStart, float3 rayDir, float3 squareTopLeftCorner, float3 squareBottomLeftCorner, float3 squareBottomRightCorner)
			{
				float3 squareRight = normalize(squareBottomRightCorner - squareBottomLeftCorner);
				float3 squareUp = normalize(squareTopLeftCorner - squareBottomLeftCorner);
				float3 squareNormal = cross(squareRight, squareUp);
				return RayToPlaneIntersection(squareBottomLeftCorner, squareNormal, rayStart, rayDir);
			}
			inline bool PointInSphere(float3 givenPoint, float3 sphereCenter, float sphereRadius)
			{
				float pointDistance = length(givenPoint - sphereCenter);
				return pointDistance <= sphereRadius;
			}
			inline int FindClosestSquare(float3 rayStart, float3 rayDir, float maxDepth)
			{
				uint squaresLength;
				uint stride = 12;
				_AllSquares.GetDimensions(squaresLength, stride); //This does work, but I don't want it to use an old buffer if it exists

				int nearestSquareIndex = -1;
				float closestDepth = maxDepth;
				float3 blCorner;
				float3 brCorner;
				float3 trCorner;
				float3 tlCorner;
				float squareNormal;
				for (int currentSquareIndex = 0; currentSquareIndex < squaresLength; currentSquareIndex += 4)
				{
					blCorner = _AllSquares[currentSquareIndex];
					brCorner = _AllSquares[currentSquareIndex + 1];
					trCorner = _AllSquares[currentSquareIndex + 2];
					tlCorner = _AllSquares[currentSquareIndex + 3];
					float squareDepth = GetSquareDepth(rayStart, rayDir, tlCorner, blCorner, brCorner);
					float3 pointOnPlane = rayStart + (rayDir * squareDepth);

					int backsideLerp = clamp(ceil(squareDepth), 0, 1);
					int closestLerp = clamp(ceil(closestDepth - squareDepth), 0, 1);
					int inSquareLerp = PointInSquare(pointOnPlane, blCorner, brCorner, trCorner, tlCorner);
					nearestSquareIndex = lerp(nearestSquareIndex, currentSquareIndex, backsideLerp * closestLerp * inSquareLerp);
					closestDepth = lerp(closestDepth, squareDepth, backsideLerp * closestLerp * inSquareLerp);
				}
				return nearestSquareIndex;
			}

			fixed4 frag(v2f_img i) : COLOR
			{
				//_ProjectionParams.y near clip plane
				//_ProjectionParams.z far clip plane

				fixed4 finalColor = tex2D(_MainTex, i.uv);
				//fixed4 originalColor = tex2D(_MainTex, i.uv);
				//finalColor = originalColor;

				float worldDepth = tex2D(_CameraDepthTexture, i.uv).r;
				worldDepth = Linear01Depth(worldDepth);
				worldDepth = worldDepth * _ProjectionParams.z;

				float3 startPosition = TransformUVToWorldPos(i.uv, _ProjectionParams.y);
				float3 rayDir = normalize(startPosition - _CameraPos);
				
				int squareIndex = FindClosestSquare(startPosition, rayDir, worldDepth);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				fixed4 squareColor = dot(-rayDir, lightDirection);
				int colorLerp = clamp(ceil(squareIndex), 0, 1);
				finalColor = lerp(finalColor, squareColor, colorLerp);

				return finalColor;
			}
            ENDCG
		}
	}
}