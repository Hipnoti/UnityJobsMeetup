using Unity.Burst;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public struct MoveJob : IJobParallelForTransform
{
    public float deltaTime;
    public float speed;

    public void Execute(int index, TransformAccess transform)
    {
        transform.position += transform.rotation * Vector3.forward * speed * deltaTime;
    }
}