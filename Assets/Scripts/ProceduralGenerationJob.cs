using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct ProceduralGenerationJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> positions;
    public NativeArray<float> results;
    
    public int iterations;
    public float scale;

    public void Execute(int index)
    {
        float3 pos = positions[index] * scale;
        float value = 0;
        
        // Heavy calculations: multiple octaves of noise or some complex math
        for (int i = 0; i < iterations; i++)
        {
            float freq = math.pow(2f, i);
            float amp = math.pow(0.5f, i);
            value += noise.snoise(pos * freq) * amp;
        }

        results[index] = value;
    }
}
