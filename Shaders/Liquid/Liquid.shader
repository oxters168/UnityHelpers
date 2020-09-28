//Modified liquid shader
//Original source: https://www.patreon.com/posts/18245226

Shader "Unlit/SpecialFX/Liquid"
{
    Properties
    {
        _LiquidColor ("Liquid Color", Color) = (1,1,1,1)
		// [Space(10)] _MainTex ("Texture", 2D) = "white" {}
        _FillAmount ("Fill Amount", Range(0,1)) = 0.0
        [HideInInspector] _SizeY ("SizeY", Float) = 0.0
		[HideInInspector] _WobbleX ("WobbleX", Range(-1,1)) = 0.0
		[HideInInspector] _WobbleZ ("WobbleZ", Range(-1,1)) = 0.0
    }
 
    SubShader
    {
        Tags { "Queue"="Geometry" "DisableBatching" = "True" "RenderType" = "Transparent" }
  
        Pass
        {
            Zwrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off // we want the front and back faces
            AlphaToMask On // transparency

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                // make fog work
                // #pragma multi_compile_fog
                
                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    // float2 uv : TEXCOORD0;
                    // float3 normal : NORMAL;
                };

                struct v2f
                {
                    // float2 uv : TEXCOORD0;
                    UNITY_FOG_COORDS(1)
                    float4 vertex : SV_POSITION;
                    // float3 viewDir : COLOR;
                    // float3 normal : COLOR2;		
                    float fillEdge : TEXCOORD2;
                };

                // sampler2D _MainTex;
                // float4 _MainTex_ST;
                float _FillAmount, _WobbleX, _WobbleZ;
                float4 _LiquidColor;
                float _SizeY;
                
                float3 RotateAroundYInDegrees (float3 vertex, float degrees)
                {
                    float alpha = degrees * UNITY_PI / 180.0;
                    float sina, cosa;
                    sincos(alpha, sina, cosa);
                    float2x2 m = float2x2(cosa, sina, -sina, cosa);
                    return float3(vertex.yz, mul(m, vertex.xz).x).xzy;
                }

                v2f vert (appdata v)
                {
                    v2f o;

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    UNITY_TRANSFER_FOG(o, o.vertex);
                    // get world position of the vertex
                    float3 worldPos = mul(unity_ObjectToWorld, v.vertex.xyz); //Somehow without the w value, the output differs in that only rotation affects the values
                    // rotate it around XY
                    float3 worldPosX = RotateAroundYInDegrees(worldPos, 360);
                    // rotate around XZ
                    float3 worldPosZ = float3(worldPosX.y, worldPosX.z, worldPosX.x);
                    // combine rotations with worldPos, based on sine wave from script
                    float3 worldPosAdjusted = worldPos + (worldPosX * _WobbleX) + (worldPosZ * _WobbleZ);

                    // how high up the liquid is
                    o.fillEdge = (worldPosAdjusted.y + (_SizeY / 2.0)) / _SizeY;
                    //o.fillEdge = worldPosAdjusted.y;

                    // o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
                    // o.normal = v.normal;

                    return o;
                }
                
                //fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target
                fixed4 frag (v2f i) : SV_Target
                {
                    // sample the texture
                    // fixed4 col = tex2D(_MainTex, i.uv);
                    // apply fog
                    // UNITY_APPLY_FOG(i.fogCoord, col);
                    
                    // rest of the liquid
                    //float4 result = step(i.fillEdge, 0.5);
                    float4 result = step(i.fillEdge, _FillAmount);
                    //float4 resultColored = result * col;

                    // color of backfaces/ top
                    float4 liquidColor = _LiquidColor * result;
                    //VFACE returns positive for front facing, negative for backfacing
                    //return facing > 0 ? resultColored : topColor;
                    return liquidColor;
                    //return i.fillEdge;
                }
            ENDCG
        }
 
    }
}