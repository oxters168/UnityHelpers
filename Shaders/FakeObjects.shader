//PostProcess Help: https://www.alanzucconi.com/2015/07/08/screen-shaders-and-postprocessing-effects-in-unity3d/

Shader "UnityHelpers/PostEffects/FakeObjects"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}

        _SquareBottomLeftCorner("Square Bottom Left Corner", Vector) = (0, 0, 0)
        _SquareBottomRightCorner("Square Bottom Right Corner", Vector) = (1, 0, 0)
		_SquareTopRightCorner("Square Top Right Corner", Vector) = (0, 1, 0)
        _SquareTopLeftCorner("Square Top Left Corner", Vector) = (0, 0, 0)
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

            float3 _SquareBottomLeftCorner;
            float3 _SquareBottomRightCorner;
			float3 _SquareTopRightCorner;
			float3 _SquareTopLeftCorner;

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
				//Equation of plane: https://web.ma.utexas.edu/users/m408m/Display12-5-3.shtml
				//Equation of line: http://sites.science.oregonstate.edu/math/home/programs/undergrad/CalculusQuestStudyGuides/vcalc/lineplane/lineplane.html
				//Intersection equation: https://math.libretexts.org/Bookshelves/Calculus/Supplemental_Modules_(Calculus)/Multivariable_Calculus/1%3A_Vectors_in_Space/Intersection_of_a_Line_and_a_Plane

				float coefficient;

				float planeNumber = planeNormal.x * pointOnPlane.x + planeNormal.y * pointOnPlane.y + planeNormal.z * pointOnPlane.z;

				float3 otherDistribution = planeNormal * rayStartPos;
				float3 varDistribution = planeNormal * rayDir;
				float otherSum = otherDistribution.x + otherDistribution.y + otherDistribution.z;
				float varCoefficient = varDistribution.x + varDistribution.y + varDistribution.z;
				coefficient = -(otherSum + planeNumber) / varCoefficient; //Apparently it's ok to divide by zero, it will return a large number
				
				return coefficient;
			}
			//Corners should be in order (clockwise or counter-clockwise)
			inline bool PointInSquare(float3 pointToBeChecked, float3 bottomLeftCorner, float3 bottomRightCorner, float3 topRightCorner, float3 topLeftCorner)
			{
				//< means angle between, ikjl are the corners of the square and p is the point
				//<ipj + <jpk + <kpl + <lpi should be around 360
				//courtesy of @bmabsout
				float twopi = 6.28318;
				//float epsilon = 0.00001; //Causes weird design due to floating point problems
				float epsilon = 0.001;

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

			fixed4 frag(v2f_img i) : COLOR
			{
				//_ProjectionParams.y near clip plane
				//_ProjectionParams.z far clip plane

				fixed4 finalColor = 1;

				fixed4 originalColor = tex2D(_MainTex, i.uv);

				float worldDepth = tex2D(_CameraDepthTexture, i.uv).r;
				worldDepth = Linear01Depth(worldDepth);
				worldDepth = worldDepth * _ProjectionParams.z;

				float3 startPosition = TransformUVToWorldPos(i.uv, _ProjectionParams.y);
				float3 rayDir = normalize(startPosition - _CameraPos);

				float3 squareRight = normalize(_SquareBottomRightCorner - _SquareBottomLeftCorner);
				float3 squareUp = normalize(_SquareTopLeftCorner - _SquareBottomLeftCorner);
				float3 squareNormal = cross(squareRight, squareUp);
				float squareDepth = RayToPlaneIntersection(_SquareBottomLeftCorner, squareNormal, startPosition, rayDir);

				finalColor = originalColor;
				if(squareDepth >= 0 && squareDepth < worldDepth)
				{
					float3 pointOnPlane = startPosition + (rayDir * squareDepth);
					if (PointInSquare(pointOnPlane, _SquareBottomLeftCorner, _SquareBottomRightCorner, _SquareTopRightCorner, _SquareTopLeftCorner))\
					{
						float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
						finalColor = dot(-rayDir, lightDirection);
					}
				}

				return finalColor;
			}
            ENDCG
		}
	}
}