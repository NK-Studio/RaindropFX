Shader "Universal Render Pipeline/Raindrop"
{
    Properties
    {
        [Header(Main Texture)]
        _MainTex("Albedo (RGB)", 2D) = "white" {}

        [Header(Raindrop)]

        _RainDropAnimationTime("Animation Time", Range(0.01,2)) = 1
        _RaindropIntensity("Intensity", Range(0.0, 1.0)) = 0
        _RainAspect("Aspect", Vector) = (4, 1, 0, 0)
        _RainSize("Size", Range(0, 5)) = 1
        _RaindropDistortion("Distortion", Range(0, 1)) = 0.5
        _RainWobbleStrength("Wobble Strength", Range(0, 1)) = 0.5

        [Header(Static Raindrop)]
        _StaticRaindropAnimationTime("Animation Time", Range(0.01,2)) = 1
        _StaticRaindropIntensity("Intensity", Range(0.0, 1.0)) = 0
        _Amount("Amount", Int) = 2
        _StaticRainSize("Size", Range(0, 1)) = 1

        [Header(Fog)]
        [Toggle(Fog)] _EnableFog("Enable Fog", Float) = 0.0
        _FogIntensity("Intensity", Range(0.0, 1.0)) = 0.5

        [Header(Blur)]
        [Toggle(Blur)] _EnableBlur("Enable Blur", Float) = 0.0
        _BlurIntensity("Intensity", Range(0.0, 1.0)) = 0.5

        [Header(Zoom)]
        _Zoom("Zoom", Range(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags
        {
            "RenderPipline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma shader_feature_fragment Blur
            #pragma shader_feature_fragment Fog

            #define S(a, b, t) smoothstep(a, b, t)

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                int _Amount;
                float _Zoom;
                float _RaindropIntensity;
                float _StaticRaindropIntensity;
                float _FogIntensity;
                float _BlurIntensity;
                float _RainSize;
                float _StaticRainSize;
                float2 _RainAspect;
                float _RainDropAnimationTime;
                float _StaticRaindropAnimationTime;
                float _RainWobbleStrength;
                float _RaindropDistortion;
            CBUFFER_END

            struct Attribute
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // p를 기반으로 한 1D 입력에서 3D 난수 값 생성
            float3 N13(float input)
            {
                // 입력값을 이용해 p3 좌표 생성
                float3 randomCoords = frac(float3(input, input, input) * float3(0.1031, 0.11369, 0.13787));
                randomCoords += dot(randomCoords, randomCoords.yzx + 19.19);

                // 최종적으로 랜덤화된 3D 좌표 반환
                return frac(float3((randomCoords.x + randomCoords.y) * randomCoords.z,
                                   (randomCoords.x + randomCoords.z) * randomCoords.y,
                                   (randomCoords.y + randomCoords.z) * randomCoords.x));
            }

            // t를 기반으로 한 1D 입력에서 4D 난수 값 생성
            float4 N14(float time)
            {
                // 시간 기반으로 난수 생성 후 정규화
                return frac(sin(time * float4(123, 1024, 1456, 264)) * float4(6547, 345, 8799, 1564));
            }

            // t를 기반으로 한 1D 입력에서 1D 난수 값 생성
            float N(float time)
            {
                // 시간 기반 난수 생성 후 정규화
                return frac(sin(time * 12345.564) * 7658.76);
            }

            // 톱니파(Saw wave) 생성 함수
            float Saw(float boundary, float time)
            {
                // 입력값에 따라 톱니파 형태로 변화하는 값 반환
                return S(0, boundary, time) * S(1, boundary, time);
            }

            float2 DropLayer(float2 uv, float time)
            {
                float2 originalUV = uv;

                uv.y += time * _RainDropAnimationTime;

                float2 aspectRatio = _RainAspect.xy;
                float2 gridSize = aspectRatio * 2; // 그리드 크기는 고정
                float2 cellID = floor(uv * gridSize);

                float colorOffset = N(cellID.x);
                uv.y += colorOffset;

                cellID = floor(uv * gridSize);
                float3 randomValue = N13(cellID.x * 35.2 + cellID.y * 2376.1);
                float2 gridPosition = frac(uv * gridSize) - float2(0.5, 0);

                float xPos = randomValue.x - 0.5;

                float yPos = originalUV.y * 20;
                float wobble = sin(yPos + sin(yPos)) * _RainWobbleStrength;
                xPos += wobble * (0.5 - abs(xPos)) * (randomValue.z - 0.5);
                xPos *= 0.7;

                float timeOffset = frac(time * _RainDropAnimationTime + randomValue.z);
                yPos = (Saw(0.85, timeOffset) - 0.5) * 0.9 + 0.5;
                float2 dropPosition = float2(xPos, yPos);

                // 물방울 크기 조절
                float distance = length((gridPosition - dropPosition) * aspectRatio.yx) / _RainSize;
                float mainDrop = S(0.4, 0, distance);

                float radius = sqrt(S(1, yPos, gridPosition.y));
                float centerDistance = abs(gridPosition.x - xPos) / _RainSize; // 테두리 효과도 크기에 맞춰 조절
                float trailEffect = S(0.23 * radius, 0.15 * radius * radius, centerDistance);
                float trailVisibility = S(-0.02, 0.02, gridPosition.y - yPos);
                trailEffect *= trailVisibility * radius * radius;

                yPos = originalUV.y;
                yPos = frac(yPos * 10) + (gridPosition.y - 0.5);
                float dropDistance = length(gridPosition - float2(xPos, yPos)) / _RainSize; // 작은 물방울도 크기 조절
                float smallDroplets = S(0.3, 0, dropDistance);

                float finalEffect = mainDrop + smallDroplets * radius * trailVisibility;

                return float2(finalEffect, trailEffect);
            }

            float StaticDrops(float2 uv, float time)
            {
                // 물방울 크기 조정
                uv *= 40 * _StaticRainSize;

                // 현재 셀의 ID 계산
                float2 cellID = floor(uv);
                uv = frac(uv) - 0.5;

                // 셀마다 고유한 랜덤 값 생성
                float3 randomValues = N13(cellID.x * 107.45 + cellID.y * 3543.654);

                // 랜덤 값 기반으로 물방울의 위치 계산
                float2 dropPosition = (randomValues.xy - 0.5) * 0.7;

                // 물방울과 현재 위치 간의 거리 계산
                float distance = length(uv - dropPosition);

                // 물방울의 페이드 효과 설정
                float fadeEffect = Saw(0.025, frac(time * _StaticRaindropAnimationTime + randomValues.z));

                // 최종 물방울 효과 계산
                float dropEffect = S(0.3, 0, distance) * frac(randomValues.z * 10) * fadeEffect;
                return dropEffect;
            }

            float2 Drops(float2 uv, float time, float layer0Strength, float layer1Strength, float layer2Strength)
            {
                // 정적 물방울 효과 생성 및 강도 조절
                float staticDropEffect = StaticDrops(uv, time) * layer0Strength;
                staticDropEffect = lerp(0, staticDropEffect, _StaticRaindropIntensity);

                // 첫 번째 레이어의 동적 물방울 효과 생성
                float2 dynamicLayer1 = DropLayer(uv, time) * layer1Strength;

                // 두 번째 레이어의 동적 물방울 효과 생성 (배율 조절)
                float2 dynamicLayer2 = DropLayer(uv * 1.85, time) * layer2Strength;

                // 전체 물방울 효과 합산
                float combinedEffect = staticDropEffect + dynamicLayer1.x + dynamicLayer2.x;

                // 물방울의 강도에 따라 최종 조정
                combinedEffect = S(0.3, 1, combinedEffect);

                // 물방울의 흔적 효과 중 최댓값을 반환
                return float2(combinedEffect, max(dynamicLayer1.y * layer0Strength, dynamicLayer2.y * layer1Strength));
            }

            // 포그 효과를 적용하는 함수
            float3 ApplyFogEffect(float3 color, float fogIntensity, float raindropEffect)
            {
                // 포그 색상 설정 (흰색)
                float3 fogColor = float3(1, 1, 1);

                // 빗방울이 지나간 자리를 따라 포그가 잠시 닦이는 효과
                return lerp(color, fogColor, (1.0 - raindropEffect) * (fogIntensity * 0.07));
            }

            // 박스 블러를 적용하는 함수
            float3 ApplyBoxBlur(float2 uv, float focus, int sampleCount)
            {
                float3 accumulatedColor = float3(0, 0, 0);

                // 박스 블러를 위해 지정된 샘플 개수만큼 반복 샘플링
                for (int j = 1; j < sampleCount; j++)
                {
                    float2 offset = float2(j, 0) / _ScreenParams.y;
                    float4 sampledColor = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, uv + offset, focus);
                    accumulatedColor += sampledColor.rgb;
                }

                // 샘플 개수로 나누어 평균을 구합니다.
                return accumulatedColor / sampleCount;
            }

            Varyings vert(Attribute i)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                // 화면 좌표계 조정
                float2 normalizedUV = (i.uv * _ScreenParams.xy - 0.5 * _ScreenParams.xy) / _ScreenParams.y;
                float2 uv = i.uv.xy;

                float3 motion = float3(0.0, 0.0, 0.0);

                // 시간 T 계산
                float timeAdjusted = fmod(_Time.y, 102);
                timeAdjusted = lerp(timeAdjusted, motion.x * 102, motion.z > 0 ? 1 : 0);

                // 물방울 애니메이션 시간 값 조정
                float timeValue = timeAdjusted * 0.2;

                // 빗방울의 기본 강도 설정
                float rainIntensity = 6;

                // 줌 레벨 조정
                normalizedUV *= _Zoom;

                // 빗방울 각 레이어의 강도 설정
                float staticLayerIntensity = S(-0.5, 1, rainIntensity) * _Amount;
                float layer1Intensity = S(0.25, 0.75, rainIntensity);
                float layer2Intensity = S(0.0, 0.5, rainIntensity);

                // Drops 함수로 물방울 효과 생성
                float2 dropEffect = Drops(normalizedUV, timeValue, staticLayerIntensity, layer1Intensity,
                                                  layer2Intensity);

                // 물방울 방향에 따른 미세한 위치 오프셋 계산
                float2 offset = float2(0.001, 0);
                float xOffset = Drops(normalizedUV + offset, timeValue, staticLayerIntensity, layer1Intensity,
                                                      layer2Intensity).x;
                float yOffset = Drops(normalizedUV + offset.yx, timeValue, staticLayerIntensity, layer1Intensity,
                                                              layer2Intensity).x;
                float2 normalOffset = float2(xOffset - dropEffect.x, yOffset - dropEffect.x);
                normalOffset = lerp(float2(0, 0), normalOffset, _RaindropIntensity);

                // 물방울의 초점 블러 값 조정
                float minBlur = 4 + S(0.5, 1.0, 0) * 3; // 끝쪽으로 갈수록 불투명한 효과
                float maxBlur = 6 + S(0.5, 1.0, 0) * 1.5;
                float focusValue = lerp(maxBlur - dropEffect.y, minBlur, S(0.1, 0.2, dropEffect.x));

                // 텍스처 좌표 생성
                float4 textureCoords = float4(uv.x + normalOffset.x * _RaindropDistortion, uv.y + normalOffset.y * _RaindropDistortion, 0, focusValue);

                // 기본 텍스처 샘플링
                float4 baseColor = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, textureCoords.xy, 0);

                // 초기 blendedColor는 baseColor로 설정
                float4 blendedColor = baseColor;

                // Blur 효과가 활성화된 경우
                #if defined(Blur)
                const int sampleCount = 8;
                float3 blurColor = ApplyBoxBlur(textureCoords.xy, focusValue, sampleCount);
                blendedColor = lerp(baseColor, float4(blurColor, 1), (1.0 - dropEffect.y) * _BlurIntensity);
                #endif

                // Fog 효과가 활성화된 경우
                #if defined(Fog)
                blendedColor.rgb = ApplyFogEffect(blendedColor.rgb, _FogIntensity, dropEffect.x);
                #endif

                // 최종 색상 결정: Blur와 Fog 적용 여부에 따라 최종 색상 혼합
                float3 finalColor;
                #if defined(Blur) || defined(Fog)
                finalColor = lerp(baseColor.rgb, blendedColor.rgb, _RaindropIntensity);
                #else
                finalColor = baseColor.rgb;
                #endif

                return float4(finalColor, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}