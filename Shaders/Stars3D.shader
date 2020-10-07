// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/SpecialFX/Stars3D"
{
    Properties
    {
        _Scale ("Scale", Float) = 0.0
        _Mult1 ("Mult1", Float) = 0.0
        _Mult2 ("Mult2", Float) = 0.0

        _Size ("Size", Float) = 1
    }
    SubShader
    {
        //Tags { "RenderType"="Opaque" }
        // Tags{ "RenderType" = "Background" "Queue" = "Background" "PreviewType" = "Skybox" }
        Tags { "RenderType"="Transparent" "Queue"="Geometry" "PreviewType" = "Skybox" }
        Blend SrcAlpha OneMinusSrcAlpha
        AlphaToMask On // transparency
        // LOD 100

        Pass
        {
			// ZWrite Off
			Cull Off
			Fog { Mode Off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "SimplexNoise3D.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 dir : TEXCOORD2;
                float3 dir2 : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Scale;
            float _Mult1;
            float _Mult2;
            float _Size;
            
            float4 AxisAngle(float3 axis, float angle)
            {
                float half_angle = angle / 2;
                float4 q = float4
                (
                    axis.x * sin(half_angle),
                    axis.y * sin(half_angle),
                    axis.z * sin(half_angle),
                    cos(half_angle)
                );
                return q;
            }
            float4 Inverse(float4 q)
            {
                float norm = sqrt(q.x*q.x + q.y*q.y + q.z*q.z + q.w*q.w);
                return float4(-q.x, -q.y, -q.z, q.w) / norm;
            }
            float4 Multiply(float4 q1, float4 q2)
            {
                return float4
                (
                    (q1.w * q2.x) + (q1.x * q2.w) + (q1.y * q2.z) - (q1.z * q2.y),
                    (q1.w * q2.y) - (q1.x * q2.z) + (q1.y * q2.w) + (q1.z * q2.x),
                    (q1.w * q2.z) + (q1.x * q2.y) - (q1.y * q2.x) + (q1.z * q2.w),
                    (q1.w * q2.w) - (q1.x * q2.x) - (q1.y * q2.y) - (q1.z * q2.z)
                );
            }
            float4 Identity()
            {
                return float4(0, 0, 0, 1);
            }
            float3 MultiplyVector(float3 v, float4 q)
            {
                float4 qr_conj = Inverse(q);
                float4 q_tmp = Multiply(q, float4(v, 0));
                return Multiply(q_tmp, qr_conj).xyz;
            }
            float3 MultiplyVectorDirect(float3 v, float4 q)
            {
                float3 t = 2 * cross(q.xyz, v);
                return v + q.w * t + cross(q.xyz, t);
            }
            float3 RotateVector(float3 position, float3 axis, float angle)
            { 
                float4 qr = AxisAngle(axis, angle);

                return MultiplyVector(position, qr);
            }

            //Matrices source: https://upload.wikimedia.org/math/2/8/5/2851c9dc2031127e6dacfb84b96446d8.png
            float3 RotateAroundXInDegrees (float3 vertex, float degrees)
            {
                const float Deg2Rad = UNITY_PI / 180;
                float alpha = degrees * Deg2Rad;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float4x4 m = float4x4
                (
                    1,  0,      0,      0,
                    0,  cosa,   -sina,  0,
                    0,  sina,   cosa,   0,
                    0,  0,      0,      1
                );
                return mul(m, float4(vertex, 0)).xyz;
            }
            float3 RotateAroundYInDegrees (float3 vertex, float degrees)
            {
                const float Deg2Rad = UNITY_PI / 180;
                float alpha = degrees * Deg2Rad;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float4x4 m = float4x4
                (
                    cosa,   0,  sina,  0,
                    0,      1,  0,     0,
                    -sina,  0,  cosa,  0,
                    0,      0,  0,     1
                );
                return mul(m, float4(vertex, 0)).xyz;
            }
            float3 RotateAroundZInDegrees (float3 vertex, float degrees)
            {
                const float Deg2Rad = UNITY_PI / 180;
                float alpha = degrees * Deg2Rad;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float4x4 m = float4x4
                (
                    cosa,  -sina,  0,  0,
                    sina,  cosa,   0,  0,
                    0,     0,      1,  0,
                    0,     0,      0,  1
                );
                return mul(m, float4(vertex, 0)).xyz;
            }
            float3 RotateAroundXYInDegrees (float3 vertex, float xAngle, float yAngle)
            {
                const float Deg2Rad = UNITY_PI / 180;
                //xRot = rotation along x axis
                float alpha = yAngle * Deg2Rad;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                //yRot = rotation along y axis
                float gamma = xAngle * Deg2Rad;
                float sing, cosg;
                sincos(gamma, sing, cosg);

                //yRot * xRot
                // float4x4 m = float4x4
                // (
                //     cosg,   sing * sina,  sing * cosa,  0,
                //     0,      cosa,         -sina,        0,
                //     -sing,  cosg * sina,  cosg * cosa,  0,
                //     0,      0,            0,            1
                // );
                //xRot * yRot
                float4x4 m = float4x4
                (
                    cosg,           0,     sing,          0,
                    -sina * -sing,  cosa,  -sina * cosg,  0,
                    cosa * -sing,   sina,  cosa * cosg,   0,
                    0,              0,     0,             1
                );

                return mul(m, float4(vertex, 0)).xyz;
            }
            float4x4 QuatToMat(float4 q)
            {
                float norm = sqrt(q.x*q.x + q.y*q.y + q.z*q.z + q.w*q.w);
                float4 nq = q / norm;
                return float4x4
                (
                    1 - 2 * nq.y * nq.y - 2 * nq.z * nq.z, 2 * nq.x * nq.y - 2 * nq.z * nq.w, 2 * nq.x * nq.z + 2 * nq.y * nq.w, 0,
                    2 * nq.x * nq.y + 2 * nq.z * nq.w, 1 - 2 * nq.x * nq.x - 2 * nq.z * nq.z, 2 * nq.y * nq.z - 2 * nq.x * nq.w, 0,
                    2 * nq.x * nq.z - 2 * nq.y * nq.w, 2 * nq.y * nq.z + 2 * nq.x * nq.w, 1 - 2 * nq.x * nq.x - 2 * nq.y * nq.y, 0,
                    0, 0, 0, 1
                );
            }
            // float3 RotateAroundXInDegrees (float3 vertex, float degrees)
            // {
            //     const float Deg2Rad = UNITY_PI / 180;
            //     float alpha = degrees * Deg2Rad;
            //     float sina, cosa;
            //     sincos(alpha, sina, cosa);
            //     float3x3 m = float3x3
            //     (
            //         1,  0,      0,
            //         0,  cosa,   sina,
            //         0,  -sina,  cosa
            //     );
            //     return mul(m, vertex);
            // }
            // float3 RotateAroundYInDegrees (float3 vertex, float degrees)
            // {
            //     const float Deg2Rad = UNITY_PI / 180;
            //     float alpha = degrees * Deg2Rad;
            //     float sina, cosa;
            //     sincos(alpha, sina, cosa);
            //     float3x3 m = float3x3
            //     (
            //         cosa,   0,  sina,
            //         0,      1,  0,
            //         -sina,  0,  cosa
            //     );
            //     return mul(m, vertex);
            // }
            // float3 RotateAroundXYInDegrees (float3 vertex, float xAngle, float yAngle)
            // {
            //     const float Deg2Rad = UNITY_PI / 180;
            //     //xRot = rotation along x axis
            //     float alpha = yAngle * Deg2Rad;
            //     float sina, cosa;
            //     sincos(alpha, sina, cosa);
            //     //yRot = rotation along y axis
            //     float gamma = xAngle * Deg2Rad;
            //     float sing, cosg;
            //     sincos(gamma, sing, cosg);

            //     //yRot * xRot
            //     float3x3 m = float3x3
            //     (
            //         cosg,   sing * -sina,  sing * cosa,
            //         0,      cosa,          sina,
            //         -sing,  cosg * -sina,  cosg * cosa
            //     );
            //     //xRot * yRot
            //     // float3x3 m = float3x3
            //     // (
            //     //     cosg,          0,      sing,
            //     //     sina * -sing,  cosa,   sina * cosg,
            //     //     cosa * -sing,  -sina,  cosa * cosg
            //     // );

            //     return mul(m, vertex);
            // }

            ////////////////// BEGIN QUATERNION FUNCTIONS //////////////////

            float PI = 3.1415926535897932384626433832795;
            
            float4 setAxisAngle (float3 axis, float rad) {
              rad = rad * 0.5;
              float s = sin(rad);
              return float4(s * axis[0], s * axis[1], s * axis[2], cos(rad));
            }
            
            float3 xUnitVec3 = float3(1.0, 0.0, 0.0);
            float3 yUnitVec3 = float3(0.0, 1.0, 0.0);
            
            float4 rotationTo (float3 a, float3 b) {
              float vecDot = dot(a, b);
              float3 tmpvec3 = float3(0, 0, 0);
              if (vecDot < -0.999999) {
                tmpvec3 = cross(xUnitVec3, a);
                if (length(tmpvec3) < 0.000001) {
                  tmpvec3 = cross(yUnitVec3, a);
                }
                tmpvec3 = normalize(tmpvec3);
                return setAxisAngle(tmpvec3, PI);
              } else if (vecDot > 0.999999) {
                return float4(0,0,0,1);
              } else {
                tmpvec3 = cross(a, b);
                float4 _out = float4(tmpvec3[0], tmpvec3[1], tmpvec3[2], 1.0 + vecDot);
                return normalize(_out);
              }
            }
            
            float4 multQuat(float4 q1, float4 q2) {
              return float4(
                q1.w * q2.x + q1.x * q2.w + q1.z * q2.y - q1.y * q2.z,
                q1.w * q2.y + q1.y * q2.w + q1.x * q2.z - q1.z * q2.x,
                q1.w * q2.z + q1.z * q2.w + q1.y * q2.x - q1.x * q2.y,
                q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z
              );
            }
            
            float3 rotateVector( float4 quat, float3 vec ) {
              // https://twistedpairdevelopment.wordpress.com/2013/02/11/rotating-a-vector-by-a-quaternion-in-glsl/
              float4 qv = multQuat( quat, float4(vec, 0.0) );
              return multQuat( qv, float4(-quat.x, -quat.y, -quat.z, quat.w) ).xyz;
            }
            
            ////////////////// END QUATERNION FUNCTIONS //////////////////

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.dir = normalize(v.vertex);
                return o;
            }

            float3 GetDir (float2 vertex)
            {
                float2 pixelPos = float2(vertex.x / _ScreenParams.x, vertex.y / _ScreenParams.y);
                float lerpedX = pixelPos.x * 2 - 1;
                float lerpedY = pixelPos.y * 2 - 1;

                float aspect = _ScreenParams.x / _ScreenParams.y;
                float t = unity_CameraProjection._m11;
                const float Rad2Deg = 180 / UNITY_PI;
                float vertFOVRad = atan(1 / t); //Already half the angle, no need to divide by 2
                float horFOVRad = 2 * atan(tan(vertFOVRad / 2) * aspect);
                float vertFOV = vertFOVRad * Rad2Deg;
                float horFOV = horFOVRad * Rad2Deg;

                // float3 cameraForward = -normalize(UNITY_MATRIX_IT_MV[2].xyz);
                float3 cameraForward = mul(unity_CameraToWorld, float3(0, 0, 1)) - _WorldSpaceCameraPos;
                // float3 pixelDir = RotateAroundXInDegrees(cameraForward, -lerpedY * vertFOV);
                // pixelDir = RotateAroundYInDegrees(pixelDir, lerpedX * horFOV);
                // float3 pixelDir = RotateAroundYInDegrees(cameraForward, lerpedX * horFOV);
                // pixelDir = RotateAroundXInDegrees(pixelDir, -lerpedY * vertFOV);
                // float3 pixelDir = RotateAroundXYInDegrees(cameraForward, lerpedX * horFOV, -lerpedY * vertFOV);
                //pixelDir = cameraForward;

                // These ones only seem to work in ortho
                // float3 pixelDir = RotateVector(cameraForward, float3(1, 0, 0), lerpedY * vertFOV);
                // pixelDir = RotateVector(pixelDir, float3(0, 1, 0), lerpedX * horFOV);
                // float3 pixelDir = RotateVector(cameraForward, float3(0, 1, 0), lerpedX * horFOV);
                // pixelDir = RotateVector(pixelDir, float3(1, 0, 0), lerpedY * vertFOV);

                float Deg2Rad = UNITY_PI / 180;
                float4 xQuat = AxisAngle(float3(1, 0, 0), lerpedY * vertFOV * Deg2Rad);
                float4 yQuat = AxisAngle(float3(0, 1, 0), -lerpedX * horFOV * Deg2Rad);
                // float4 xQuat = setAxisAngle(float3(1, 0, 0), lerpedY * vertFOV * Deg2Rad);
                // float4 yQuat = setAxisAngle(float3(0, 1, 0), -lerpedX * horFOV * Deg2Rad);
                // float4 rotQuat = Multiply(xQuat, yQuat);
                // float4 rotQuat = Multiply(yQuat, xQuat);
                // float3 pixelDir = rotateVector(rotQuat, cameraForward);
                float3 pixelDir = rotateVector(yQuat, cameraForward);
                pixelDir = rotateVector(xQuat, pixelDir);
                // float4x4 m = QuatToMat(rotQuat);
                // float3 pixelDir = MultiplyVectorDirect(cameraForward, rotQuat);
                // float4x4 m = QuatToMat(xQuat);
                // float3 pixelDir = mul(m, cameraForward);
                // m = QuatToMat(yQuat);
                // pixelDir = mul(m, pixelDir);
                // float3 pixelDir = MultiplyVector(cameraForward, rotQuat);
                // pixelDir = mul((float3x3)unity_CameraToWorld, pixelDir);

                return pixelDir;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);
                // float3 dir = GetDir(i.vertex.xy);
                // float color = 0;
                // for (int t = 0; t < 100; t++)
                // {
                //     float3 pos = _WorldSpaceCameraPos + (dir * t) / t * _Size;
                //     color = ((snoise(pos * _Mult1) + 1) / 2) * _Mult2;
                //     if (color > _Scale)
                //     {
                //         break;
                //     }
                // }
                // return color;
                // return fixed4(dir, 1);

                float2 uv = normalize(UnityObjectToClipPos(mul(unity_WorldToObject, dir)).xy);
                // float2 uv = float2(i.vertex.x / _ScreenParams.x, i.vertex.y / _ScreenParams.y);
                return fixed4(uv, 0, 1);
            }
            ENDCG
        }
    }
}
