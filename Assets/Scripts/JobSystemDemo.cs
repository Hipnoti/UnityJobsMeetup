using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;

public class JobSystemDemo : MonoBehaviour
{
    [Header("Setup")]
    public GameObject prefab;
    public int spawnCount = 40000;
    public float moveSpeed = 5f;

    [Header("Performance")]
    public bool useJobSystem = true;
    public bool useParallelJob = true;

    private TransformAccessArray transformAccessArray;
    private Transform[] transformReferences;

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
            Vector3 randomPos = new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f), Random.Range(-100f, 100f));
            GameObject instance = Instantiate(prefab, randomPos, Quaternion.identity);
            transformReferences[i] = instance.transform;
        }

        transformAccessArray = new TransformAccessArray(transformReferences);
    }

    void Update()
    {
        if (!useJobSystem)
        {
            UpdateTraditional();
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

    void UpdateTraditional()
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
    }
}
