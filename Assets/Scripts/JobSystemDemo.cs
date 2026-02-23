using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public class JobSystemDemo : MonoBehaviour
{
    public enum JobType { Move, Procedural }

    [Header("Setup")]
    public GameObject prefab;
    public int spawnCount = 40000;
    public float moveSpeed = 5f;
    public JobType jobType = JobType.Move;

    [Header("Procedural Settings")]
    public int noiseIterations = 10;
    public float noiseScale = 0.1f;

    [Header("Performance")]
    public bool useJobSystem = true;
    public bool useParallelJob = true;

    private TransformAccessArray transformAccessArray;
    private Transform[] transformReferences;
    private NativeArray<float3> positions;
    private NativeArray<float> results;

    void Start()
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab not assigned to JobSystemDemo!");
            return;
        }

        transformReferences = new Transform[spawnCount];
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 randomPos = new Vector3(UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(-100f, 100f));
            GameObject instance = Instantiate(prefab, randomPos, Quaternion.identity);
            transformReferences[i] = instance.transform;
        }

        transformAccessArray = new TransformAccessArray(transformReferences);
        
        positions = new NativeArray<float3>(spawnCount, Allocator.Persistent);
        results = new NativeArray<float>(spawnCount, Allocator.Persistent);
    }

    void Update()
    {
        if (jobType == JobType.Move)
        {
            UpdateMove();
        }
        else
        {
            UpdateProcedural();
        }
    }

    void UpdateMove()
    {
        if (!useJobSystem)
        {
            UpdateMoveTraditional();
            return;
        }

        MoveJob moveJob = new MoveJob
        {
            deltaTime = Time.deltaTime,
            speed = moveSpeed
        };

        if (useParallelJob)
        {
            JobHandle handle = moveJob.Schedule(transformAccessArray);
            handle.Complete();
        }
        else
        {
            moveJob.RunReadOnly(transformAccessArray);
        }
    }

    void UpdateProcedural()
    {
        // Sync positions for procedural job
        for (int i = 0; i < spawnCount; i++)
        {
            positions[i] = transformReferences[i].position;
        }

        if (!useJobSystem)
        {
            UpdateProceduralTraditional();
            return;
        }

        ProceduralGenerationJob procJob = new ProceduralGenerationJob
        {
            positions = positions,
            results = results,
            iterations = noiseIterations,
            scale = noiseScale
        };

        if (useParallelJob)
        {
            JobHandle handle = procJob.Schedule(spawnCount, 64);
            handle.Complete();
        }
        else
        {
            procJob.Run(spawnCount);
        }
        
        // Use results to do something (e.g. adjust height)
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = transformReferences[i].position;
            pos.y = results[i];
            transformReferences[i].position = pos;
        }
    }

    void UpdateMoveTraditional()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < transformReferences.Length; i++)
        {
            transformReferences[i].position += transformReferences[i].rotation * Vector3.forward * (moveSpeed * dt);
        }
    }

    void UpdateProceduralTraditional()
    {
        for (int index = 0; index < spawnCount; index++)
        {
            float3 pos = (float3)transformReferences[index].position * noiseScale;
            float value = 0;

            for (int i = 0; i < noiseIterations; i++)
            {
                float freq = math.pow(2f, i);
                float amp = math.pow(0.5f, i);
                value += noise.snoise(pos * freq) * amp;
            }

            results[index] = value;
            
            Vector3 finalPos = transformReferences[index].position;
            finalPos.y = results[index];
            transformReferences[index].position = finalPos;
        }
    }

    void OnDestroy()
    {
        if (transformAccessArray.isCreated)
        {
            transformAccessArray.Dispose();
        }

        if (positions.IsCreated) positions.Dispose();
        if (results.IsCreated) results.Dispose();
    }
}
