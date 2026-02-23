using System.Diagnostics;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ProceduralDemo : MonoBehaviour
{
    [Header("Setup")]
    public int dataSize = 100000;
    public int noiseIterations = 10;
    public float noiseScale = 0.1f;

    [Header("Performance")]
    public bool useJobSystem = true;

    private NativeArray<float3> positions;
    private NativeArray<float> results;

    async void Start()
    {
        positions = new NativeArray<float3>(dataSize, Allocator.Persistent);
        results = new NativeArray<float>(dataSize, Allocator.Persistent);

        for (int i = 0; i < dataSize; i++)
        {
            positions[i] = new float3(i, 0, 0);
        }

        await RunBenchmark();
    }

    async Task RunBenchmark()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        if (useJobSystem)
        {
            ProceduralGenerationJob job = new ProceduralGenerationJob
            {
                positions = positions,
                results = results,
                iterations = noiseIterations,
                scale = noiseScale
            };

            JobHandle handle = job.Schedule(dataSize, 64);
            
            while (!handle.IsCompleted)
            {
                await Task.Yield();
            }
            handle.Complete();
        }
        else
        {
            RunTraditional();
        }

        sw.Stop();
        Debug.Log($"[ProceduralDemo] Execution Time ({(useJobSystem ? "Job System" : "Traditional")}): {sw.Elapsed.TotalMilliseconds:F2} ms");
    }

    void RunTraditional()
    {
        for (int index = 0; index < dataSize; index++)
        {
            float3 pos = positions[index] * noiseScale;
            float value = 0;

            for (int i = 0; i < noiseIterations; i++)
            {
                float freq = math.pow(2f, i);
                float amp = math.pow(0.5f, i);
                value += noise.snoise(pos * freq) * amp;
            }

            results[index] = value;
        }
    }

    void OnDestroy()
    {
        if (positions.IsCreated) positions.Dispose();
        if (results.IsCreated) results.Dispose();
    }
}
