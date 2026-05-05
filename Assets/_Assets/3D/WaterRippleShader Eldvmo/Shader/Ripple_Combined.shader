Shader "Custom/Ripple_Combined"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _Texture("Texture", 2D) = "white"{}
        _Decay("Decay", Range(0, 20)) = 5
        _WaveLiftTime("Wave Life Time", Range(1, 10)) = 2
        _WaveFrequency("Wave Frequency", Range(0, 100)) = 25
        _WaveSpeed("Wave Speed", Range(0, 10)) = 0.1
        _WaveStrength("Wave Strength", Range(0, 5)) = 0.5
        _WorldScale("World Scale", Float) = 1
        _StencilRef("Stencil Ref", Range(0, 255)) = 1
        _BoatWaveSpeed("Boat Wave Speed", Range(0,20)) = 8
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Stencil
            {
                ref [_StencilRef]
comp Equal
                pass replace
            }

            CGPROGRAM
# include "UnityCG.cginc"

#pragma vertex vert
#pragma fragment frag

            sampler2D _Texture;
float4 _Texture_ST;
float4 _Color;

float _Decay;
float _WaveLiftTime;
float _WaveFrequency;
float _WaveSpeed;
float _WaveStrength;
float _WorldScale;
float _BoatWaveSpeed;

float4 _InputCentre[10];
float4 _ContinuousCentre;

struct VertexInput
{
    float4 pos : POSITION;
                float2 uv  : TEXCOORD0;
            };

struct VertexOutput
{
    float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float2 rawUV : TEXCOORD1;
            };


float Wave(float2 uv, float2 centre, float startTime)
{
    if (startTime < 0) return 0;

    float age = _Time.y - startTime;
    if (age < 0 || age > _WaveLiftTime) return 0;

    float2 offset = uv - centre;
    float distanceFromCentre = length(offset) / _WorldScale;

    float rippleRadius = age * _WaveSpeed;

    float wave = 1.0 - abs(distanceFromCentre - rippleRadius) * _WaveFrequency;
    wave = saturate(wave);

    float spatialDecay = 1.0 - saturate(distanceFromCentre * _Decay);
    float decay = spatialDecay * (1 - age / _WaveLiftTime);

    return wave * _WaveStrength * decay;
}

float ContinuousWave(float2 uv, float2 centre, float startTime)
{
    if(startTime <= 0) return 0;

    float2 offset = uv - centre;
    float distanceFromCentre = length(offset) / _WorldScale;

    // Отдельная скорость для лодки
    float wave = cos(distanceFromCentre * _WaveFrequency - _Time.y * _BoatWaveSpeed) * 0.5 + 0.5;

    float spatialDecay = 1.0 - saturate(distanceFromCentre * _Decay);

    return wave * _WaveStrength * spatialDecay;
}


VertexOutput vert(VertexInput v)
{
    VertexOutput o;
    float combinedWave = 0;

    // импульсные волны (игрок)
    for (int n = 0; n < 10; n++)
    {
        combinedWave += Wave(v.uv, _InputCentre[n].xy, _InputCentre[n].z);
    }

    // постоянная волна (лодка)
    combinedWave += ContinuousWave(v.uv, _ContinuousCentre.xy, _ContinuousCentre.z);


    v.pos.y += combinedWave * 0.5;

    o.pos = UnityObjectToClipPos(v.pos);
    o.rawUV = v.uv;
    o.uv = TRANSFORM_TEX(v.uv, _Texture);

    return o;
}

float4 frag(VertexOutput o) : SV_Target
            {
                float4 tex = tex2D(_Texture, o.uv);
float combinedWave = 0;

for (int n = 0; n < 10; n++)
{
    combinedWave += Wave(o.rawUV, _InputCentre[n].xy, _InputCentre[n].z);
}

combinedWave += ContinuousWave(o.rawUV, _ContinuousCentre.xy, _ContinuousCentre.z);

float4 col = tex + saturate(combinedWave * _Color);
col.a = _Color.a;

return col;
            }

            ENDCG
        }
    }
}