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

            uniform float4x4 _MatrixHClipToWorld;

			uniform float4 _AllSquares[30]; //5 * 6 (blCorner, brCorner, trCorner, tlCorner, color...)
			//uniform float4 _AllSquares[5];

			float _currentSquareDepth; //Until I find a better solution, this'll have to do
			float _currentAngleSum;
			float radius;
			float3 _currentSquareColor;

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

			inline float RayToPlaneIntersection2(float3 pointOnPlane, float3 planeNormal, float3 rayStartPos, float3 rayDir)
			{
				//Equation of plane: https://web.ma.utexas.edu/users/m408m/Display12-5-3.shtml
				//Equation of line: http://sites.science.oregonstate.edu/math/home/programs/undergrad/CalculusQuestStudyGuides/vcalc/lineplane/lineplane.html
				//Intersection equation: https://math.libretexts.org/Bookshelves/Calculus/Supplemental_Modules_(Calculus)/Multivariable_Calculus/1%3A_Vectors_in_Space/Intersection_of_a_Line_and_a_Plane

				float coefficient;

				float planeNumber = planeNormal.x * -pointOnPlane.x + planeNormal.y * -pointOnPlane.y + planeNormal.z * -pointOnPlane.z;

				float3 otherDistribution = planeNormal * rayStartPos;
				float3 varDistribution = planeNormal * rayDir;
				float otherSum = otherDistribution.x + otherDistribution.y + otherDistribution.z;
				float varCoefficient = varDistribution.x + varDistribution.y + varDistribution.z;
				coefficient = -(otherSum + planeNumber) / varCoefficient; //Apparently it's ok to divide by zero, it will return a large number
				
				return coefficient;
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
			inline bool PointInSquare(float3 pointToBeChecked, float3 bottomLeftCorner, float3 bottomRightCorner, float3 topRightCorner, float3 topLeftCorner)
			{
				//< means angle between, ikjl are the corners of the square and p is the point
				//<ipj + <jpk + <kpl + <lpi should be around 360
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
				
				return sum < (twopi + epsilon) && sum > (twopi - epsilon);
			}
			inline float GetSquareDepth(float3 rayStart, float3 rayDir, float3 squareTopLeftCorner, float3 squareBottomLeftCorner, float3 squareBottomRightCorner)
			{
				float3 squareRight = normalize(squareBottomRightCorner - squareBottomLeftCorner);
				float3 squareUp = normalize(squareTopLeftCorner - squareBottomLeftCorner);
				float3 squareNormal = cross(squareRight, squareUp);
				_currentSquareColor = squareNormal;
				return RayToPlaneIntersection(squareBottomLeftCorner, squareNormal, rayStart, rayDir);
			}
			inline bool PointInSphere(float3 givenPoint, float3 sphereCenter, float sphereRadius)
			{
				float pointDistance = length(givenPoint - sphereCenter);
				return pointDistance <= sphereRadius;
			}
			inline int FindClosestSquare(float3 rayStart, float3 rayDir, float maxDepth)
			{
				int nearestSquareIndex = -1;
				float closestDepth = maxDepth;
				float3 blCorner;
				float3 brCorner;
				float3 trCorner;
				float3 tlCorner;
				float squareNormal;
				for (int currentSquareIndex = 0; currentSquareIndex < 30; currentSquareIndex += 5)
				{
					blCorner = _AllSquares[currentSquareIndex];
					brCorner = _AllSquares[currentSquareIndex + 1];
					trCorner = _AllSquares[currentSquareIndex + 2];
					tlCorner = _AllSquares[currentSquareIndex + 3];
					squareNormal = _AllSquares[currentSquareIndex + 4];
					//float3 squareCenter = (blCorner + brCorner + trCorner + tlCorner) / 4;
					float squareDepth = GetSquareDepth(rayStart, rayDir, tlCorner, blCorner, brCorner);
					//float squareDepth = RayToPlaneIntersection(blCorner, squareNormal, rayStart, rayDir);
					if (squareDepth >= 0 && squareDepth < closestDepth)
					{
						float3 pointOnPlane = rayStart + (rayDir * squareDepth);
						if (PointInSquare(pointOnPlane, blCorner, brCorner, trCorner, tlCorner))
						//if (PointInSphere(pointOnPlane, squareCenter, radius))
						{
							closestDepth = squareDepth;
							_currentSquareDepth = squareDepth;
							nearestSquareIndex = currentSquareIndex;
						}
					}
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
				//_currentSquareDepth = worldDepth;

				float3 startPosition = TransformUVToWorldPos(i.uv, _ProjectionParams.y);
				float3 rayDir = normalize(startPosition - _CameraPos);
				
				//finalColor = 1;
				//int squareIndex = 5;
				int squareIndex = FindClosestSquare(startPosition, rayDir, worldDepth);
				if (squareIndex >= 0)
				{
					//float3 bottomLeftCorner = _AllSquares[squareIndex];
					//float3 bottomRightCorner = _AllSquares[squareIndex + 1];
					//float3 topRightCorner = _AllSquares[squareIndex + 2];
					//float3 topLeftCorner = _AllSquares[squareIndex + 3];
					//float3 bottomLeftCorner = float3(-0.5, -0.5, 0);
					//float3 bottomRightCorner = float3(0.5, -0.5, 0);
					//float3 topRightCorner = float3(0.5, 0.5, 0);
					//float3 topLeftCorner = float3(-0.5, 0.5, 0);


					//float squareDepth = GetSquareDepth(startPosition, rayDir, topLeftCorner, bottomLeftCorner, bottomRightCorner);

					//if(squareDepth >= 0 && squareDepth < worldDepth)
					//{
						//float3 pointOnPlane = startPosition + (rayDir * _currentSquareDepth);
						//if (PointInSquare(pointOnPlane, bottomLeftCorner, bottomRightCorner, topRightCorner, topLeftCorner))
						//{
							float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
							finalColor = dot(-rayDir, lightDirection);
						//}
						//else
						//	finalColor = 0;
					//}
					//finalColor = 1;
					//float3 summed = (bottomLeftCorner + bottomRightCorner + topRightCorner + topLeftCorner) / 4;
					//finalColor = fixed4(summed.r, summed.g, summed.b, 1);
					//finalColor = _currentAngleSum / 6.28318;
					//finalColor = _currentSquareDepth / 10;
					//finalColor = _AllSquares[squareIndex + 4];
					//finalColor = fixed4(rayDir.r, rayDir.g, rayDir.b, 1);
					//finalColor = fixed4(_currentSquareColor.r, _currentSquareColor.g, _currentSquareColor.b, 1);
				}
				//else
				//{
				//	finalColor = 0;
				//}
				//finalColor = _currentSquareDepth / 10;
				//finalColor = squareIndex;
				//finalColor = fixed4(startPosition.r, startPosition.g, startPosition.b, 1);
				//finalColor = fixed4(rayDir.r, rayDir.g, rayDir.b, 1);
				return finalColor;
			}
            ENDCG
		}
	}
}