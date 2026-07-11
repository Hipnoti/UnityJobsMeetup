using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public class JobSystemDemo : MonoBehaviour
{
    [Header("Movement Settings")]
    public GameObject prefab;
    public int spawnCount = 40000;
    public float moveSpeed = 5f;
    
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
        UpdateMove();
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
            moveJob.Schedule(transformAccessArray).Complete();
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
