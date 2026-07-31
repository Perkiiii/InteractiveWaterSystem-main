#ifndef WATER25D_AMBIENT_WAVES_INCLUDED
#define WATER25D_AMBIENT_WAVES_INCLUDED

float EvaluateDirectionalWave(
    float2 worldXZ,
    float2 direction,
    float waveLength,
    float amplitude,
    float speed,
    float time)
{
    direction = normalize(direction + float2(0.0001, 0.0001));
    float phase = dot(worldXZ, direction) * 6.2831853 / max(waveLength, 0.001) + time * speed;
    return sin(phase) * amplitude;
}

float EvaluateWaterAmbientWaves(
    float2 worldXZ,
    float2 direction,
    float waveLength,
    float amplitude,
    float speed,
    float time,
    float bandCount)
{
    float value = EvaluateDirectionalWave(worldXZ, direction, waveLength, amplitude, speed, time);
    if (bandCount > 1.5)
    {
        value += EvaluateDirectionalWave(
            worldXZ + float2(0.0, 0.37),
            float2(direction.y, -direction.x),
            waveLength * 0.61,
            amplitude * 0.35,
            speed * 1.17,
            time + 1.7);
    }
    if (bandCount > 2.5)
    {
        value += EvaluateDirectionalWave(
            worldXZ * 1.31,
            direction + float2(0.23, -0.11),
            waveLength * 1.73,
            amplitude * 0.17,
            speed * 0.73,
            time + 3.1);
    }
    if (bandCount > 3.5)
    {
        value += EvaluateDirectionalWave(
            worldXZ * 0.77,
            direction + float2(-0.19, 0.27),
            waveLength * 0.43,
            amplitude * 0.09,
            speed * 1.61,
            time - 0.8);
    }
    return value;
}

#endif
