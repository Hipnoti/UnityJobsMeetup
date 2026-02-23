using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;

public class JobSystemDemo : MonoBehaviour
{
    [Header("Setup")]
    public GameObject prefab;
    public int spawnCount = 40000;
    public float moveSpeed = 5f;

    [Header("Performance")]
    public bool useJobSystem = true;

    private TransformAccessArray transformAccessArray;
    private Transform[] transformReferences;

    struct MoveJob : IJobParallelForTransform
    {
        public float deltaTime;
        public float speed;

        public void Execute(int index, TransformAccess transform)
        {
            transform.position += transform.rotation * Vector3.forward * speed * deltaTime;
        }
    }

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

        JobHandle handle = moveJob.Schedule(transformAccessArray);
        handle.Complete();
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
